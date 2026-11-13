using System.Collections.Generic;
using System.Linq;
using FifteenBraincells.MurderGenerator.Data;

namespace FifteenBraincells.MurderGenerator.Generation
{
    // Step 5: make the answer unique, then make sure every character has something to say.
    internal sealed partial class LevelAttempt
    {
        private const int MaxSpeakerRounds = 10;

        /// <summary>
        /// While the clues still allow another murderer and victim, gives one of those wrong people an
        /// alibi. Fails when the real answer is no longer possible or nobody can get an alibi.
        /// </summary>
        private bool MakeSolutionUnique()
        {
            for (var round = 0; round <= _settings.MaxExtraAlibiRounds; round++)
            {
                var result = Solve();
                if (!result.Remaining.Any(s => s.Is(_suspect, _victim)))
                {
                    return false;
                }

                if (result.IsSolved)
                {
                    return true;
                }

                var wrong = result.Remaining.First(s => !s.Is(_suspect, _victim));
                var toRuleOut = new[] { wrong.Suspect, wrong.Victim }.Where(c => c != _suspect && c != _victim);
                if (!_rng.Shuffled(toRuleOut).Any(c => TryGiveAlibi(c, mustTellItThemselves: false)))
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Fills the open speaker slots, preferring characters who have nothing to say yet. Characters who
        /// stay silent get an alibi they tell themselves, or an extra red herring is added for them to voice.
        /// Adding alibis or red herrings never rules out the real answer, so uniqueness is kept.
        /// </summary>
        private bool GiveEveryoneALine()
        {
            for (var round = 0; round < MaxSpeakerRounds; round++)
            {
                if (!AssignSpeakers())
                {
                    return false;
                }

                var silent = Cast.Where(c => SpokenLineCount(c) == 0).ToList();
                if (silent.Count == 0)
                {
                    return true;
                }

                var changed = false;
                foreach (var character in silent)
                {
                    changed |= TryGiveAlibi(character, mustTellItThemselves: true);
                }

                if (!changed && _redHerrings.Count < _settings.RedHerringsLimit)
                {
                    changed = TryAddRedHerring();
                }

                if (!changed)
                {
                    return false;
                }
            }

            return false;
        }

        private int SpokenLineCount(Character character) => AllClues().Count(c => c.Speaker == character);

        /// <summary>Chooses a character for every open slot. Two passes: silent characters first, then the least busy.</summary>
        private bool AssignSpeakers()
        {
            var counts = Cast.ToDictionary(c => c, c => 0);
            foreach (var clue in AllClues().Where(c => c.FixedSpeaker != null))
            {
                counts[clue.FixedSpeaker]++;
            }

            foreach (var slot in _slots)
            {
                slot.Assigned = null;
            }

            var order = _rng.Shuffled(_slots);
            foreach (var silentOnly in new[] { true, false })
            {
                foreach (var slot in order.Where(s => s.Assigned == null))
                {
                    var candidates = Cast.Where(c => CanSpeak(slot, c)).ToList();
                    if (candidates.Count == 0)
                    {
                        return false;
                    }

                    var lowest = candidates.Min(c => counts[c]);
                    if (silentOnly && lowest > 0)
                    {
                        continue;
                    }

                    var chosen = _rng.Pick(candidates.Where(c => counts[c] == lowest).ToList());
                    slot.Assigned = chosen;
                    counts[chosen]++;
                }
            }

            return true;
        }

        private bool CanSpeak(SpeakerSlot slot, Character character) =>
            !slot.Forbidden.Contains(character)
            && slot.Pair?.Assigned != character
            && !(slot.IsWitness && CannotWitness(BlockedFor(character)));
    }
}
