using UnityEngine;
using TMPro;

public class ClueListUI : MonoBehaviour
{
    public GameObject clueEntryPrefab;
    public Transform clueList;

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

    // Displays the selected clue on the right page
    public void ShowClue(ClueData clue)
    {
        clueImage.sprite = clue.clueImage;
        clueImage.enabled = clue.clueImage != null;

        clueName.text = clue.clueName;
        clueDescription.text = clue.description;
    }
}