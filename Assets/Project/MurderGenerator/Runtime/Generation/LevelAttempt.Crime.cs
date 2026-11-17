using System.Collections.Generic;
using System.Linq;
using FifteenBraincells.MurderGenerator.Data;
using FifteenBraincells.MurderGenerator.Output;

namespace FifteenBraincells.MurderGenerator.Generation
{
    // Step 1: the crime itself, the method clues and the motives.
    internal sealed partial class LevelAttempt
    {
        /// <summary>Picks the method, murder room, suspect, victim and a motive that fits them.</summary>
        private bool ChooseCrime()
        {
            _method = _forcedMethod ?? _rng.Pick(_db.Methods);
            _murderRoom = new PlacedRoom(_db.GetRoom(_rng.Pick(_method.RoomIds)));

            var crimes = new List<(Character Suspect, Character Victim, List<Motive> Motives)>();
            foreach (var suspect in Cast.Where(_method.Suspects.Matches))
            {
                foreach (var victim in Cast.Where(_method.Victims.Matches))
                {
                    var motives = _db.Motives.Where(m => m.Fits(suspect, victim)).ToList();
                    if (motives.Count > 0)
                    {
                        crimes.Add((suspect, victim, motives));
                    }
                }
            }

            if (crimes.Count == 0)
            {
                return false;
            }

            var crime = _rng.Pick(crimes);
            _suspect = crime.Suspect;
            _victim = crime.Victim;
            _motive = _rng.Pick(crime.Motives);
            _mayNotHaveAlibi.Add(_victim); // the victim will be at the murder place
            return true;
        }

        // ------------------------------------------------------------------ method clues

        /// <summary>Rolls each clue group against its chance and adds one fitting clue per group that hits.</summary>
        private void AddMethodClues()
        {
            foreach (var rule in _db.ClueGroups)
            {
                if (!_rng.Chance(rule.ShowChance))
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
            if (clue.NotSuspect.Contains(_suspect) || clue.NotVictim.Contains(_victim))
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
            var args = Args(("murder_place", new RoomArg(_murderRoom)));
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

            AddClue(ClueCategory.Method, clue.Id, clue.Line, args, effect, new LineContext { Suspect = _suspect, Victim = _victim });
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
            return Innocents.Where(c => c != clue.Line.Speaker.Character && filter.Matches(c)).ToList();
        }

        // ------------------------------------------------------------------ motives

        /// <summary>Adds the real motive and one or two false leads between other characters.</summary>
        private void AddMotiveClues()
        {
            AddMotiveClue(_motive, _suspect, _victim);

            var falseLeads = _rng.Range(_settings.FalseMotivesMin, _settings.FalseMotivesMax);
            var usedMotives = new HashSet<Motive> { _motive };
            var usedPairs = new HashSet<(Character, Character)> { (_suspect, _victim) };
            var options = Innocents
                .SelectMany(suspect => Cast.SelectMany(victim => _db.Motives
                    .Where(m => m.Fits(suspect, victim))
                    .Select(m => (Motive: m, Suspect: suspect, Victim: victim))))
                .ToList();

            foreach (var option in _rng.Shuffled(options))
            {
                if (usedMotives.Count > falseLeads)
                {
                    break;
                }

                if (usedMotives.Contains(option.Motive) || !usedPairs.Add((option.Suspect, option.Victim)))
                {
                    continue;
                }

                usedMotives.Add(option.Motive);
                AddMotiveClue(option.Motive, option.Suspect, option.Victim);
            }
        }

        private void AddMotiveClue(Motive motive, Character suspect, Character victim)
        {
            var context = new LineContext { Suspect = suspect, Victim = victim };
            var args = Args(("suspect", Arg(suspect)), ("victim", Arg(victim)));
            AddClue(ClueCategory.Motive, motive.Id, motive.Clue, args, ClueEffect.Motive(suspect, victim), context,
                mayNotSpeak: new[] { suspect, victim });
        }
    }
}
