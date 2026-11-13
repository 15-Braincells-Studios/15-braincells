using System.Linq;
using FifteenBraincells.MurderGenerator.Data;
using FifteenBraincells.MurderGenerator.Data.Dto;
using NUnit.Framework;

namespace FifteenBraincells.MurderGenerator.Tests
{
    /// <summary>Checks the data files themselves.</summary>
    [TestFixture]
    public sealed class DataTests
    {
        [Test]
        public void TheDataLoadsWithoutProblems()
        {
            // Loading throws GeneratorDataException with every problem listed when something is wrong.
            Assert.IsNotNull(TestData.Database);
            Assert.IsTrue(TestData.Database.Methods.Count > 0, "no methods");
        }

        [Test]
        public void EveryMethodHasAClueForWhenAndWhere()
        {
            var db = TestData.Database;
            var missing = db.Methods.Where(m => db.GetMethodClues(m, ClueGroup.WhenAndWhere).Count == 0).Select(m => m.Id);
            Assert.IsEmpty(missing);
        }

        [Test]
        public void EveryStringIsUsedByTheData()
        {
            // Unused strings usually mean a typo in a key somewhere.
            var db = TestData.Database;
            var used = db.Characters.SelectMany(c => new[] { c.NameKey, c.ReferenceKey })
                .Concat(db.Rooms.Select(r => r.NameKey))
                .Concat(db.Methods.Select(m => m.NameKey))
                .Concat(db.MethodClues.Select(m => m.Line.TextKey))
                .Concat(db.Motives.Select(m => m.Clue.TextKey))
                .Concat(db.Alibis.SelectMany(a => a.Variants.Select(v => v.Line.TextKey).Append(a.NameKey).Append(a.PlaceKey)))
                .Concat(db.Contradictions.Select(c => c.Line.TextKey))
                .Concat(db.RedHerrings.SelectMany(h => new[] { h.Suspicion.Line.TextKey, h.Explanation.Line.TextKey }))
                .Concat(new[]
                {
                    Text.ClueTextRenderer.ListSeparatorKey,
                    Text.ClueTextRenderer.ListLastSeparatorKey,
                    Text.ClueTextRenderer.FirstPersonObjectKey,
                })
                .ToHashSet();
            Assert.AreEqual(used.Count(k => k.Length > 0), TestData.Strings.Count, "string table size differs from used keys");
        }

        [Test]
        public void BrokenDataReportsEveryProblem()
        {
            var dto = new GeneratorDataDto();
            dto.characters.Add(new CharacterDto { id = "chef", homeRoom = "nowhere", tags = { "cook" } });
            dto.methods.Add(new MethodDto { id = "m", time = "25:00" });

            var exception = Assert.Throws<GeneratorDataException>(() => DatabaseBuilder.Build(dto));
            Assert.IsTrue(exception.Errors.Count >= 3, exception.Message);
        }
    }
}
