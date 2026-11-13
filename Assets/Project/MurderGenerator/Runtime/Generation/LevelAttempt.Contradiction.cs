using System.Linq;
using FifteenBraincells.MurderGenerator.Data;
using FifteenBraincells.MurderGenerator.Output;

namespace FifteenBraincells.MurderGenerator.Generation
{
    // Step 3: the clue that breaks the suspect's fake alibi.
    internal sealed partial class LevelAttempt
    {
        private const string GeneralAnyTag = "Any";
        private const string GeneralInsideTag = "Inside";
        private const string GeneralOutsideTag = "Outside";

        private Contradiction _contradiction;

        /// <summary>
        /// Picks a contradiction for the fake alibi. When an alibi-specific one fits, it is used with
        /// <see cref="GeneratorSettings.SpecificContradictionChance"/>; otherwise a general one.
        /// </summary>
        private bool ChooseContradiction()
        {
            var specific = _db.Contradictions.Where(c => c.IsSpecific && c.Alibi == _fakeAlibi.Alibi && ContradictionRulesHold(c)).ToList();
            var general = _db.Contradictions.Where(c => !c.IsSpecific && FitsFakeAlibiTag(c) && ContradictionRulesHold(c)).ToList();
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

        /// <summary>
        /// Every condition from the Excel "Condition" column. Checked again whenever an alibi is added
        /// later, so a later alibi can never make the contradiction untrue.
        /// </summary>
        private bool ContradictionRulesHold(Contradiction contradiction)
        {
            var rules = contradiction.Rules;
            var fake = _fakeAlibi.Alibi;
            var others = RealPlans.Select(p => p.Alibi).ToList();

            return !rules.NotSuspect.Contains(_suspect)
                   && (!rules.AllOtherAlibisInside || others.All(a => !a.IsOutside))
                   && (!rules.NoOtherAlibiAtAlibiPlace || (!fake.IsOutside && others.All(a => a.IsOutside || a.RoomId != fake.RoomId)))
                   && (rules.NoOtherAlibiInRoom == null || others.All(a => a.RoomId != rules.NoOtherAlibiInRoom.Id))
                   && (!rules.NoOtherOutsideAlibi || others.All(a => !a.IsOutside))
                   && (rules.NoOtherHolderOf == null || others.All(a => a != rules.NoOtherHolderOf))
                   && rules.SuspectHasTags.All(_suspect.HasTag)
                   && (rules.OnlyForAlibis.Count == 0 || rules.OnlyForAlibis.Contains(fake))
                   && !rules.NotForAlibis.Contains(fake)
                   && !rules.NotWithMethods.Contains(_method)
                   && (contradiction.Line.Speaker.Kind != SpeakerKind.Partner || _fakeAlibi.Partner != null);
        }

        private void AddContradictionClue()
        {
            var fake = _fakeAlibi.Alibi;
            var alibiRoom = AlibiRoom(_fakeAlibi);
            var otherRooms = _db.Rooms
                .Where(r => r.Id != RoomRef.Bedroom && r.Id != _murderRoom.Room.Id && r.Id != alibiRoom?.Room.Id)
                .ToList();

            var args = Args(
                ("suspect", Arg(_suspect)),
                ("partner", Arg(_fakeAlibi.Partner)),
                ("murder_place", new RoomArg(_murderRoom)),
                ("murder_time", new TimeArg(_method.Time)),
                ("alibi_place", fake.IsOutside ? new TextKeyArg(fake.PlaceKey) : (TextArg)new RoomArg(alibiRoom)),
                ("other_place", new RoomArg(new PlacedRoom(_rng.Pick(otherRooms)))));

            var context = new LineContext { Suspect = _suspect, Holder = _suspect, Partner = _fakeAlibi.Partner };
            AddClue(ClueCategory.Contradiction, _contradiction.Id, _contradiction.Line, args,
                ClueEffect.BreaksFakeAlibi(_suspect), context, mayNotSpeak: new[] { _suspect });
        }
    }
}
