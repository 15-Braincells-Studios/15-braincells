using System.Collections.Generic;
using System.Linq;
using FifteenBraincells.MurderGenerator.Data;
using FifteenBraincells.MurderGenerator.Output;
using FifteenBraincells.MurderGenerator.Solving;

namespace FifteenBraincells.MurderGenerator.Generation
{
    /// <summary>
    /// One try at building a level. Each step can fail (the data offers no fitting option); then the
    /// whole attempt is dropped and <see cref="LevelGenerator"/> starts a new one.
    /// The steps live in separate files of this partial class, in the order they run:
    /// Crime (method, suspect, victim, motive and method clues), Alibis, Contradiction, RedHerrings, Speakers.
    /// </summary>
    internal sealed partial class LevelAttempt
    {
        private readonly GeneratorDatabase _db;
        private readonly GeneratorSettings _settings;
        private readonly Rng _rng;
        private readonly Method _forcedMethod;

        // Clues that do not change once added. Alibi clues are made from _plans when needed.
        private readonly List<DraftClue> _clues = new List<DraftClue>();
        private readonly List<SpeakerSlot> _slots = new List<SpeakerSlot>();

        // What each character can do today and whether they may still be given an alibi.
        private readonly Dictionary<Character, Need> _blocked = new Dictionary<Character, Need>();
        private readonly HashSet<Character> _mayNotHaveAlibi = new HashSet<Character>();

        private Method _method;
        private PlacedRoom _murderRoom;
        private Character _suspect;
        private Character _victim;
        private Motive _motive;

        public LevelAttempt(GeneratorDatabase db, Rng rng, Method forcedMethod)
        {
            _db = db;
            _settings = db.Settings;
            _rng = rng;
            _forcedMethod = forcedMethod;
        }

        private IReadOnlyList<Character> Cast => _db.Characters;

        private IEnumerable<Character> Innocents => Cast.Where(c => c != _suspect && c != _victim);

        /// <summary>Runs every step. Returns null when a step finds no valid option.</summary>
        public Level TryBuild(int seed, int attemptNumber)
        {
            if (!ChooseCrime())
            {
                return null;
            }

            AddMethodClues();
            AddMotiveClues();

            if (!PlanFakeAlibi())
            {
                return null;
            }

            PlanInnocentAlibis();

            if (!ChooseContradiction())
            {
                return null;
            }

            var redHerrings = _rng.Range(_settings.RedHerringsMin, _settings.RedHerringsMax);
            for (var i = 0; i < redHerrings; i++)
            {
                TryAddRedHerring();
            }

            if (!MakeSolutionUnique() || !GiveEveryoneALine())
            {
                return null;
            }

            return Finish(seed, attemptNumber);
        }

        // ------------------------------------------------------------------ shared helpers

        /// <summary>All clues as they stand now, including one alibi clue per alibi plan.</summary>
        private IEnumerable<DraftClue> AllClues() => _clues.Concat(_plans.Select(AlibiClue));

        private SolverResult Solve() => Solver.Solve(AllClues().Select(c => c.Effect), Cast);

        private Need BlockedFor(Character character) =>
            _blocked.TryGetValue(character, out var blocked) ? blocked : Need.None;

        private void Block(Character character, Need needs) => _blocked[character] = BlockedFor(character) | needs;

        /// <summary>Whether the character can do something that needs these things today.</summary>
        private bool CanDo(Character character, Need needs) =>
            (BlockedFor(character) & needs) == 0
            && ((needs & Need.Lifting) == 0 || character.HasTag(_settings.LiftingRequiresTag));

        private static bool CannotWitness(Need blocked) => (blocked & (Need.Sight | Need.Afternoon)) != 0;

        /// <summary>Adds a clue and creates a speaker slot when the speaker is still unknown.</summary>
        private DraftClue AddClue(ClueCategory category, string sourceId, LineSpec line, Dictionary<string, TextArg> args,
            ClueEffect effect, LineContext context, IEnumerable<Character> mayNotSpeak = null)
        {
            var clue = CreateClue(category, sourceId, line, args, effect, context);
            var kind = line.Speaker.Kind;
            if (kind == SpeakerKind.Witness || kind == SpeakerKind.Explainer)
            {
                clue.Slot = new SpeakerSlot(kind == SpeakerKind.Witness, mayNotSpeak ?? new Character[0]);
                _slots.Add(clue.Slot);
            }

            _clues.Add(clue);
            return clue;
        }

        private DraftClue CreateClue(ClueCategory category, string sourceId, LineSpec line, Dictionary<string, TextArg> args,
            ClueEffect effect, LineContext context) => new DraftClue
            {
                Category = category,
                SourceId = sourceId,
                Line = line,
                FixedSpeaker = FixedSpeaker(line.Speaker, context),
                Room = line.Speaker.IsSpoken ? null : ResolveRoom(line.Room, context),
                Args = args,
                Effect = effect,
            };

        /// <summary>The speaker when it is already known from the data and context; null for open slots.</summary>
        private static Character FixedSpeaker(SpeakerSpec speaker, LineContext context)
        {
            switch (speaker.Kind)
            {
                case SpeakerKind.Fixed: return speaker.Character;
                case SpeakerKind.Self: return context.Holder;
                case SpeakerKind.Partner: return context.Partner;
                default: return null;
            }
        }

        private PlacedRoom ResolveRoom(RoomRef room, LineContext context)
        {
            switch (room.RoomId)
            {
                case RoomRef.MurderPlace:
                    return _murderRoom;
                case RoomRef.AlibiPlace:
                    return AlibiRoom(_fakeAlibi);
                case RoomRef.Home:
                    return HomeOf(context.Owner(room.Owner));
                case RoomRef.Bedroom:
                    return BedroomOf(context.Owner(room.Owner));
                default:
                    return new PlacedRoom(_db.GetRoom(room.RoomId));
            }
        }

        private PlacedRoom HomeOf(Character owner) =>
            owner.HomeRoomId == RoomRef.Bedroom ? BedroomOf(owner) : new PlacedRoom(_db.GetRoom(owner.HomeRoomId));

        private PlacedRoom BedroomOf(Character owner) => new PlacedRoom(_db.GetRoom(RoomRef.Bedroom), owner);

        private static Dictionary<string, TextArg> Args(params (string Name, TextArg Value)[] values) =>
            values.Where(v => v.Value != null).ToDictionary(v => v.Name, v => v.Value);

        private static TextArg Arg(Character character) => character == null ? null : new CharacterArg(character);

        // ------------------------------------------------------------------ finish

        private Level Finish(int seed, int attemptNumber)
        {
            var clues = AllClues().Select(ToClue).ToList();

            // Defensive: the steps above guarantee this, but a level with two answers must never leave the generator.
            var result = Solver.Solve(clues, Cast);
            if (!result.IsSolved || !result.Remaining[0].Is(_suspect, _victim))
            {
                return null;
            }

            var solution = new Solution(_method, _murderRoom, _suspect, _victim, _motive, _fakeAlibi.Alibi, _contradiction);
            var alibis = _plans
                .Select(p => new AlibiAssignment(p.Holder, p.Alibi, p.Partner, p.IsFake))
                .ToList();
            var blocked = _blocked.Where(b => b.Value != Need.None).ToDictionary(b => b.Key, b => b.Value);
            return new Level(seed, attemptNumber, solution, clues, alibis, blocked);
        }

        private static Clue ToClue(DraftClue draft) =>
            new Clue(draft.Category, draft.SourceId, draft.Line.Form, draft.Speaker, draft.Room, draft.Line.TextKey,
                draft.Args, draft.Effect);
    }
}
