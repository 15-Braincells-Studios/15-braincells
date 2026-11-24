using System.Collections.Generic;
using System.Linq;
using FifteenBraincells.MurderGenerator.Data;
using FifteenBraincells.MurderGenerator.Output;

namespace FifteenBraincells.MurderGenerator.Generation
{
    // Step 2: the suspect's fake alibi and real alibis for innocent characters.
    internal sealed partial class LevelAttempt
    {
        /// <summary>How often the fake alibi is told by the suspect themselves when that is possible.</summary>
        private const float SelfToldFakeAlibiChance = 0.7f;

        private readonly List<AlibiPlan> _plans = new List<AlibiPlan>();
        private readonly Dictionary<Character, AlibiPlan> _busy = new Dictionary<Character, AlibiPlan>();
        private readonly HashSet<Character> _eveningSubjects = new HashSet<Character>();
        private AlibiPlan _fakeAlibi;

        private IEnumerable<AlibiPlan> RealPlans => _plans.Where(p => !p.IsFake);

        /// <summary>An alibi that many characters may hold gets a smaller chance.</summary>
        private double AlibiWeight(Alibi alibi) =>
            _settings.WeightedAlibis ? 1.0 / System.Math.Max(1, alibi.HolderCount) : 1.0;

        /// <summary>Whether this alibi may be used tonight by this holder: time, room, method and the day's rules.</summary>
        private bool IsAlibiPossible(Alibi alibi, Character holder)
        {
            if (!alibi.Holders.Matches(holder) || IsPlanAlibi(alibi))
            {
                return false;
            }

            if (_murderTime.Minutes < alibi.Start.Minutes || _murderTime.Minutes >= alibi.End.Minutes
                || alibi.NotWithMethods.Contains(_method))
            {
                return false;
            }

            var place = PlaceOf(alibi, holder);
            if (SameRoom(place, _murderRoom) || (_method.NoOtherOutsideAlibi && alibi.IsOutside))
            {
                return false;
            }

            if (place != null && _noAlibiRooms.TryGetValue(holder, out var banned) && banned.Contains(place.Room.Id))
            {
                return false;
            }

            // One tub per bathroom: at most one character with an alibi in the same room.
            return !(alibi.OneInSameRoom && _plans.Any(p => p.Alibi.OneInSameRoom && SameRoom(PlaceOf(p.Alibi, p.Holder), place)));
        }

        /// <summary>Gives the suspect an alibi that seems to place them elsewhere at the time of the murder.</summary>
        private bool PlanFakeAlibi()
        {
            var options = new List<(Alibi Alibi, Character Partner, List<AlibiVariant> Variants)>();
            foreach (var alibi in _db.Alibis)
            {
                if (!IsAlibiPossible(alibi, _suspect) || alibi.RequiresOtherHolderOf != null)
                {
                    continue;
                }

                if (!TryChoosePartner(alibi, _suspect, isFake: true, out var partner))
                {
                    continue;
                }

                var variants = alibi.Variants.Where(v => IsVariantAllowed(v, _suspect, partner, isFake: true)).ToList();
                if (variants.Count > 0)
                {
                    options.Add((alibi, partner, variants));
                }
            }

            if (options.Count == 0)
            {
                return false;
            }

            var option = _rng.PickWeighted(options, o => AlibiWeight(o.Alibi));
            var selfTold = option.Variants.Where(v => v.Line.Speaker.Kind == SpeakerKind.Self).ToList();
            var variant = _rng.Pick(selfTold.Count > 0 && _rng.Chance(SelfToldFakeAlibiChance) ? selfTold : option.Variants);
            _fakeAlibi = new AlibiPlan(_suspect, option.Alibi, variant, option.Partner, isFake: true);
            _plans.Add(_fakeAlibi);
            return true;
        }

        private void PlanInnocentAlibis()
        {
            foreach (var character in _rng.Shuffled(Innocents))
            {
                // Someone who already says as much as allowed only gets an alibi when the solver needs one.
                if (_rng.Chance(_settings.InnocentAlibiChance) && !IsFull(character))
                {
                    TryGiveAlibi(character, mustTellItThemselves: false);
                }
            }
        }

        /// <summary>
        /// Gives an innocent character a real alibi. Alibis nobody has yet are preferred, so lines do not repeat;
        /// among those, an alibi few characters may hold gets a bigger chance.
        /// </summary>
        /// <param name="mustTellItThemselves">Only alibis the character says themselves (to give a silent character a line).</param>
        private bool TryGiveAlibi(Character holder, bool mustTellItThemselves)
        {
            if (!MayGetAlibi(holder))
            {
                return false;
            }

            var options = new List<AlibiPlan>();
            foreach (var alibi in _db.Alibis)
            {
                if (!IsAlibiPossible(alibi, holder) || !CanDo(holder, alibi.AllNeeds))
                {
                    continue;
                }

                if (alibi.RequiresOtherHolderOf != null && SickCompanion(alibi, holder) == null)
                {
                    continue;
                }

                if (!TryChoosePartner(alibi, holder, isFake: false, out var partner))
                {
                    continue;
                }

                foreach (var variant in alibi.Variants)
                {
                    if (IsVariantAllowed(variant, holder, partner, isFake: false)
                        && (!mustTellItThemselves || SpeakerOf(variant, holder, partner) == holder))
                    {
                        options.Add(new AlibiPlan(holder, alibi, variant, partner, isFake: false));
                    }
                }
            }

            var unused = options.Where(o => _plans.All(p => p.Alibi != o.Alibi)).ToList();
            var order = _rng.WeightedShuffled(unused, o => AlibiWeight(o.Alibi))
                .Concat(_rng.WeightedShuffled(options.Except(unused), o => AlibiWeight(o.Alibi)));
            foreach (var plan in order)
            {
                Assign(plan);
                if ((_contradiction == null || ContradictionRulesHold(_contradiction)) && _loose.All(LooseRulesHold))
                {
                    return true;
                }

                Unassign(plan); // this alibi would make a chosen contradiction wrong
            }

            return false;
        }

        private bool MayGetAlibi(Character character) =>
            character != _suspect && character != _victim && !_mayNotHaveAlibi.Contains(character)
            && !_busy.ContainsKey(character) && !_planHolders.ContainsKey(character) && !_eveningSubjects.Contains(character);

        /// <summary>Finds the alibi's second character. Returns false when the alibi needs one and nobody fits.</summary>
        private bool TryChoosePartner(Alibi alibi, Character holder, bool isFake, out Character partner)
        {
            partner = null;
            IEnumerable<Character> candidates;
            switch (alibi.Partner.Kind)
            {
                case PartnerKind.None:
                    return true;
                case PartnerKind.Fixed:
                    candidates = new[] { alibi.Partner.Character };
                    break;
                case PartnerKind.Joint:
                    candidates = Cast.Where(alibi.Holders.Matches);
                    break;
                default:
                    candidates = Cast.Where(alibi.Partner.Pool.Matches);
                    break;
            }

            // A fake alibi's partner is not really busy; they only need to be someone else who could do it.
            var fitting = candidates
                .Where(c => c != holder && c != _suspect && CanDo(c, alibi.AllNeeds) && (isFake || MayGetAlibi(c)))
                .ToList();
            if (fitting.Count == 0)
            {
                return false;
            }

            partner = _rng.Pick(fitting);
            return true;
        }

        private bool IsVariantAllowed(AlibiVariant variant, Character holder, Character partner, bool isFake)
        {
            if (!_db.IsOpen(variant))
            {
                return false; // lies in or talks about a closed room
            }

            if (variant.HolderTag.Length > 0 && !holder.HasTag(variant.HolderTag))
            {
                return false;
            }

            var speaker = SpeakerOf(variant, holder, partner);
            if (variant.Line.Speaker.IsSpoken && speaker == null)
            {
                return false; // spoken by a partner the alibi does not have
            }

            // Someone else talks about the holder: that someone cannot be the holder.
            if (variant.Line.Speaker.Kind == SpeakerKind.Fixed && speaker == holder)
            {
                return false;
            }

            // Someone with a plan tonight is not around to tell about someone else's evening.
            if (speaker != null && speaker != holder && _planHolders.ContainsKey(speaker))
            {
                return false;
            }

            if (IsFull(speaker))
            {
                return false;
            }

            // The partner of a fake alibi knows nothing about it, so they cannot be the one telling it.
            return !(isFake && partner != null && speaker == partner);
        }

        private static Character SpeakerOf(AlibiVariant variant, Character holder, Character partner) =>
            FixedSpeaker(variant.Line.Speaker, new LineContext { Holder = holder, Partner = partner });

        /// <summary>For "Caring for the sick": someone else who really has the required alibi.</summary>
        private Character SickCompanion(Alibi alibi, Character holder) =>
            RealPlans.FirstOrDefault(p => p.Alibi == alibi.RequiresOtherHolderOf && p.Holder != holder)?.Holder;

        private void Assign(AlibiPlan plan)
        {
            _plans.Add(plan);
            _busy[plan.Holder] = plan;
            if (plan.Partner != null)
            {
                _busy[plan.Partner] = plan;
            }
        }

        private void Unassign(AlibiPlan plan)
        {
            _plans.Remove(plan);
            _busy.Remove(plan.Holder);
            if (plan.Partner != null)
            {
                _busy.Remove(plan.Partner);
            }
        }

        private PlacedRoom AlibiRoom(AlibiPlan plan) => PlaceOf(plan.Alibi, plan.Holder);

        /// <summary>The clue that tells the player about an alibi.</summary>
        private DraftClue AlibiClue(AlibiPlan plan)
        {
            var alibi = plan.Alibi;
            var args = Args(("character", Arg(plan.Holder)));
            if (plan.Partner != null)
            {
                args[alibi.Partner.Placeholder] = new CharacterArg(plan.Partner);
            }

            if (alibi.RequiresOtherHolderOf != null)
            {
                args["sick_character"] = new CharacterArg(SickCompanion(alibi, plan.Holder));
            }

            var effect = plan.IsFake
                ? ClueEffect.FakeAlibi(plan.Holder)
                : ClueEffect.Alibi(plan.Partner == null ? new[] { plan.Holder } : new[] { plan.Holder, plan.Partner });
            var context = new LineContext { Holder = plan.Holder, Partner = plan.Partner };
            var clue = CreateClue(ClueCategory.Alibi, alibi.Id, plan.Variant.Line, args, effect, context);
            clue.Evidence = plan.IsFake ? EvidenceRole.FakeAlibi : EvidenceRole.None;
            return clue;
        }
    }
}
