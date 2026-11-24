using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One row in the evidence list. Click to select or unselect the clue.
public class EvidenceEntryUI : MonoBehaviour
{
    public TMP_Text label;
    public Button button;
    public Image background;

    public Color normalColor = new Color(1f, 1f, 1f, 0f);                // see-through
    public Color selectedColor = new Color(0.55f, 0.40f, 0.20f, 0.25f);  // light brown

    private ClueData clue;
    private EvidencePicker picker;

    public void Setup(ClueData clueData, EvidencePicker evidencePicker)
    {
        clue = clueData;
        picker = evidencePicker;

        label.text = string.IsNullOrEmpty(clue.description)
            ? clue.clueName
            : clue.clueName + ": " + clue.description;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => SetSelected(picker.ToggleClue(clue)));

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (background != null)
            background.color = selected ? selectedColor : normalColor;
    }
}