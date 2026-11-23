using System.Collections.Generic;
using System.Linq;
using FifteenBraincells.MurderGenerator.Data;
using FifteenBraincells.MurderGenerator.Output;

namespace FifteenBraincells.MurderGenerator.Generation
{
    // Step 1: the crime itself (who, how, when, where, why), the method clues and the motives.
    internal sealed partial class LevelAttempt
    {
        /// <summary>
        /// Picks the victim, murderer, method and motive, then the victim's plan, the time and the murder room.
        /// With weighted motives the victim and then the murderer are picked with equal chances, so every
        /// character is murderer and victim about equally often; then a method (one that fits few pairs gets a
        /// bigger chance) and a motive (one that fits few pairs gets a bigger chance).
        /// </summary>
        private bool ChooseCrime()
        {
            var crime = _settings.WeightedMotives ? PickCrimeWeighted() : PickCrimeUniform();
            if (crime == null)
            {
                return false;
            }

            _method = crime.Method;
            _suspect = crime.Suspect;
            _victim = crime.Victim;
            _motive = _settings.WeightedMotives
                ? _rng.PickWeighted(crime.Motives, _cache.MotiveWeight)
                : _rng.Pick(crime.Motives);
            _mayNotHaveAlibi.Add(_victim); // the victim will be at the murder place
            return ChooseTimeAndPlace();
        }

        private Crime PickCrimeUniform()
        {
            var crimes = _cache.Crimes[_forcedMethod ?? _rng.Pick(_db.Methods)];
            return crimes.Count == 0 ? null : _rng.Pick(crimes);
        }

        private Crime PickCrimeWeighted()
        {
            var methods = _forcedMethod != null ? new[] { _forcedMethod } : _db.Methods.ToArray();
            var crimes = methods.SelectMany(m => _cache.Crimes[m]).ToList();
            if (crimes.Count == 0)
            {
                return null;
            }

            var victim = _rng.Pick(crimes.Select(c => c.Victim).Distinct().ToList());
            var suspect = _rng.Pick(crimes.Where(c => c.Victim == victim).Select(c => c.Suspect).Distinct().ToList());
            var options = crimes.Where(c => c.Victim == victim && c.Suspect == suspect).ToList();
            return _rng.PickWeighted(options, c => 1.0 / _cache.Crimes[c.Method].Count);
        }

        /// <summary>The time of murder and the murder room, from the method or from the victim's plan.</summary>
        private bool ChooseTimeAndPlace()
        {
            var s = _settings;
            switch (_method.Kind)
            {
                case MethodKind.Fixed:
                    _murderTime = _rng.Pick(GenerationCache.Steps(s, _method.WindowStart.Minutes, _method.WindowEnd.Minutes));
                    _murderRoom = new PlacedRoom(_db.GetRoom(_rng.Pick(_method.RoomIds)));
                    return true;

                case MethodKind.Plan:
                    _victimPlan = _rng.Pick(GenerationCache.PlansFor(_db, _method, _victim));
                    _murderTime = _victimPlan.StartTime;
                    var roomId = _rng.Pick(_method.RoomIds);
                    _murderRoom = new PlacedRoom(_db.GetRoom(roomId == RoomRef.VictimBathroom ? RoomRef.BathroomOf(_victim) : roomId));
                    return true;

                case MethodKind.Solo:
                    _victimPlan = _rng.Pick(GenerationCache.PlansFor(_db, _method, _victim));
                    _murderTime = _rng.Pick(GenerationCache.SoloTimes(s, _victimPlan));
                    _murderRoom = PlaceOf(_victimPlan.Alibi, _victim);
                    return _murderRoom != null;

                case MethodKind.Sleep:
                    _victimPlan = _rng.Pick(GenerationCache.PlansFor(_db, _method, _victim));
                    var sleepsInInfirmary = _victimPlan.Alibi != null && _victimPlan.Alibi.RoomId == "infirmary";
                    _murderRoom = sleepsInInfirmary ? new PlacedRoom(_db.GetRoom("infirmary")) : BedroomOf(_victim);
                    var bedtime = _victimPlan.Alibi == null && _victimPlan.Time.HasValue ? _victimPlan.Time.Value : s.DefaultBedtime;
                    var times = GenerationCache.Steps(s, bedtime.Minutes + _method.SleepDelayMin,
                        System.Math.Min(_method.WindowEnd.Minutes, bedtime.Minutes + _method.SleepDelayMax));
                    if (times.Count == 0)
                    {
                        return false;
                    }

                    _murderTime = _rng.Pick(times);
                    return true;
            }

            return false;
        }

        // ------------------------------------------------------------------ method clues

        /// <summary>Rolls each clue group against its chance and adds one fitting clue per group that hits.</summary>
        private void AddMethodClues()
        {
            foreach (var rule in _db.ClueGroups)
            {
                // Groups A and B are part of the evidence, so they are always shown.
                var isEvidence = rule.Group == ClueGroup.WhenAndWhere || rule.Group == ClueGroup.Method;
                if (!isEvidence && !_rng.Chance(rule.ShowChance))
                {
                    continue;
                }

                var usable = _db.GetMethodClues(_method, rule.Group).Where(IsUsable).ToList();
                if (usable.Count > 0)
                {
                    AddMethodClue(_rng.Pick(usable));
                }
            }
        }

        private bool IsUsable(MethodClue clue)
        {
            if (clue.NotSuspect.Contains(_suspect) || clue.NotVictim.Contains(_victim)
                || !clue.SuspectHasTags.All(_suspect.HasTag) || clue.NoOtherAlibiFor.Contains(_victim)
                || IsFull(clue.Line.Speaker.Character))
            {
                return false;
            }

            switch (clue.Effect)
            {
                case MethodClueEffect.PartialList:
                    // "(partial list) rode out with me" makes no sense when the speaker is the one it is about.
                    return Truth(clue.EffectRole) != clue.Line.Speaker.Character
                           && PartialListOthers(clue).Count >= _settings.PartialListExtraMin;
                case MethodClueEffect.Excluded:
                    return ExcludedCandidates(clue).Count >= _settings.ExcludedMin;
                default:
                    return true;
            }
        }

        private void AddMethodClue(MethodClue clue)
        {
            var role = clue.EffectRole;
            var args = Args(("murder_place", new RoomArg(_murderRoom)), ("murder_time", new TimeArg(_murderTime)),
                ("victim", Arg(_victim)));
            var effect = ClueEffect.Nothing;
            IReadOnlyList<Character> named = null;

            switch (clue.Effect)
            {
                case MethodClueEffect.FullList:
                    named = Cast.Where(_method.PoolFor(role).Matches).ToList();
                    var shown = clue.ListOmitsSpeaker ? named.Where(c => c != clue.Line.Speaker.Character).ToList() : named;
                    args["list"] = new CharacterListArg(shown);
                    effect = ClueEffect.OnlyThese(role, named);
                    break;

                case MethodClueEffect.PartialList:
                    var others = PartialListOthers(clue);
                    var extra = _rng.Range(_settings.PartialListExtraMin, System.Math.Min(_settings.PartialListExtraMax, others.Count));
                    named = _rng.Shuffled(_rng.Shuffled(others).Take(extra).Append(Truth(role)));
                    _partialListed.UnionWith(named);
                    args["partial_list"] = new CharacterListArg(named);
                    effect = ClueEffect.OnlyThese(role, named);
                    break;

                case MethodClueEffect.Excluded:
                    var candidates = ExcludedCandidates(clue);
                    var count = _rng.Range(_settings.ExcludedMin, System.Math.Min(_settings.ExcludedMax, candidates.Count));
                    named = _rng.Shuffled(candidates).Take(count).ToList();
                    args["excluded"] = new CharacterListArg(named);
                    effect = ClueEffect.NotThese(role, named);
                    foreach (var character in named)
                    {
                        Block(character, clue.Blocks);
                        if (clue.ExcludedGetNoOtherAlibi)
                        {
                            _mayNotHaveAlibi.Add(character);
                        }

                        if (clue.ExcludedNoAlibiRooms.Count > 0)
                        {
                            if (!_noAlibiRooms.TryGetValue(character, out var rooms))
                            {
                                _noAlibiRooms[character] = rooms = new HashSet<string>();
                            }

                            rooms.UnionWith(clue.ExcludedNoAlibiRooms);
                        }
                    }

                    break;

                case MethodClueEffect.ExcludeFixed:
                    effect = ClueEffect.NotThese(role, clue.EffectCharacters);
                    break;

                case MethodClueEffect.OnlyFixed:
                    effect = ClueEffect.OnlyThese(role, clue.EffectCharacters);
                    break;
            }

            if (clue.ListedGetNoOtherAlibi && named != null)
            {
                // A presence list: everyone on it will be at the murder place tonight.
                _mayNotHaveAlibi.UnionWith(named.Where(c => c != _suspect));
            }

            _mayNotHaveAlibi.UnionWith(clue.NoOtherAlibiFor);

            if (clue.Group == ClueGroup.WhenAndWhere && (_method.Kind == MethodKind.Plan || _method.Kind == MethodKind.Solo))
            {
                // Time and place point at one plan for tonight: the victim's.
                effect = ClueEffect.OnlyThese(CrimeRole.Victim, new[] { _victim });
            }

            // An unnamed witness is never the suspect or the victim.
            var draft = AddClue(ClueCategory.Method, clue.Id, clue.Line, args, effect, new LineContext { Suspect = _suspect, Victim = _victim },
                mayNotSpeak: new[] { _suspect, _victim });
            draft.Evidence = clue.Group == ClueGroup.WhenAndWhere ? EvidenceRole.WhenAndWhere
                : clue.Group == ClueGroup.Method ? EvidenceRole.Method
                : EvidenceRole.None;
        }

        private Character Truth(CrimeRole role) => role == CrimeRole.Suspect ? _suspect : _victim;

        /// <summary>Everyone who could join the real suspect or victim on a "(partial list)".</summary>
        private List<Character> PartialListOthers(MethodClue clue)
        {
            var truth = Truth(clue.EffectRole);
            var pool = _method.PoolFor(clue.EffectRole);
            return Cast.Where(c => c != truth && c != clue.Line.Speaker.Character && pool.Matches(c)).ToList();
        }

        /// <summary>Innocent characters who may be named by "(excluded)". A filter in the data replaces the method's pool.</summary>
        private List<Character> ExcludedCandidates(MethodClue clue)
        {
            var filter = clue.ExcludedFilter.IsEmpty ? _method.PoolFor(clue.EffectRole) : clue.ExcludedFilter;
            return Innocents
                .Where(c => c != clue.Line.Speaker.Character && filter.Matches(c)
                            && CanDo(c, clue.ExcludedNotBlockedFor) && !_mayNotHaveAlibi.Contains(c)
                            && !(clue.ExcludedNotOnPartialList && _partialListed.Contains(c)))
                .ToList();
        }

        // ------------------------------------------------------------------ motives

        /// <summary>Adds the real motive and one or two false leads between other characters.</summary>
        private void AddMotiveClues()
        {
            AddMotiveClue(_motive, _suspect, _victim).Evidence = EvidenceRole.Motive;

            var falseLeads = _rng.Range(_settings.FalseMotivesMin, _settings.FalseMotivesMax);
            var usedMotives = new HashSet<Motive> { _motive };
            var usedPairs = new HashSet<(Character, Character)> { (_suspect, _victim) };
            var options = Innocents
                .SelectMany(suspect => Cast.SelectMany(victim => _db.Motives
                    .Where(m => m.Fits(suspect, victim))
                    .Select(m => (Motive: m, Suspect: suspect, Victim: victim))))
                .ToList();

            for (var tries = 0; tries < falseLeads * 6 && usedMotives.Count <= falseLeads; tries++)
            {
                var left = options.Where(o => !usedMotives.Contains(o.Motive) && !usedPairs.Contains((o.Suspect, o.Victim))).ToList();
                if (left.Count == 0)
                {
                    break;
                }

                // Weighted: an innocent first, then someone they could have a motive against, then the motive.
                var option = _settings.WeightedMotives ? PickFalseLead(left) : _rng.Pick(left);
                usedMotives.Add(option.Motive);
                usedPairs.Add((option.Suspect, option.Victim));
                AddMotiveClue(option.Motive, option.Suspect, option.Victim);
            }
        }

        private (Motive Motive, Character Suspect, Character Victim) PickFalseLead(
            List<(Motive Motive, Character Suspect, Character Victim)> options)
        {
            var suspect = _rng.Pick(options.Select(o => o.Suspect).Distinct().ToList());
            var victim = _rng.Pick(options.Where(o => o.Suspect == suspect).Select(o => o.Victim).Distinct().ToList());
            return _rng.PickWeighted(options.Where(o => o.Suspect == suspect && o.Victim == victim).ToList(),
                o => _cache.MotiveWeight(o.Motive));
        }

        private DraftClue AddMotiveClue(Motive motive, Character suspect, Character victim)
        {
            var context = new LineContext { Suspect = suspect, Victim = victim };
            var args = Args(("suspect", Arg(suspect)), ("victim", Arg(victim)));
            return AddClue(ClueCategory.Motive, motive.Id, motive.Clue, args, ClueEffect.Motive(suspect, victim), context,
                mayNotSpeak: new[] { suspect, victim });
        }
    }
}
