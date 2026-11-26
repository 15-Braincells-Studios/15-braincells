using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EvidencePicker : MonoBehaviour
{
    public ClueManager clueManager;
    [Tooltip("Optional: found automatically if left empty. Its conversation lines are listed as evidence too.")]
    public ConversationListUI conversationListUI;
    public Transform listParent;
    public EvidenceEntryUI entryPrefab;
    public TMP_Text selectedText;

    [Tooltip("Every case has exactly 5 pieces of evidence: when/where, method, fake alibi, contradiction, motive.")]
    public int maxSelected = 5;

    public List<ClueData> Selected { get; } = new List<ClueData>();

    public bool HasSelected(string clueID) => Selected.Exists(c => c.clueID == clueID);

    void OnEnable() => Rebuild();

    public void Rebuild()
    {
        foreach (Transform child in listParent)
            Destroy(child.gameObject);

        Selected.Clear();

        // Clues from the journal's clue page (items)
        if (clueManager != null)
        {
            foreach (ClueData clue in clueManager.clues)
                AddEntry(clue);
        }

        // Every line from the journal's conversations tab (the journal can be inactive, so include inactive)
        if (conversationListUI == null)
            conversationListUI = FindFirstObjectByType<ConversationListUI>(FindObjectsInactive.Include);

        if (conversationListUI != null)
        {
            foreach (ClueData clue in conversationListUI.ConversationClues)
                AddEntry(clue);
        }

        RefreshText();
    }

    private void AddEntry(ClueData clue)
    {
        EvidenceEntryUI entry = Instantiate(entryPrefab, listParent);
        entry.Setup(clue, this);
    }

    // Called by a row when it's clicked. Returns true if the clue is now selected.
    public bool ToggleClue(ClueData clue)
    {
        if (Selected.Contains(clue))
        {
            Selected.Remove(clue);
        }
        else if (Selected.Count < maxSelected)
        {
            Selected.Add(clue);
        }

        RefreshText();
        return Selected.Contains(clue);
    }

    void RefreshText()
    {
        if (selectedText != null)
            selectedText.text = $"[{Selected.Count}/{maxSelected}]";
    }
}