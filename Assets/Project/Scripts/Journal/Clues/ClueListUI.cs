using UnityEngine;
using TMPro;

public class ClueListUI : MonoBehaviour
{
    public GameObject clueEntryPrefab;
    public Transform clueList;
    public ClueManager clueManager;

    private string currentFilter = "All";
    public TMP_Text categoryLabel;
    public UnityEngine.UI.Image clueImage;
    public TMP_Text clueName;
    public TMP_Text clueDescription;

    // Creates a new button for a discovered clue
    public void AddClueToList(ClueData clue)
    {
        GameObject newEntry = Instantiate(clueEntryPrefab, clueList);
        ClueEntryUI entryUI = newEntry.GetComponent<ClueEntryUI>();
        entryUI.Setup(clue, this);
    }
// Shows only clues matching the selected category
    // Shows only clues matching the selected category
    public void FilterClues(string filter)
    {
        currentFilter = filter;

        // Remove currently visible clue entries
        foreach (Transform child in clueList)
        {
            Destroy(child.gameObject);
        }

        // Recreate entries matching the selected category
        foreach (ClueData clue in clueManager.clues)
        {
            if (clue.clueType == currentFilter)
            {
                AddClueToList(clue);
            }
        }

        categoryLabel.text = filter == "Item"
            ? "Item Clues"
            : "Conversation Clues";
    }
    // Refreshes the visible clues using the active filter
    public void RefreshClueList()
    {
        foreach (Transform child in clueList)
        {
            Destroy(child.gameObject);
        }

        foreach (ClueData clue in clueManager.clues)
        {
            if (currentFilter == "All" || clue.clueType == currentFilter)
            {
                AddClueToList(clue);
            }
        }
    }
    // Show important item clues
    public void ShowItems()
    {
        FilterClues("Item");
    }

// Show important dialogue clues
    public void ShowDialogue()
    {
        FilterClues("Dialogue");
    }
    // Show all discovered clues
    public void ShowAllClues()
    {
        currentFilter = "All";

        foreach (Transform child in clueList)
        {
            Destroy(child.gameObject);
        }

        foreach (ClueData clue in clueManager.clues)
        {
            AddClueToList(clue);
        }

        categoryLabel.text = "All Clues";
    }
    // Displays the selected clue on the right page
    public void ShowClue(ClueData clue)
    {
        clueImage.sprite = clue.clueImage;
        clueImage.enabled = clue.clueImage != null;

        clueName.text = clue.clueName;
        clueDescription.text = clue.description;
    }
}