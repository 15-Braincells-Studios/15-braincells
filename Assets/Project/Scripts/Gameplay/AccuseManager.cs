using UnityEngine;
using Project.Scripts.Core;
using System.Collections.Generic;
using TMPro;
public class AccuseManager : MonoBehaviour
{
    [System.Serializable]
    public class CharacterOption
    {
        public string characterID;
        public string displayName;
    }

    [Header("References")]
    public ClueManager clueManager;
    public GameObject victoryScreen;
    public GameObject gameoverScreen;
    public Timer timer;
    
    [Header("Dropdowns")]
    public TMP_Dropdown targetDropdown;
    public TMP_Dropdown culpritDropdown;

    [Header("Characters shown in the dropdowns")]
    public List<CharacterOption> characters = new List<CharacterOption>();

    [Header("Correct answers (characterID)")]
    public string correctTargetID;
    public string correctCulpritID;

    [Header("Optional")]
    [Tooltip("Keep your old rule: also require all clue items to be collected. Turn off until supporting evidence is implemented.")]
    public bool requireAllClues = false;

    private const string PlaceholderText = "{ Select Character }";
    
    private void Start()
    {
        PopulateDropdown(targetDropdown);
        PopulateDropdown(culpritDropdown);
    }
    
    private void PopulateDropdown(TMP_Dropdown dropdown)
    {
        if (dropdown == null) return;

        dropdown.ClearOptions();

        var options = new List<TMP_Dropdown.OptionData>
        {
            new TMP_Dropdown.OptionData(PlaceholderText)
        };

        foreach (var c in characters)
            options.Add(new TMP_Dropdown.OptionData(c.displayName));

        dropdown.AddOptions(options);
        dropdown.value = 0;
        dropdown.RefreshShownValue();
    }
    
    private string GetSelectedID(TMP_Dropdown dropdown)
    {
        if (dropdown == null || dropdown.value == 0) return null;
        return characters[dropdown.value - 1].characterID;
    }

    public void Accuse()
    {
        string selectedTarget = GetSelectedID(targetDropdown);
        string selectedCulprit = GetSelectedID(culpritDropdown);

        Debug.Log($"Target value: {targetDropdown.value}, Culprit value: {culpritDropdown.value}, " + $"Target ID: {selectedTarget ?? "null"}, Culprit ID: {selectedCulprit ?? "null"}");
        
        if (selectedTarget == null || selectedCulprit == null)
        {
            Debug.Log("Select both a target and a culprit before signing.");
            return;
        }

        bool targetCorrect = selectedTarget == correctTargetID;
        bool culpritCorrect = selectedCulprit == correctCulpritID;
        bool cluesOk = !requireAllClues || AllCluesCollected();

        bool won = targetCorrect && culpritCorrect && cluesOk;

        victoryScreen.SetActive(won);
        gameoverScreen.SetActive(!won);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        timer.StopTimer();
        Time.timeScale = 0f;
    }
    
    private bool AllCluesCollected()
    {
        InventoryItemData[] importantItems =
            FindObjectsByType<InventoryItemData>(FindObjectsSortMode.None);

        foreach (InventoryItemData item in importantItems)
        {
            if (bool.TryParse(Localization.Get(item.itemID + "_isClue"), out bool isClue) && isClue)
            {
                string itemName = Localization.Get(item.itemID + "_name");

                bool found = clueManager.clues.Exists(clue =>
                    clue.clueName == itemName &&
                    clue.clueType == "Item");

                if (!found) return false;
            }
        }
        return true;
    }
}