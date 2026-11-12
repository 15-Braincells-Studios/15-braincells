using UnityEngine;
using TMPro;

public class ClueListUI : MonoBehaviour
{
    public GameObject clueEntryPrefab;
    public Transform clueList;
    public ClueManager clueManager;
    
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
    // Refreshes the visible clues using the active filter
    public void RefreshClueList()
    {
        foreach (Transform child in clueList)
        {
            Destroy(child.gameObject);
        }

        foreach (ClueData clue in clueManager.clues)
        {
            {
                AddClueToList(clue);
            }
        }
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