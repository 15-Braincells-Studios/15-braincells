using System.Collections.Generic;
using System.Linq;
using FifteenBraincells.MurderGenerator.Data;
using FifteenBraincells.MurderGenerator.Output;

namespace FifteenBraincells.MurderGenerator.Generation
{
    // Step 3: the clue that breaks the suspect's fake alibi, and loose contradictions that break nothing.
    internal sealed partial class LevelAttempt
    {
        private const string GeneralAnyTag = "Any";
        private const string GeneralInsideTag = "Inside";
        private const string GeneralOutsideTag = "Outside";

        private readonly List<Contradiction> _loose = new List<Contradiction>();
        private Contradiction _contradiction;

        /// <summary>
        /// Picks a contradiction for the fake alibi. When an alibi-specific one fits, it is used with
        /// <see cref="GeneratorSettings.SpecificContradictionChance"/>; otherwise a general one.
        /// </summary>
        private bool ChooseContradiction()
        {
            bool Usable(Contradiction c) => ContradictionRulesHold(c) && !IsFull(c.Line.Speaker.Character);
            var specific = _db.Contradictions.Where(c => c.IsSpecific && c.Alibi == _fakeAlibi.Alibi && Usable(c)).ToList();
            var general = _db.Contradictions.Where(c => !c.IsSpecific && FitsFakeAlibiTag(c) && Usable(c)).ToList();
            if (specific.Count == 0 && general.Count == 0)
            {
                return false;
            }

            var useSpecific = specific.Count > 0 && (general.Count == 0 || _rng.Chance(_settings.SpecificContradictionChance));
            _contradiction = _rng.Pick(useSpecific ? specific : general);
            AddContradictionClue();
            return true;
        }

        private bool FitsFakeAlibiTag(Contradiction contradiction) =>
            contradiction.AlibiTag == GeneralAnyTag
            || (contradiction.AlibiTag == GeneralOutsideTag && _fakeAlibi.Alibi.IsOutside)
            || (contradiction.AlibiTag == GeneralInsideTag && !_fakeAlibi.Alibi.IsOutside);

        private IEnumerable<PlacedRoom> RealAlibiRooms => RealPlans.Select(p => PlaceOf(p.Alibi, p.Holder)).Where(r => r != null);

        /// <summary>
        /// Every condition from the Excel "Condition" column. Checked again whenever an alibi is added
        /// later, so a later alibi can never make the contradiction untrue.
        /// </summary>
        private bool ContradictionRulesHold(Contradiction contradiction)
        {
            var rules = contradiction.Rules;
            var fake = _fakeAlibi.Alibi;
            var fakeRoom = PlaceOf(fake, _suspect);
            var others = RealPlans.Select(p => p.Alibi).ToList();

            return !rules.NotSuspect.Contains(_suspect)
                   && (!rules.AllOtherAlibisInside || others.All(a => !a.IsOutside))
                   && (!rules.NoOtherAlibiAtAlibiPlace || (fakeRoom != null && !RealAlibiRooms.Any(r => SameRoom(r, fakeRoom))))
                   && (rules.NoOtherAlibiInRoom == null || RealAlibiRooms.All(r => r.Room != rules.NoOtherAlibiInRoom))
                   && (!rules.NoOtherAlibiInBathroom || RealAlibiRooms.All(r => !RoomRef.IsBathroom(r.Room.Id)))
                   && (!rules.OnlyForBathroomAlibis || (fakeRoom != null && RoomRef.IsBathroom(fakeRoom.Room.Id)))
                   && (!rules.NoOtherOutsideAlibi || others.All(a => !a.IsOutside))
                   && (rules.NoOtherHolderOf == null || others.All(a => a != rules.NoOtherHolderOf))
                   && rules.SuspectHasTags.All(_suspect.HasTag)
                   && (rules.OnlyForAlibis.Count == 0 || rules.OnlyForAlibis.Contains(fake))
                   && !rules.NotForAlibis.Contains(fake)
                   && !rules.NotWithMethods.Contains(_method)
                   && !(rules.NotForOutsideAlibis && fake.IsOutside)
                   && (contradiction.Line.Speaker.Kind != SpeakerKind.Partner || _fakeAlibi.Partner != null);
        }

        private void AddContradictionClue()
        {
            var fake = _fakeAlibi.Alibi;
            var alibiRoom = AlibiRoom(_fakeAlibi);
            var otherRooms = _db.Rooms
                .Where(r => r.Id != RoomRef.Bedroom && r.Id != _murderRoom.Room.Id && r.Id != alibiRoom?.Room.Id && !_db.IsClosed(r.Id))
                .ToList();

            var args = Args(
                ("suspect", Arg(_suspect)),
                ("partner", Arg(_fakeAlibi.Partner)),
                ("murder_place", new RoomArg(_murderRoom)),
                ("murder_time", new TimeArg(_murderTime)),
                ("alibi_place", fake.IsOutside ? new TextKeyArg(fake.PlaceKey) : (TextArg)new RoomArg(alibiRoom)),
                ("other_place", new RoomArg(new PlacedRoom(_rng.Pick(otherRooms)))));

            var context = new LineContext { Suspect = _suspect, Holder = _suspect, Partner = _fakeAlibi.Partner };
            var clue = AddClue(ClueCategory.Contradiction, _contradiction.Id, _contradiction.Line, args,
                ClueEffect.BreaksFakeAlibi(_suspect), context, mayNotSpeak: new[] { _suspect });
            clue.About = null;
            clue.Evidence = EvidenceRole.Contradiction;
        }

        // ------------------------------------------------------------------ loose contradictions

        /// <summary>
        /// Adds one or more alibi-specific contradictions about an alibi nobody has tonight. They are true and
        /// refute nothing: a false lead. Their own conditions still hold for the level.
        /// </summary>
        private void AddLooseContradictions()
        {
            var count = _settings.LooseContradictionsMin;
            while (count < _settings.LooseContradictionsMax && _rng.Chance(_settings.SecondLooseContradictionChance))
            {
                count++;
            }

            var candidates = _db.Contradictions
                .Where(c => c.IsSpecific && !c.MentionsSuspect && c != _contradiction)
                .ToList();
            foreach (var candidate in _rng.Shuffled(candidates))
            {
                if (_loose.Count >= count)
                {
                    break;
                }

                if (!IsFull(candidate.Line.Speaker.Character) && LooseRulesHold(candidate))
                {
                    _loose.Add(candidate);
                    AddLooseClue(candidate);
                }
            }
        }

        /// <summary>A loose contradiction stays true: nobody has its alibi and its conditions hold.</summary>
        private bool LooseRulesHold(Contradiction contradiction)
        {
            var rules = contradiction.Rules;
            var rooms = RealAlibiRooms.Concat(_plans.Where(p => p.IsFake).Select(p => PlaceOf(p.Alibi, p.Holder)).Where(r => r != null)).ToList();
            return !IsHeldTonight(contradiction.Alibi)
                   && !rules.NotSuspect.Contains(_suspect)
                   && !rules.NotWithMethods.Contains(_method)
                   && (rules.NoOtherHolderOf == null || !IsHeldTonight(rules.NoOtherHolderOf))
                   && (rules.NoOtherAlibiInRoom == null || (rooms.All(r => r.Room != rules.NoOtherAlibiInRoom) && _murderRoom.Room != rules.NoOtherAlibiInRoom))
                   && (!rules.NoOtherAlibiInBathroom || rooms.All(r => !RoomRef.IsBathroom(r.Room.Id)))
                   && (!rules.NoOtherOutsideAlibi || _plans.All(p => !p.Alibi.IsOutside))
                   && (!rules.AllOtherAlibisInside || _plans.All(p => !p.Alibi.IsOutside));
        }

        private void AddLooseClue(Contradiction contradiction)
        {
            var args = Args(("murder_place", new RoomArg(_murderRoom)), ("murder_time", new TimeArg(_murderTime)));
            var clue = AddClue(ClueCategory.Contradiction, contradiction.Id, contradiction.Line, args, ClueEffect.Nothing,
                new LineContext());
            clue.IsFalseLead = true;
        }
    }
}
