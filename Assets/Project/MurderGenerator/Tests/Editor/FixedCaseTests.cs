using System.Linq;
using FifteenBraincells.MurderGenerator.Generation;
using FifteenBraincells.MurderGenerator.Text;
using FifteenBraincells.MurderGenerator.Unity;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace FifteenBraincells.MurderGenerator.Tests
{
    /// <summary>Every hand-built case in Data/FixedCases must still build and render with the current data.</summary>
    [TestFixture]
    public sealed class FixedCaseTests
    {
        private static TextAsset[] FixedCases() =>
            AssetDatabase.FindAssets("t:TextAsset", new[] { "Assets/Project/MurderGenerator/Data/FixedCases" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.EndsWith(".json"))
                .Select(AssetDatabase.LoadAssetAtPath<TextAsset>)
                .ToArray();

        [Test]
        public void EveryFixedCaseBuildsAndRenders()
        {
            var cases = FixedCases();
            Assert.That(cases, Is.Not.Empty, "No fixed cases found.");

            var renderer = new ClueTextRenderer(TestData.Strings);
            foreach (var caseJson in cases)
            {
                try
                {
                    var level = GeneratorDataLoader.LoadFixedCase(caseJson, TestData.Database);
                    foreach (var clue in level.Clues)
                    {
                        var text = renderer.Render(clue);
                        Assert.That(text, Does.Not.Contain("{"), $"{caseJson.name}: unfilled placeholder in '{text}'.");
                    }
                }
                catch (FixedCaseException e)
                {
                    Assert.Fail($"{caseJson.name}: {e.Message}");
                }
            }
        }
    }
}
