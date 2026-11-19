using FifteenBraincells.MurderGenerator.Data;
using FifteenBraincells.MurderGenerator.Generation;
using FifteenBraincells.MurderGenerator.Output;
using FifteenBraincells.MurderGenerator.Text;
using UnityEngine;

namespace FifteenBraincells.MurderGenerator.Unity
{
    /// <summary>
    /// Proof of concept: generates a level and writes it to the Console. Put it on any GameObject,
    /// assign the two JSON files, and press Play (or use "Generate level" in the component's menu).
    /// </summary>
    public sealed class MurderGeneratorDemo : MonoBehaviour
    {
        [SerializeField] private TextAsset generatorData = default;
        [SerializeField] private TextAsset strings = default;

        [Tooltip("Off: a new random level every time.")]
        [SerializeField] private bool useFixedSeed = default;
        [SerializeField] private int seed = 2611;

        [Tooltip("Optional method id, e.g. 'poisoned_remedy'. Empty: random.")]
        [SerializeField] private string methodId = "";

        private GeneratorDatabase _database;
        private ClueTextRenderer _renderer;

        private void Start() => GenerateLevel();

        [ContextMenu("Generate level")]
        private void GenerateLevel()
        {
            if (!EnsureLoaded())
            {
                return;
            }

            Method method = null;
            if (methodId.Length > 0 && !_database.TryGetMethod(methodId, out method))
            {
                Debug.LogError($"Unknown method id '{methodId}'.", this);
                return;
            }

            var levelSeed = useFixedSeed ? seed : Random.Range(int.MinValue, int.MaxValue);
            Level level = new LevelGenerator(_database).Generate(levelSeed, method);
            Debug.Log(LevelReport.Describe(level, _renderer), this);
        }

        private bool EnsureLoaded()
        {
            if (_database != null)
            {
                return true;
            }

            if (generatorData == null || strings == null)
            {
                Debug.LogError("Assign generator_data.json and strings_en.json first.", this);
                return false;
            }

            var table = GeneratorDataLoader.LoadStrings(strings);
            _database = GeneratorDataLoader.LoadDatabase(generatorData, table);
            _renderer = new ClueTextRenderer(table);
            return true;
        }
    }
}
