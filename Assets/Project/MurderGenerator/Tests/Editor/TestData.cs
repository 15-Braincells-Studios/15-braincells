using System.Linq;
using FifteenBraincells.MurderGenerator.Data;
using FifteenBraincells.MurderGenerator.Text;
using FifteenBraincells.MurderGenerator.Unity;
using UnityEditor;
using UnityEngine;

namespace FifteenBraincells.MurderGenerator.Tests
{
    /// <summary>Loads the real JSON files from the project once and shares them between tests.</summary>
    internal static class TestData
    {
        private static GeneratorDatabase _database;
        private static StringTable _strings;

        public static StringTable Strings => _strings ??= GeneratorDataLoader.LoadStrings(Find("strings_en"));

        public static GeneratorDatabase Database => _database ??= GeneratorDataLoader.LoadDatabase(Find("generator_data"), Strings);

        private static TextAsset Find(string fileName)
        {
            var path = AssetDatabase.FindAssets($"{fileName} t:TextAsset")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Single(p => System.IO.Path.GetFileNameWithoutExtension(p) == fileName);
            return AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        }
    }
}
