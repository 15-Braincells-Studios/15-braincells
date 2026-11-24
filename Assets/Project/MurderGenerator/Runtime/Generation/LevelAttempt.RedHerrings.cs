using System.Collections.Generic;
using System.Linq;
using FifteenBraincells.MurderGenerator.Data;
using FifteenBraincells.MurderGenerator.Output;

namespace FifteenBraincells.MurderGenerator.Generation
{
    // Step 4: red herrings, things that make an innocent look suspicious, each with an explanation.
    internal sealed partial class LevelAttempt
    {
        /// <summary>For "Doctor or (Explainer)": how often the named character explains instead of a random one.</summary>
        private const float FixedExplainerChance = 0.5f;

        private readonly List<(RedHerring Herring, Character Subject)> _redHerrings = new List<(RedHerring, Character)>();

        /// <summary>Adds one unused red herring about a fitting innocent character.</summary>
        private bool TryAddRedHerring()
        {
            foreach (var herring in _rng.Shuffled(_db.RedHerrings))
            {
                if (_redHerrings.Any(r => r.Herring == herring))
                {
                    continue;
                }

                var subjects = Innocents.Where(c => IsPossibleSubject(herring, c)).ToList();
                if (subjects.Count > 0)
                {
                    AddRedHerring(herring, _rng.Pick(subjects));
                    return true;
                }
            }

            return false;
        }

        private bool IsPossibleSubject(RedHerring herring, Character character) =>
            herring.Subjects.Matches(character)
            && CanDo(character, herring.Needs)
            // Someone with an alibi or plan for tonight never gets a red herring about tonight.
            && ((herring.Needs & Need.Evening) == 0 || (!_busy.ContainsKey(character) && !_planHolders.ContainsKey(character)))
            && !IsFull(herring.Suspicion.Line.Speaker.Character)
            && !IsFull(herring.Explanation.Line.Speaker.Character)
            && !(herring.Suspicion.Line.Speaker.Character != null
                 && herring.Suspicion.Line.Speaker.Character == herring.Explanation.Line.Speaker.Character
                 && FixedLineCount(herring.Suspicion.Line.Speaker.Character) >= _settings.MaxLinesPerCharacter - 1)
            && _redHerrings.All(r => r.Subject != character)
            && herring.Suspicion.Line.Speaker.Character != character
            && herring.Explanation.Line.Speaker.Character != character;

        private void AddRedHerring(RedHerring herring, Character subject)
        {
            _redHerrings.Add((herring, subject));
            if ((herring.Needs & Need.Evening) != 0)
            {
                _eveningSubjects.Add(subject);
            }

            var context = new LineContext { Subject = subject };

            var explanationLine = herring.Explanation.Line;
            var explainers = herring.Explanation.FixedExplainerOptions.Where(c => c != subject && !IsFull(c)).ToList();
            Character fixedExplainer = null;
            if (explainers.Count > 0 && _rng.Chance(FixedExplainerChance))
            {
                fixedExplainer = _rng.Pick(explainers);
            }

            var witnessSpeaker = herring.Suspicion.Line.Speaker.Character;
            var explainerSpeaker = fixedExplainer ?? explanationLine.Speaker.Character;

            var suspicion = AddClue(ClueCategory.RedHerring, herring.Id, herring.Suspicion.Line,
                Args(("character", Arg(subject))), ClueEffect.Nothing, context,
                mayNotSpeak: new[] { subject, explainerSpeaker });

            var explanation = AddClue(ClueCategory.RedHerring, herring.Id, explanationLine,
                Args(("character", Arg(subject))), ClueEffect.Nothing, context,
                mayNotSpeak: new[] { subject, witnessSpeaker });
            if (fixedExplainer != null)
            {
                // The data allowed a named character instead of an open explainer slot.
                _slots.Remove(explanation.Slot);
                explanation.Slot = null;
                explanation.FixedSpeaker = fixedExplainer;
            }

            if (suspicion.Slot != null && explanation.Slot != null)
            {
                suspicion.Slot.Pair = explanation.Slot;
                explanation.Slot.Pair = suspicion.Slot;
            }
        }
    }
}
