using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EvidencePicker : MonoBehaviour
{
    public ClueManager clueManager;
    public Transform listParent;
    public Toggle togglePrefab;
    public TMP_Text selectedText;
    public int maxSelected = 3;

    public List<ClueData> Selected { get; } = new List<ClueData>();

    public bool HasSelected(string clueID) => Selected.Exists(c => c.clueID == clueID);

    void OnEnable() => Rebuild();

    public void Rebuild()
    {
        foreach (Transform child in listParent)
            Destroy(child.gameObject);

        Selected.Clear();

        foreach (ClueData clue in clueManager.clues)
        {
            Toggle toggle = Instantiate(togglePrefab, listParent);
            toggle.GetComponentInChildren<TMP_Text>().text = clue.clueName;
            toggle.isOn = false;
            toggle.onValueChanged.AddListener(isOn => OnToggled(clue, isOn, toggle));
        }

        RefreshText();
    }

    void OnToggled(ClueData clue, bool isOn, Toggle toggle)
    {
        if (isOn)
        {
            if (Selected.Count >= maxSelected)
            {
                toggle.SetIsOnWithoutNotify(false);
                return;
            }
            Selected.Add(clue);
        }
        else
        {
            Selected.Remove(clue);
        }

        RefreshText();
    }

    void RefreshText()
    {
        selectedText.text = "Selected Evidence: " + string.Join(", ", Selected.ConvertAll(c => c.clueName));
    }
}