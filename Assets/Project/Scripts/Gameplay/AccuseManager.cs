using UnityEngine;
using Project.Scripts.Core;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using FifteenBraincells.MurderGenerator.Output;

public class AccuseManager : MonoBehaviour
{


    [Header("References")]
    public ClueManager clueManager;
    public ResultScreen resultScreen;
    public Timer timer;
    public AccuseDoor accuseDoor;
    
    [Header("Dropdowns")]
    public TMP_Dropdown targetDropdown;
    public TMP_Dropdown culpritDropdown;
    
    [Header("Evidence")]
    public EvidencePicker evidencePicker;
    [Tooltip("Points for each correct piece of evidence the player hands in (there are 5 per case).")]
    public int pointsPerEvidence = 500;

    [Header("Optional")]
    [Tooltip("Keep your old rule: also require all clue items to be collected. Turn off until supporting evidence is implemented.")]
    public bool requireAllClues = false;

    private const int PointsPerAnswer = 1000;
    private const string PlaceholderText = "{ Select Character }";
    private readonly List<string> optionIds = new List<string>();
    
    private bool IsOpen;
    
    private void Start()
    {
        BuildCharacterList();
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

        foreach (string id in optionIds)
            options.Add(new TMP_Dropdown.OptionData(CaseManager.Instance.GetDisplayName(id)));

        dropdown.AddOptions(options);
        dropdown.value = 0;
        dropdown.RefreshShownValue();
    }
    
    private string GetSelectedID(TMP_Dropdown dropdown)
    {
        if (dropdown == null || dropdown.value == 0) return null;
        int index = dropdown.value - 1;
        return index < optionIds.Count ? optionIds[index] : null;
    }

    public void Accuse()
    {
        string selectedTarget = GetSelectedID(targetDropdown);
        string selectedCulprit = GetSelectedID(culpritDropdown);
        
        if (selectedTarget == null || selectedCulprit == null)
        {
            Debug.Log("Select both a target and a culprit before signing.");
            return;
        }
        
        // Check if there is a case
        CaseManager cases = CaseManager.Instance;
        if (cases == null || !cases.HasCase)
        {
            Debug.LogError("AccuseManager: no case has been generated, can't check the accusation.");
            return;
        }
        
        // Get the answer from the generator
        Solution solution = cases.Level.Solution;
        string correctTargetID  = solution.Victim.Id;
        string correctCulpritID = solution.Suspect.Id;

        bool targetCorrect = selectedTarget == correctTargetID;
        bool culpritCorrect = selectedCulprit == correctCulpritID;
        
        int evidenceCorrect = CountCorrectEvidence();
        int evidencePoints = evidenceCorrect * pointsPerEvidence;

        int score = (targetCorrect ? PointsPerAnswer : 0) + (culpritCorrect ? PointsPerAnswer : 0) + evidencePoints;
        bool won = targetCorrect && culpritCorrect;

        accuseDoor.FinishAccusation();
        resultScreen.Show(won, targetCorrect, culpritCorrect, PointsPerAnswer, evidencePoints, score);
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        timer.StopTimer();
        Time.timeScale = 0f;
    }
    
    // How many of the selected clues are one of the case's 5 pieces of evidence.
    // Items and fixed dialogue lines have no generator clue, so they never count.
    private int CountCorrectEvidence()
    {
        if (evidencePicker == null)
            return 0;

        return evidencePicker.Selected
            .Where(clue => clue.generatorClue != null && clue.generatorClue.IsEvidence)
            .Select(clue => clue.generatorClue)
            .Distinct()
            .Count();
    }

    private bool AllCluesCollected()
    {
        InventoryItemData[] importantItems =
            FindObjectsByType<InventoryItemData>(FindObjectsSortMode.None);

        foreach (InventoryItemData item in importantItems)
        {
            string itemName = Localization.Get(item.itemID + "_name");

            bool found = clueManager.clues.Exists(clue =>
                clue.clueName == itemName &&
                clue.clueType == "Item");

            if (!found)
            {
                Debug.Log("Missing clue item: " + item.itemID);
                return false;
            }
        }

        return true;
    }
    
    private void BuildCharacterList()
    {
        optionIds.Clear();

        CaseManager cases = CaseManager.Instance;
        if (cases == null || cases.Database == null)
        {
            Debug.LogError("AccuseManager: no CaseManager or generator data, dropdowns will be empty.");
            return;
        }

        foreach (var character in cases.Database.Characters)
            optionIds.Add(character.Id);
    }
}