using System.Collections.Generic;
using System.Linq;
using FifteenBraincells.MurderGenerator.Data;
using FifteenBraincells.MurderGenerator.Output;

namespace FifteenBraincells.MurderGenerator.Generation
{
    // Step 1b: plans for tonight. The victim's plan tells the murderer where the victim will be; a decoy has a
    // plan of the same kind. Plans are not alibis, and nobody with a plan gets an alibi.
    internal sealed partial class LevelAttempt
    {
        private readonly Dictionary<Character, VictimPlan> _planHolders = new Dictionary<Character, VictimPlan>();

        private bool AddPlans()
        {
            if (_victimPlan == null)
            {
                return true;
            }

            _planHolders[_victim] = _victimPlan;
            if (_method.AllowDecoys && _rng.Chance(_settings.DecoyChance))
            {
                var excluded = new HashSet<Character>(_clues
                    .Where(c => c.Effect.Kind == ClueEffectKind.NotThese)
                    .SelectMany(c => c.Effect.Characters));
                foreach (var character in _rng.Shuffled(Innocents.Where(c => !excluded.Contains(c) && !_mayNotHaveAlibi.Contains(c))))
                {
                    var options = GenerationCache.PlansFor(_db, _method, character)
                        .Where(p => p != _victimPlan && (_method.Kind != MethodKind.Plan || p.StartTime.Minutes != _murderTime.Minutes))
                        .ToList();
                    if (options.Count > 0)
                    {
                        _planHolders[character] = _rng.Pick(options);
                        break;
                    }
                }
            }

            foreach (var holder in _planHolders.Keys.ToList())
            {
                _mayNotHaveAlibi.Add(holder);
                if (!AddPlanClue(holder, _planHolders[holder]))
                {
                    return false;
                }
            }

            return true;
        }

        private bool AddPlanClue(Character holder, VictimPlan plan)
        {
            var context = new LineContext { Holder = holder };
            var args = Args(("character", Arg(holder)));
            var source = plan.Id;
            if (plan.Line != null)
            {
                if (IsFull(FixedSpeaker(plan.Line.Speaker, context)))
                {
                    return false;
                }

                AddClue(ClueCategory.Plan, source, plan.Line, args, ClueEffect.Nothing, context);
                return true;
            }

            // An alibi used as a plan: one of its own lines, preferably told by the holder.
            var variants = plan.Alibi.Variants
                .Where(v => _db.IsOpen(v) && (v.HolderTag.Length == 0 || holder.HasTag(v.HolderTag))
                            && v.Line.Speaker.Kind != SpeakerKind.Partner
                            && !(v.Line.Speaker.Kind == SpeakerKind.Fixed && v.Line.Speaker.Character == holder)
                            && !IsFull(FixedSpeaker(v.Line.Speaker, context)))
                .ToList();
            if (variants.Count == 0)
            {
                return false;
            }

            var self = variants.Where(v => v.Line.Speaker.Kind == SpeakerKind.Self).ToList();
            var variant = _rng.Pick(self.Count > 0 ? self : variants);
            AddClue(ClueCategory.Plan, source, variant.Line, args, ClueEffect.Nothing, context);
            return true;
        }

        /// <summary>Alibis that are plans with this method (an outside alibi for the ambush, a bath), never alibis.</summary>
        private bool IsPlanAlibi(Alibi alibi) =>
            _db.GetPlans(_method).Any(p => p.Alibi == alibi && !p.IsSolo);

        /// <summary>Whether someone has this alibi tonight, as an alibi or as a plan.</summary>
        private bool IsHeldTonight(Alibi alibi) =>
            _plans.Any(p => p.Alibi == alibi)
            || _planHolders.Values.Any(p => p.Alibi == alibi || p.SameAsAlibi == alibi);
    }
}
