using System;
using System.Collections.Generic;
using System.Linq;
using FifteenBraincells.MurderGenerator.Data;
using FifteenBraincells.MurderGenerator.Data.Dto;
using FifteenBraincells.MurderGenerator.Output;
using FifteenBraincells.MurderGenerator.Solving;

namespace FifteenBraincells.MurderGenerator.Generation
{
    /// <summary>Thrown when a hand-built case has problems. The message lists all of them.</summary>
    public sealed class FixedCaseException : Exception
    {
        public FixedCaseException(IReadOnlyList<string> errors)
            : base($"Fixed case has {errors.Count} problem(s):\n- " + string.Join("\n- ", errors))
        {
            Errors = errors;
        }

        public IReadOnlyList<string> Errors { get; }
    }

    /// <summary>
    /// Turns a hand-built case (<see cref="FixedCaseDto"/>) into a <see cref="Level"/>, so a designer can pin one
    /// exact level. Clue texts, speakers, rooms, effects and evidence come from the same data rows the generator
    /// uses; the builder only skips the random choices. It runs the same checks as the generator: the solver must
    /// find exactly the murderer and victim, there must be one clue for each of the five pieces of evidence, and
    /// nobody may say more lines than the settings allow.
    /// </summary>
    public sealed class FixedCaseBuilder
    {
        private readonly GeneratorDatabase _db;
        private readonly FixedCaseDto _dto;
        private readonly List<string> _errors = new List<string>();
        private readonly List<Clue> _clues = new List<Clue>();
        private readonly List<AlibiAssignment> _alibis = new List<AlibiAssignment>();
        private readonly Dictionary<Character, Need> _blocked = new Dictionary<Character, Need>();

        private Method _method;
        private PlacedRoom _murderRoom;
        private ClockTime _murderTime;
        private Character _murderer;
        private Character _victim;
        private Motive _motive;
        private Alibi _fakeAlibi;
        private Contradiction _contradiction;
        private Character _fakeAlibiPartner;

        private FixedCaseBuilder(GeneratorDatabase db, FixedCaseDto dto)
        {
            _db = db;
            _dto = dto;
        }

        /// <exception cref="FixedCaseException">The case has problems; the message lists all of them.</exception>
        public static Level Build(GeneratorDatabase database, FixedCaseDto dto)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            return new FixedCaseBuilder(database, dto).Build();
        }

        private Level Build()
        {
            ReadSolution();
            if (_errors.Count > 0)
            {
                throw new FixedCaseException(_errors);
            }

            // The fake alibi's partner is needed by the contradiction text, so read alibis first.
            var murdererAlibi = _dto.clues.FirstOrDefault(c => c.type == "alibi" && c.character == _murderer.Id && c.id == _fakeAlibi.Id);
            _fakeAlibiPartner = murdererAlibi != null ? FindCharacter(murdererAlibi.partner, "fake alibi partner", optional: true) : null;

            for (var i = 0; i < _dto.clues.Count; i++)
            {
                AddClue(_dto.clues[i], $"clue {i + 1} ({_dto.clues[i].type} '{_dto.clues[i].id}')");
            }

            Check();
            if (_errors.Count > 0)
            {
                throw new FixedCaseException(_errors);
            }

            var solution = new Solution(_method, _murderRoom, _murderTime, null, _murderer, _victim, _motive, _fakeAlibi, _contradiction);
            var blocked = _blocked.Where(b => b.Value != Need.None).ToDictionary(b => b.Key, b => b.Value);
            return new Level(_dto.number, 0, solution, _clues, _alibis, blocked, new List<PlanAssignment>());
        }

        // ------------------------------------------------------------------ solution

        private void ReadSolution()
        {
            if (!_db.TryGetMethod(_dto.method, out _method))
            {
                _errors.Add($"Unknown method '{_dto.method}'.");
                return;
            }

            if (_method.Kind != MethodKind.Fixed)
            {
                _errors.Add($"Method '{_method.Id}' is a {_method.Kind} method. Fixed cases only support Fixed methods for now.");
            }

            var roomId = _dto.murderRoom.Length > 0 ? _dto.murderRoom : _method.RoomIds.Count == 1 ? _method.RoomIds[0] : "";
            if (roomId.Length == 0)
            {
                _errors.Add($"Method '{_method.Id}' can happen in several rooms ({string.Join(", ", _method.RoomIds)}); set murderRoom.");
            }
            else if (!_method.RoomIds.Contains(roomId))
            {
                _errors.Add($"Method '{_method.Id}' cannot happen in '{roomId}'. Options: {string.Join(", ", _method.RoomIds)}.");
            }
            else
            {
                _murderRoom = new PlacedRoom(_db.GetRoom(roomId));
            }

            if (!ClockTime.TryParse(_dto.murderTime, out _murderTime))
            {
                _errors.Add($"murderTime '{_dto.murderTime}' is not a time like 18:05.");
            }
            else if (_method.Kind == MethodKind.Fixed
                     && (_murderTime.Minutes < _method.WindowStart.Minutes || _murderTime.Minutes > _method.WindowEnd.Minutes))
            {
                _errors.Add($"murderTime {_murderTime} is outside the method's window ({_method.WindowStart}-{_method.WindowEnd}).");
            }

            _murderer = FindCharacter(_dto.murderer, "murderer");
            _victim = FindCharacter(_dto.victim, "victim");
            if (_murderer != null && !_method.Suspects.Matches(_murderer))
            {
                _errors.Add($"'{_murderer.Id}' cannot be the murderer with method '{_method.Id}'.");
            }

            if (_victim != null && !_method.Victims.Matches(_victim))
            {
                _errors.Add($"'{_victim.Id}' cannot be the victim of method '{_method.Id}'.");
            }

            _motive = Find(_db.Motives, m => m.Id, _dto.motive, "motive");
            if (_motive != null && _murderer != null && _victim != null && !_motive.Fits(_murderer, _victim))
            {
                _errors.Add($"Motive '{_motive.Id}' does not fit {_murderer.Id} against {_victim.Id}.");
            }

            _fakeAlibi = Find(_db.Alibis, a => a.Id, _dto.fakeAlibi, "fake alibi");
            _contradiction = Find(_db.Contradictions, c => c.Id, _dto.contradiction, "contradiction");
            if (_contradiction != null && _contradiction.IsSpecific && _contradiction.Alibi != _fakeAlibi)
            {
                _errors.Add($"Contradiction '{_contradiction.Id}' is written for alibi '{_contradiction.Alibi.Id}', not '{_dto.fakeAlibi}'.");
            }
        }

        // ------------------------------------------------------------------ clues

        private void AddClue(FixedClueDto dto, string where)
        {
            switch (dto.type)
            {
                case "method":
                    AddMethodClue(dto, where);
                    break;
                case "motive":
                    AddMotiveClue(dto, where);
                    break;
                case "alibi":
                    AddAlibiClue(dto, where);
                    break;
                case "contradiction":
                    AddContradictionClue(dto, where);
                    break;
                case "herring":
                    AddHerring(dto, where);
                    break;
                default:
                    _errors.Add($"{where}: unknown type '{dto.type}'. Use method, motive, alibi, contradiction or herring.");
                    break;
            }
        }

        private void AddMethodClue(FixedClueDto dto, string where)
        {
            var clue = Find(_db.MethodClues, c => c.Id, dto.id, where + ": method clue");
            if (clue == null)
            {
                return;
            }

            if (clue.Method != _method)
            {
                _errors.Add($"{where}: belongs to method '{clue.Method.Id}', not '{_method.Id}'.");
                return;
            }

            var role = clue.EffectRole;
            var args = new Dictionary<string, TextArg>
            {
                ["murder_place"] = new RoomArg(_murderRoom),
                ["murder_time"] = new TimeArg(_murderTime),
                ["victim"] = new CharacterArg(_victim),
            };
            var effect = ClueEffect.Nothing;
            var named = dto.characters.Select(id => FindCharacter(id, where)).Where(c => c != null).ToList();

            switch (clue.Effect)
            {
                case MethodClueEffect.FullList:
                    var all = _db.Characters.Where(_method.PoolFor(role).Matches).ToList();
                    var shown = clue.ListOmitsSpeaker ? all.Where(c => c != clue.Line.Speaker.Character).ToList() : all;
                    args["list"] = new CharacterListArg(shown);
                    effect = ClueEffect.OnlyThese(role, all);
                    break;

                case MethodClueEffect.PartialList:
                    if (named.Count == 0)
                    {
                        _errors.Add($"{where}: needs 'characters' for its (partial list).");
                    }

                    args["partial_list"] = new CharacterListArg(named);
                    effect = ClueEffect.OnlyThese(role, named);
                    break;

                case MethodClueEffect.Excluded:
                    if (named.Count == 0)
                    {
                        _errors.Add($"{where}: needs 'characters' for its (excluded).");
                    }

                    args["excluded"] = new CharacterListArg(named);
                    effect = ClueEffect.NotThese(role, named);
                    foreach (var character in named)
                    {
                        _blocked[character] = (_blocked.TryGetValue(character, out var b) ? b : Need.None) | clue.Blocks;
                    }

                    break;

                case MethodClueEffect.ExcludeFixed:
                    effect = ClueEffect.NotThese(role, clue.EffectCharacters);
                    break;

                case MethodClueEffect.OnlyFixed:
                    effect = ClueEffect.OnlyThese(role, clue.EffectCharacters);
                    break;
            }

            var evidence = clue.Group == ClueGroup.WhenAndWhere ? EvidenceRole.WhenAndWhere
                : clue.Group == ClueGroup.Method ? EvidenceRole.Method
                : EvidenceRole.None;
            var speaker = Speaker(clue.Line, dto.speaker, where, null, null, _murderer, _victim);
            Add(ClueCategory.Method, clue.Id, clue.Line, speaker, ObjectRoom(clue.Line, speaker, null, null), args, effect,
                null, false, evidence);
        }

        private void AddMotiveClue(FixedClueDto dto, string where)
        {
            var motive = Find(_db.Motives, m => m.Id, dto.id, where + ": motive");
            var suspect = FindCharacter(dto.suspect, where + ": suspect");
            var victim = FindCharacter(dto.victim, where + ": victim");
            if (motive == null || suspect == null || victim == null)
            {
                return;
            }

            if (!motive.Fits(suspect, victim))
            {
                _errors.Add($"{where}: does not fit {suspect.Id} against {victim.Id}.");
            }

            var isReal = motive == _motive && suspect == _murderer && victim == _victim;
            var args = new Dictionary<string, TextArg> { ["suspect"] = new CharacterArg(suspect), ["victim"] = new CharacterArg(victim) };
            var speaker = Speaker(motive.Clue, dto.speaker, where, null, null, suspect, victim);
            Add(ClueCategory.Motive, motive.Id, motive.Clue, speaker, ObjectRoom(motive.Clue, speaker, null, null, suspect, victim),
                args, ClueEffect.Motive(suspect, victim), null, false, isReal ? EvidenceRole.Motive : EvidenceRole.None);
        }

        private void AddAlibiClue(FixedClueDto dto, string where)
        {
            var alibi = Find(_db.Alibis, a => a.Id, dto.id, where + ": alibi");
            var holder = FindCharacter(dto.character, where + ": character");
            if (alibi == null || holder == null)
            {
                return;
            }

            var index = Math.Max(dto.variant, 1) - 1;
            if (index >= alibi.Variants.Count)
            {
                _errors.Add($"{where}: has {alibi.Variants.Count} variant(s), not {dto.variant}.");
                return;
            }

            var variant = alibi.Variants[index];
            var partner = FindCharacter(dto.partner, where + ": partner", optional: true);
            if (!alibi.Holders.Matches(holder))
            {
                _errors.Add($"{where}: '{holder.Id}' may not hold this alibi.");
            }

            if (variant.HolderTag.Length > 0 && !holder.HasTag(variant.HolderTag))
            {
                _errors.Add($"{where}: variant {index + 1} needs a holder with tag '{variant.HolderTag}'.");
            }

            if ((alibi.Partner.Kind == PartnerKind.None) != (partner == null))
            {
                _errors.Add(partner == null ? $"{where}: this alibi needs a 'partner'." : $"{where}: this alibi has no partner.");
            }

            var isFake = holder == _murderer;

            // The partner of the fake alibi is part of the lie, so they may really be somewhere else tonight.
            var taken = _alibis.SelectMany(a => a.IsFake ? new[] { a.Holder } : new[] { a.Holder, a.Partner }).ToList();
            foreach (var character in isFake ? new[] { holder } : new[] { holder, partner })
            {
                if (character != null && taken.Contains(character))
                {
                    _errors.Add($"{where}: '{character.Id}' already has an alibi.");
                }
            }

            if (isFake && alibi != _fakeAlibi)
            {
                _errors.Add($"{where}: the murderer's alibi must be the fake alibi '{_fakeAlibi.Id}'.");
            }

            if (holder == _victim)
            {
                _errors.Add($"{where}: the victim cannot have an alibi.");
            }

            if (!alibi.Covers(_murderTime, _method, _murderRoom.Room.Id, holder))
            {
                _errors.Add($"{where}: does not put '{holder.Id}' elsewhere at {_murderTime} ({alibi.Start}-{alibi.End}).");
            }

            var args = new Dictionary<string, TextArg> { ["character"] = new CharacterArg(holder) };
            if (partner != null)
            {
                args[alibi.Partner.Placeholder] = new CharacterArg(partner);
            }

            if (alibi.RequiresOtherHolderOf != null)
            {
                var sick = FindCharacter(dto.sickCharacter, where + ": sickCharacter");
                if (sick != null)
                {
                    args["sick_character"] = new CharacterArg(sick);
                }
            }

            var effect = isFake
                ? ClueEffect.FakeAlibi(holder)
                : ClueEffect.Alibi(partner == null ? new[] { holder } : new[] { holder, partner });
            var speaker = Speaker(variant.Line, dto.speaker, where, holder, partner, null, null);
            Add(ClueCategory.Alibi, alibi.Id, variant.Line, speaker, ObjectRoom(variant.Line, speaker, holder, null), args, effect,
                holder, false, isFake ? EvidenceRole.FakeAlibi : EvidenceRole.None);
            _alibis.Add(new AlibiAssignment(holder, alibi, partner, isFake));
        }

        private void AddContradictionClue(FixedClueDto dto, string where)
        {
            var contradiction = Find(_db.Contradictions, c => c.Id, dto.id, where + ": contradiction");
            if (contradiction == null)
            {
                return;
            }

            var isReal = contradiction == _contradiction;
            var args = new Dictionary<string, TextArg>
            {
                ["murder_place"] = new RoomArg(_murderRoom),
                ["murder_time"] = new TimeArg(_murderTime),
            };
            if (isReal)
            {
                args["suspect"] = new CharacterArg(_murderer);
                if (_fakeAlibiPartner != null)
                {
                    args["partner"] = new CharacterArg(_fakeAlibiPartner);
                }

                args["alibi_place"] = _fakeAlibi.IsOutside
                    ? new TextKeyArg(_fakeAlibi.PlaceKey)
                    : (TextArg)new RoomArg(PlaceOf(_fakeAlibi, _murderer));
            }

            if (dto.otherRoom.Length > 0)
            {
                var room = Find(_db.Rooms, r => r.Id, dto.otherRoom, where + ": otherRoom");
                if (room != null)
                {
                    args["other_place"] = new RoomArg(new PlacedRoom(room));
                }
            }

            var speaker = Speaker(contradiction.Line, dto.speaker, where, null, null, isReal ? _murderer : null, null);
            Add(ClueCategory.Contradiction, contradiction.Id, contradiction.Line, speaker,
                ObjectRoom(contradiction.Line, speaker, null, null, _murderer), args,
                isReal ? ClueEffect.BreaksFakeAlibi(_murderer) : ClueEffect.Nothing,
                null, !isReal, isReal ? EvidenceRole.Contradiction : EvidenceRole.None);
        }

        /// <summary>A red herring is two clues: the suspicion and its explanation.</summary>
        private void AddHerring(FixedClueDto dto, string where)
        {
            var herring = Find(_db.RedHerrings, h => h.Id, dto.id, where + ": red herring");
            var subject = FindCharacter(dto.character, where + ": character");
            if (herring == null || subject == null)
            {
                return;
            }

            if (subject == _murderer || subject == _victim)
            {
                _errors.Add($"{where}: a red herring is never about the murderer or the victim.");
            }

            if (!herring.Subjects.Matches(subject))
            {
                _errors.Add($"{where}: cannot be about '{subject.Id}'.");
            }

            var args = new Dictionary<string, TextArg> { ["character"] = new CharacterArg(subject) };
            var suspicion = herring.Suspicion.Line;
            var witness = Speaker(suspicion, dto.speaker, where + " suspicion", null, null, subject, null);
            Add(ClueCategory.RedHerring, herring.Id, suspicion, witness, ObjectRoom(suspicion, witness, null, subject), args,
                ClueEffect.Nothing, null, false, EvidenceRole.None);

            var explanation = herring.Explanation.Line;
            var explainerId = dto.explainer;
            if (explainerId.Length > 0 && explanation.Speaker.Kind == SpeakerKind.Fixed
                && herring.Explanation.FixedExplainerOptions.Any(c => c.Id == explainerId))
            {
                // The data allows a named character instead of the line's own speaker.
                var named = _db.GetCharacter(explainerId);
                Add(ClueCategory.RedHerring, herring.Id, explanation, named, null, args, ClueEffect.Nothing, null, false, EvidenceRole.None);
                return;
            }

            var explainer = Speaker(explanation, explainerId, where + " explanation", null, null, subject, null);
            if (explainer != null && explainer == witness)
            {
                _errors.Add($"{where}: the suspicion and the explanation need two different speakers.");
            }

            Add(ClueCategory.RedHerring, herring.Id, explanation, explainer, ObjectRoom(explanation, explainer, null, subject), args,
                ClueEffect.Nothing, null, false, EvidenceRole.None);
        }

        private void Add(ClueCategory category, string sourceId, LineSpec line, Character speaker, PlacedRoom room,
            Dictionary<string, TextArg> args, ClueEffect effect, Character about, bool isFalseLead, EvidenceRole evidence)
        {
            _clues.Add(new Clue(category, sourceId, line.Form, speaker, room, line.TextKey, args, effect, about, isFalseLead, evidence));
        }

        // ------------------------------------------------------------------ speakers and rooms

        /// <summary>The speaker of a line, or null for an object. Open speakers come from the case file.</summary>
        private Character Speaker(LineSpec line, string given, string where, Character self, Character partner,
            Character notThis, Character norThis)
        {
            var spec = line.Speaker;
            switch (spec.Kind)
            {
                case SpeakerKind.None:
                    if (given.Length > 0)
                    {
                        _errors.Add($"{where}: is an object, so it has no speaker (remove '{given}').");
                    }

                    return null;

                case SpeakerKind.Fixed:
                    if (given.Length > 0 && given != spec.Character.Id)
                    {
                        _errors.Add($"{where}: is always said by '{spec.Character.Id}', not '{given}'.");
                    }

                    return spec.Character;

                case SpeakerKind.Self:
                    return self;

                case SpeakerKind.Partner:
                    return partner;

                default:
                    var speaker = FindCharacter(given, where + ": speaker (the data leaves it open)");
                    if (speaker != null && (speaker == notThis || speaker == norThis))
                    {
                        _errors.Add($"{where}: '{speaker.Id}' cannot say this line about themselves.");
                    }

                    return speaker;
            }
        }

        /// <summary>Where an object lies. Null for spoken lines.</summary>
        private PlacedRoom ObjectRoom(LineSpec line, Character speaker, Character holder, Character subject,
            Character suspect = null, Character victim = null)
        {
            if (line.Speaker.IsSpoken)
            {
                return null;
            }

            var room = line.Room;
            if (!room.IsSet)
            {
                return null;
            }

            Character Owner()
            {
                switch (room.Owner)
                {
                    case RoomOwner.Suspect: return suspect ?? _murderer;
                    case RoomOwner.Victim: return victim ?? _victim;
                    case RoomOwner.Subject: return subject;
                    case RoomOwner.Holder: return holder;
                    default: return null;
                }
            }

            switch (room.RoomId)
            {
                case RoomRef.MurderPlace:
                    return _murderRoom;
                case RoomRef.AlibiPlace:
                    return PlaceOf(_fakeAlibi, _murderer);
                case RoomRef.Bedroom:
                    return new PlacedRoom(_db.GetRoom(RoomRef.Bedroom), Owner());
                case RoomRef.Home:
                    var owner = Owner();
                    return owner.HomeRoomId == RoomRef.Bedroom
                        ? new PlacedRoom(_db.GetRoom(RoomRef.Bedroom), owner)
                        : new PlacedRoom(_db.GetRoom(_db.IsClosed(owner.HomeRoomId) ? "staff_room" : owner.HomeRoomId));
                default:
                    return new PlacedRoom(_db.GetRoom(room.RoomId));
            }
        }

        private PlacedRoom PlaceOf(Alibi alibi, Character holder) =>
            alibi.IsOutside ? null
            : alibi.RoomId == RoomRef.Bedroom ? new PlacedRoom(_db.GetRoom(RoomRef.Bedroom), holder)
            : new PlacedRoom(_db.GetRoom(alibi.RoomFor(holder)));

        // ------------------------------------------------------------------ checks

        private void Check()
        {
            var result = Solver.Solve(_clues, _db.Characters);
            if (!result.IsSolved || !result.Remaining[0].Is(_murderer, _victim))
            {
                var left = result.Remaining.Count == 0
                    ? "nobody"
                    : string.Join(", ", result.Remaining.Select(r => $"{r.Suspect.Id} -> {r.Victim.Id}"));
                _errors.Add($"The clues do not lead to exactly {_murderer.Id} -> {_victim.Id}. A player is left with: {left}.");
            }

            var roles = _clues.Where(c => c.IsEvidence).Select(c => c.Evidence).ToList();
            foreach (EvidenceRole role in Enum.GetValues(typeof(EvidenceRole)))
            {
                if (role == EvidenceRole.None)
                {
                    continue;
                }

                var count = roles.Count(r => r == role);
                if (count != 1)
                {
                    _errors.Add($"Needs exactly one clue for evidence {role}, found {count}.");
                }
            }

            foreach (var character in _db.Characters)
            {
                var lines = _clues.Count(c => c.Speaker == character);
                if (lines > _db.Settings.MaxLinesPerCharacter)
                {
                    _errors.Add($"'{character.Id}' says {lines} lines; the most is {_db.Settings.MaxLinesPerCharacter}.");
                }
            }
        }

        // ------------------------------------------------------------------ lookups

        private Character FindCharacter(string id, string what, bool optional = false)
        {
            if (string.IsNullOrEmpty(id))
            {
                if (!optional)
                {
                    _errors.Add($"{what}: no character given.");
                }

                return null;
            }

            var character = _db.Characters.FirstOrDefault(c => c.Id == id);
            if (character == null)
            {
                _errors.Add($"{what}: unknown character '{id}'.");
            }

            return character;
        }

        private T Find<T>(IEnumerable<T> items, Func<T, string> idOf, string id, string what) where T : class
        {
            var item = items.FirstOrDefault(x => idOf(x) == id);
            if (item == null)
            {
                _errors.Add($"{what}: unknown id '{id}'.");
            }

            return item;
        }
    }
}
