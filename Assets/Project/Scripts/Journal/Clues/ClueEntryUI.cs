using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClueEntryUI : MonoBehaviour
{
    public TMP_Text clueNameText;
    public Button button;

    // Stores the clue information
    private ClueData clueData;
    private ClueListUI clueListUI;

    // Initializes this clue entry
    public void Setup(ClueData clue, ClueListUI listUI)
    {
        clueData = clue;
        clueListUI = listUI;

        clueNameText.text = clue.clueName;

        button.onClick.RemoveAllListeners();

        // Display clue details when clicked
        button.onClick.AddListener(() =>
        {
            clueListUI.ShowClue(clueData);
        });
    }
}