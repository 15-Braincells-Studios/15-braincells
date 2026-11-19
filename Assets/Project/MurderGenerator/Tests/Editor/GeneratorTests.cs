using System.Collections.Generic;
using System.Linq;
using FifteenBraincells.MurderGenerator.Data;
using FifteenBraincells.MurderGenerator.Generation;
using FifteenBraincells.MurderGenerator.Output;
using FifteenBraincells.MurderGenerator.Solving;
using FifteenBraincells.MurderGenerator.Text;
using NUnit.Framework;

namespace FifteenBraincells.MurderGenerator.Tests
{
    /// <summary>
    /// Generates many levels with the real data and checks every rule from the Excel on each of them.
    /// Each test collects all violations, so a failure shows every broken level at once.
    /// </summary>
    [TestFixture]
    public sealed class GeneratorTests
    {
        private const int LevelCount = 400;
        private const int LevelsPerMethod = 60;

        private static List<Level> _levels;
        private static Dictionary<string, SpeakerKind> _speakerKindByTextKey;

        private static GeneratorDatabase Db => TestData.Database;

        private static IReadOnlyList<Level> Levels => _levels ??= GenerateLevels();

        private static List<Level> GenerateLevels()
        {
            var generator = new LevelGenerator(Db);
            var levels = Enumerable.Range(1, LevelCount).Select(seed => generator.Generate(seed)).ToList();
            foreach (var method in Db.Methods)
            {
                levels.AddRange(Enumerable.Range(10_000, LevelsPerMethod).Select(seed => generator.Generate(seed, method)));
            }

            return levels;
        }

        // ------------------------------------------------------------------ basics

        [Test]
        public void SameSeedGivesSameLevel()
        {
            var renderer = new ClueTextRenderer(TestData.Strings);
            var first = LevelReport.Describe(new LevelGenerator(Db).Generate(42), renderer);
            var second = LevelReport.Describe(new LevelGenerator(Db).Generate(42), renderer);
            Assert.AreEqual(first, second);
        }

        [Test]
        public void EveryMethodCanBeForced()
        {
            var problems = Db.Methods
                .Where(m => Levels.Count(l => l.Solution.Method == m) < LevelsPerMethod)
                .Select(m => m.Id);
            Assert.IsEmpty(problems);
        }

        [Test]
        public void EveryLevelHasExactlyTheRightAnswer()
        {
            var problems = Check(level =>
            {
                var result = Solver.Solve(level.Clues, Db.Characters);
                return result.IsSolved && result.Remaining[0].Is(level.Solution.Suspect, level.Solution.Victim)
                    ? null
                    : $"{result.Remaining.Count} answers left";
            });
            Assert.IsEmpty(problems);
        }

        [Test]
        public void WithoutTheContradictionTheFakeAlibiHolds()
        {
            var problems = Check(level =>
            {
                var withoutContradiction = level.Clues.Where(c => c.Effect.Kind != ClueEffectKind.BreaksFakeAlibi);
                var result = Solver.Solve(withoutContradiction, Db.Characters);
                return result.Remaining.Any(s => s.Is(level.Solution.Suspect, level.Solution.Victim))
                    ? "the murderer is found without breaking the fake alibi"
                    : null;
            });
            Assert.IsEmpty(problems);
        }

        [Test]
        public void EveryCharacterHasALine()
        {
            var problems = Check(level =>
            {
                var silent = Db.Characters.Where(c => !level.LinesOf(c).Any()).Select(c => c.Id).ToList();
                return silent.Count == 0 ? null : "silent: " + string.Join(", ", silent);
            });
            Assert.IsEmpty(problems);
        }

        [Test]
        public void EveryTextRendersCompletely()
        {
            var renderer = new ClueTextRenderer(TestData.Strings);
            var problems = Check(level => level.Clues
                .Select(renderer.Render)
                .FirstOrDefault(text => text.Contains("{") || text.Contains("}") || text.Contains("  ")));
            Assert.IsEmpty(problems);
        }

        // ------------------------------------------------------------------ method clues

        [Test]
        public void MethodCluesRespectNotSuspectAndNotVictim()
        {
            var problems = Check(level => MethodClues(level)
                .Where(m => m.NotSuspect.Contains(level.Solution.Suspect) || m.NotVictim.Contains(level.Solution.Victim))
                .Select(m => m.Id)
                .FirstOrDefault());
            Assert.IsEmpty(problems);
        }

        [Test]
        public void PartialListsHoldTheTruthAndTheRightNumberOfOthers()
        {
            var settings = Db.Settings;
            var problems = Check(level => level.Clues
                .Where(c => c.Args.ContainsKey("partial_list"))
                .Select(c =>
                {
                    var names = ((CharacterListArg)c.Args["partial_list"]).Characters;
                    var truth = c.Effect.Role == CrimeRole.Suspect ? level.Solution.Suspect : level.Solution.Victim;
                    var others = names.Count - 1;
                    return names.Contains(truth) && others >= settings.PartialListExtraMin && others <= settings.PartialListExtraMax
                        ? null
                        : c.SourceId;
                })
                .FirstOrDefault(p => p != null));
            Assert.IsEmpty(problems);
        }

        [Test]
        public void ExcludedCharactersAreInnocentAndMatchTheirFilter()
        {
            var problems = Check(level => level.Clues
                .Where(c => c.Args.ContainsKey("excluded"))
                .Select(c =>
                {
                    var clue = Db.MethodClues.Single(m => m.Id == c.SourceId);
                    var named = ((CharacterListArg)c.Args["excluded"]).Characters;
                    var bad = named.Where(x => x == level.Solution.Suspect || x == level.Solution.Victim
                                               || (!clue.ExcludedFilter.IsEmpty && !clue.ExcludedFilter.Matches(x)));
                    return bad.Any() ? $"{c.SourceId}: {string.Join(", ", bad)}" : null;
                })
                .FirstOrDefault(p => p != null));
            Assert.IsEmpty(problems);
        }

        [Test]
        public void PresenceListsAndExcludedWithoutAlibiGetNoOtherAlibi()
        {
            var problems = Check(level =>
            {
                var withAlibi = level.Alibis.Where(a => !a.IsFake)
                    .SelectMany(a => new[] { a.Holder, a.Partner })
                    .Where(c => c != null)
                    .ToList();
                foreach (var clue in level.Clues.Where(c => c.Category == ClueCategory.Method))
                {
                    var data = Db.MethodClues.Single(m => m.Id == clue.SourceId);
                    if (data.ListedGetNoOtherAlibi || data.ExcludedGetNoOtherAlibi)
                    {
                        var elsewhere = clue.Effect.Characters.Where(withAlibi.Contains).Select(c => c.Id).ToList();
                        if (elsewhere.Count > 0)
                        {
                            return $"{clue.SourceId}: {string.Join(", ", elsewhere)} also have an alibi";
                        }
                    }
                }

                return null;
            });
            Assert.IsEmpty(problems);
        }

        // ------------------------------------------------------------------ needs and blocks

        [Test]
        public void BlockedCharactersNeverGetSomethingTheyCannotDo()
        {
            var liftingTag = Db.Settings.LiftingRequiresTag;
            var problems = Check(level =>
            {
                foreach (var alibi in level.Alibis)
                {
                    foreach (var person in new[] { alibi.Holder, alibi.Partner }.Where(p => p != null))
                    {
                        if (!CanDo(level, person, alibi.Alibi.Needs, liftingTag))
                        {
                            return $"{person.Id} has alibi {alibi.Alibi.Id}";
                        }
                    }
                }

                foreach (var clue in level.Clues.Where(c => c.Category == ClueCategory.RedHerring && c.Args.ContainsKey("character")))
                {
                    var subject = ((CharacterArg)clue.Args["character"]).Character;
                    var herring = Db.RedHerrings.Single(h => h.Id == clue.SourceId);
                    if (!CanDo(level, subject, herring.Needs, liftingTag))
                    {
                        return $"{subject.Id} is the subject of {herring.Id}";
                    }
                }

                return null;
            });
            Assert.IsEmpty(problems);
        }

        [Test]
        public void UnnamedWitnessesCanSeeAndAreNotWhoTheLineIsAbout()
        {
            var problems = Check(level => level.Clues
                .Where(c => SpeakerKindOf(c) == SpeakerKind.Witness)
                .Select(c =>
                {
                    var blocked = level.Blocked.TryGetValue(c.Speaker, out var b) ? b : Need.None;
                    if ((blocked & (Need.Sight | Need.Afternoon)) != 0)
                    {
                        return $"{c.Speaker.Id} witnesses {c.SourceId} while blocked for {blocked}";
                    }

                    var template = TestData.Strings.Get(c.TextKey);
                    var about = c.Args
                        .Where(a => a.Value is CharacterArg && template.Contains("{" + a.Key + "}"))
                        .Select(a => ((CharacterArg)a.Value).Character);
                    return about.Contains(c.Speaker) ? $"{c.Speaker.Id} witnesses {c.SourceId} about themselves" : null;
                })
                .FirstOrDefault(p => p != null));
            Assert.IsEmpty(problems);
        }

        // ------------------------------------------------------------------ alibis and contradictions

        [Test]
        public void AlibisCoverTheMurderAndTheVictimHasNone()
        {
            var problems = Check(level =>
            {
                var solution = level.Solution;
                var notCovering = level.Alibis.FirstOrDefault(a => !a.Alibi.Covers(solution.Method, solution.MurderRoom.Room.Id));
                if (notCovering != null)
                {
                    return $"{notCovering.Alibi.Id} does not cover the murder";
                }

                // The partner named in the fake alibi may be the victim: that is part of the lie.
                return level.Alibis.Any(a => !a.IsFake && (a.Holder == solution.Victim || a.Partner == solution.Victim))
                    ? "the victim has an alibi"
                    : null;
            });
            Assert.IsEmpty(problems);
        }

        [Test]
        public void TheContradictionFitsTheFakeAlibiAndAllItsConditions()
        {
            var problems = Check(level =>
            {
                var solution = level.Solution;
                var contradiction = solution.Contradiction;
                var fake = level.Alibis.Single(a => a.IsFake);
                var others = level.Alibis.Where(a => !a.IsFake).Select(a => a.Alibi).ToList();
                var rules = contradiction.Rules;

                var failed = new List<string>();
                void Require(bool ok, string rule)
                {
                    if (!ok)
                    {
                        failed.Add(rule);
                    }
                }

                Require(fake.Alibi == solution.FakeAlibi && fake.Holder == solution.Suspect, "fake alibi belongs to the murderer");
                Require(contradiction.IsSpecific
                    ? contradiction.Alibi == fake.Alibi
                    : contradiction.AlibiTag == "Any" || (contradiction.AlibiTag == "Outside") == fake.Alibi.IsOutside, "alibi tag");
                Require(!rules.NotSuspect.Contains(solution.Suspect), "not the suspect");
                Require(!rules.AllOtherAlibisInside || others.All(a => !a.IsOutside), "all others inside");
                Require(!rules.NoOtherAlibiAtAlibiPlace || others.All(a => a.IsOutside || a.RoomId != fake.Alibi.RoomId), "nobody else at that place");
                Require(rules.NoOtherAlibiInRoom == null || others.All(a => a.RoomId != rules.NoOtherAlibiInRoom.Id), "nobody else in the room");
                Require(!rules.NoOtherOutsideAlibi || others.All(a => !a.IsOutside), "nobody else outside");
                Require(rules.NoOtherHolderOf == null || !others.Contains(rules.NoOtherHolderOf), "nobody else has the alibi");
                Require(rules.SuspectHasTags.All(solution.Suspect.HasTag), "suspect tags");
                Require(rules.OnlyForAlibis.Count == 0 || rules.OnlyForAlibis.Contains(fake.Alibi), "only for alibis");
                Require(!rules.NotForAlibis.Contains(fake.Alibi), "not for alibis");
                Require(!rules.NotWithMethods.Contains(solution.Method), "not with method");
                return failed.Count == 0 ? null : $"{contradiction.Id}: {string.Join(", ", failed)}";
            });
            Assert.IsEmpty(problems);
        }

        [Test]
        public void SpecificContradictionsAreUsedAboutAsOftenAsTheSettingSays()
        {
            var withSpecificOption = Levels
                .Where(l => Db.Contradictions.Any(c => c.IsSpecific && c.Alibi == l.Solution.FakeAlibi && c.Rules.NotSuspect.All(s => s != l.Solution.Suspect)))
                .ToList();
            var share = withSpecificOption.Count(l => l.Solution.Contradiction.IsSpecific) / (float)withSpecificOption.Count;
            var expected = Db.Settings.SpecificContradictionChance;
            Assert.IsTrue(share > expected - 0.15f && share < expected + 0.15f, $"specific share {share:0.00}, setting {expected:0.00}");
        }

        // ------------------------------------------------------------------ motives and red herrings

        [Test]
        public void RedHerringsAreNeverAboutTheMurdererOrVictim()
        {
            var problems = Check(level => level.Clues
                .Where(c => c.Category == ClueCategory.RedHerring && c.Args.ContainsKey("character"))
                .Select(c => ((CharacterArg)c.Args["character"]).Character)
                .Where(subject => subject == level.Solution.Suspect || subject == level.Solution.Victim)
                .Select(subject => subject.Id)
                .FirstOrDefault());
            Assert.IsEmpty(problems);
        }

        [Test]
        public void FalseMotivesAreAboutInnocentSuspects()
        {
            var problems = Check(level => level.Clues
                .Where(c => c.Effect.Kind == ClueEffectKind.Motive && c.Effect.MotiveSuspect != level.Solution.Suspect)
                .Where(c => c.Effect.MotiveSuspect == level.Solution.Victim)
                .Select(c => c.SourceId)
                .FirstOrDefault());
            Assert.IsEmpty(problems);
        }

        // ------------------------------------------------------------------ helpers

        private static List<string> Check(System.Func<Level, string> problemIn) =>
            Levels.Select(l => (Level: l, Problem: problemIn(l)))
                .Where(x => x.Problem != null)
                .Select(x => $"seed {x.Level.Seed} ({x.Level.Solution.Method.Id}): {x.Problem}")
                .ToList();

        private static IEnumerable<MethodClue> MethodClues(Level level) =>
            level.Clues.Where(c => c.Category == ClueCategory.Method).Select(c => Db.MethodClues.Single(m => m.Id == c.SourceId));

        private static bool CanDo(Level level, Character character, Need needs, string liftingTag)
        {
            var blocked = level.Blocked.TryGetValue(character, out var b) ? b : Need.None;
            return (blocked & needs) == 0 && ((needs & Need.Lifting) == 0 || character.HasTag(liftingTag));
        }

        private static SpeakerKind SpeakerKindOf(Clue clue)
        {
            if (_speakerKindByTextKey == null)
            {
                var lines = Db.MethodClues.Select(m => m.Line)
                    .Concat(Db.Motives.Select(m => m.Clue))
                    .Concat(Db.Alibis.SelectMany(a => a.Variants).Select(v => v.Line))
                    .Concat(Db.Contradictions.Select(c => c.Line))
                    .Concat(Db.RedHerrings.SelectMany(h => new[] { h.Suspicion.Line, h.Explanation.Line }));
                _speakerKindByTextKey = lines.ToDictionary(l => l.TextKey, l => l.Speaker.Kind);
            }

            return _speakerKindByTextKey[clue.TextKey];
        }
    }
}
