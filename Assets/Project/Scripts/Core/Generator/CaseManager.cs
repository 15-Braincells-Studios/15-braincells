using System.Collections.Generic;
using System.Linq;
using FifteenBraincells.MurderGenerator.Data;
using FifteenBraincells.MurderGenerator.Generation;
using FifteenBraincells.MurderGenerator.Output;
using FifteenBraincells.MurderGenerator.Text;
using FifteenBraincells.MurderGenerator.Unity;
using UnityEngine;
using GenClue = FifteenBraincells.MurderGenerator.Output.Clue;

namespace Project.Scripts.Core
{
    // Runs before other scripts, so the case exists before any NPC asks for it.
    [DefaultExecutionOrder(-100)]
    public class CaseManager : MonoBehaviour
    {
        public static CaseManager Instance { get; private set; }

        [Header("Generator data")]
        [SerializeField] private TextAsset generatorData;
        [SerializeField] private TextAsset strings;

        [Header("Seed")]
        [Tooltip("Off: a new random case every time you press Play.")]
        [SerializeField] private bool useFixedSeed;
        [SerializeField] private int seed = 2611;

        [Header("Fixed case")]
        [Tooltip("Optional: a hand-built case from MurderGenerator/Data/FixedCases. When set, it is played instead of a generated case, and the seed is ignored.")]
        [SerializeField] private TextAsset fixedCase;

        [Header("Debug")]
        [Tooltip("Logs the full case, including the solution, to the Console.")]
        [SerializeField] private bool logCase = true;

        public GeneratorDatabase Database { get; private set; }
        public ClueTextRenderer Renderer { get; private set; }
        public Level Level { get; private set; }
        public bool HasCase => Level != null;

        // Fires every time a new case has been generated.
        public event System.Action OnCaseGenerated;

        private void Awake()
        {
            // Check if there is already a CaseManager so no dupes, then make a new case
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("There is more than one CaseManager in the scene; removing this one.", this);
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (!LoadData())
                return;

            // Started from the main menu: the menu decides. Opened this scene directly: the Inspector field decides.
            var caseToLoad = SelectedCase.ChosenInMenu ? SelectedCase.FixedCase : fixedCase;

            if (caseToLoad != null)
                LoadFixedCase(caseToLoad);
            else
                NewCase(useFixedSeed ? seed : Random.Range(int.MinValue, int.MaxValue));
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        // Loads and checks the JSON.
        private bool LoadData()
        {
            if (generatorData == null || strings == null)
            {
                Debug.LogError("CaseManager: assign generator_data.json and strings_en.json.", this);
                return false;
            }

            try
            {
                var table = GeneratorDataLoader.LoadStrings(strings);
                Database = GeneratorDataLoader.LoadDatabase(generatorData, table);
                Renderer = new ClueTextRenderer(table);
                return true;
            }
            catch (GeneratorDataException e)
            {
                Debug.LogError("CaseManager: the generator data has problems:\n" + e.Message, this);
                return false;
            }
        }

        // Generates a new case. The same seed always gives the same case.
        public void NewCase(int newSeed)
        {
            var generator = new LevelGenerator(Database);

            // Almost never needed, but if a seed fails we try the next few.
            for (int i = 0; i < 5; i++)
            {
                if (generator.TryGenerate(newSeed + i, out var level))
                {
                    Level = level;
                    if (logCase)
                        Debug.Log(LevelReport.Describe(Level, Renderer), this);
                    OnCaseGenerated?.Invoke();
                    return;
                }
            }

            Debug.LogError($"CaseManager: could not generate a case from seed {newSeed}.", this);
        }

        // Plays a hand-built case (a JSON file in MurderGenerator/Data/FixedCases) instead of a generated one.
        public void LoadFixedCase(TextAsset caseJson)
        {
            try
            {
                Level = GeneratorDataLoader.LoadFixedCase(caseJson, Database);
            }
            catch (FixedCaseException e)
            {
                Debug.LogError($"CaseManager: fixed case '{caseJson.name}' has problems:\n" + e.Message, this);
                return;
            }

            if (logCase)
                Debug.Log(LevelReport.Describe(Level, Renderer), this);
            OnCaseGenerated?.Invoke();
        }

        [ContextMenu("New random case")]
        private void NewRandomCase() => NewCase(Random.Range(int.MinValue, int.MaxValue));

        // Finds a generator character by id, like "chef". Returns false for a typo instead of throwing.
        public bool TryGetCharacter(string characterId, out Character character)
        {
            character = Database?.Characters.FirstOrDefault(c => c.Id == characterId);
            return character != null;
        }

        // The clues one character says in this case. Empty if the id is unknown or there is no case.
        public List<GenClue> GetLinesFor(string characterId)
        {
            if (!HasCase || !TryGetCharacter(characterId, out var character))
                return new List<GenClue>();

            return Level.LinesOf(character).ToList();
        }

        // The finished sentence for a clue, like "The King has summoned me to the study tonight."
        public string Render(GenClue clue) => Renderer.Render(clue);

        // A room as used in a sentence, like "the library" or "the King's bedroom".
        public string RenderRoom(PlacedRoom room) => Renderer.RenderRoom(room);

        // The name shown above a character's dialogue, like "Head Guard".
        public string GetDisplayName(string characterId) =>
            TryGetCharacter(characterId, out var character) ? Renderer.RenderName(character) : characterId;
    }
}