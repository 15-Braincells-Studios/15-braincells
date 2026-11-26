using FifteenBraincells.MurderGenerator.Data;
using FifteenBraincells.MurderGenerator.Data.Dto;
using FifteenBraincells.MurderGenerator.Text;
using UnityEngine;

namespace FifteenBraincells.MurderGenerator.Unity
{
    /// <summary>
    /// Reads the JSON files with Unity's JsonUtility. This is the only place that knows about JSON;
    /// the generator itself works on the DTOs and has no Unity dependency.
    /// </summary>
    public static class GeneratorDataLoader
    {
        public static StringTable LoadStrings(TextAsset stringsJson) =>
            new StringTable(JsonUtility.FromJson<StringTableDto>(stringsJson.text));

        /// <exception cref="GeneratorDataException">The data has problems; the message lists all of them.</exception>
        public static GeneratorDatabase LoadDatabase(TextAsset generatorDataJson, IStringTable strings) =>
            DatabaseBuilder.Build(JsonUtility.FromJson<GeneratorDataDto>(generatorDataJson.text), strings);

        /// <summary>Builds a hand-made case (Data/FixedCases) instead of generating one.</summary>
        /// <exception cref="Generation.FixedCaseException">The case has problems; the message lists all of them.</exception>
        public static Output.Level LoadFixedCase(TextAsset fixedCaseJson, GeneratorDatabase database) =>
            Generation.FixedCaseBuilder.Build(database, JsonUtility.FromJson<FixedCaseDto>(fixedCaseJson.text));
    }
}
