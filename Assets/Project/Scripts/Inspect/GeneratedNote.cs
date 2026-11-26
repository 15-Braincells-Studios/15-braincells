using System.Linq;
using Project.Scripts.Core;
using TMPro;
using UnityEngine;
using GenClue = FifteenBraincells.MurderGenerator.Output.Clue;

// Put this on a note, letter or document in the level.
// It fills the text on the paper with the clue the generator made for the current case.
public class GeneratedNote : MonoBehaviour
{
    [Tooltip("Row id in generator_data.json, like \"forbidden_experiments\" or \"poisoned_wine_b1\". " +
             "The case log in the Console lists every object per room with its id in [brackets].")]
    public string clueId;

    [Tooltip("The text on the paper. Left empty: the first TMP text in the children.")]
    public TMP_Text text;

    [Tooltip("Hide this object when the current case doesn't have this clue.")]
    public bool hideWhenNotInCase = true;

    // The generator clue behind this note, or null when this case doesn't have it
    public GenClue Clue { get; private set; }

    // The finished sentence, for a plain-text view or the journal
    public string NoteText { get; private set; } = "";

    void Awake()
    {
        if (text == null)
            text = GetComponentInChildren<TMP_Text>(true);
    }

    void Start()
    {
        if (CaseManager.Instance == null)
        {
            Debug.LogWarning("GeneratedNote: no CaseManager in this scene.", this);
            return;
        }

        // also refill when a new case is made (context menu "New random case")
        CaseManager.Instance.OnCaseGenerated += Refresh;
        Refresh();
    }

    void OnDestroy()
    {
        if (CaseManager.Instance != null)
            CaseManager.Instance.OnCaseGenerated -= Refresh;
    }

    void Refresh()
    {
        CaseManager cases = CaseManager.Instance;

        // only objects: an alibi id can also have a spoken line
        Clue = cases.HasCase
            ? cases.Level.Clues.FirstOrDefault(c => !c.IsSpoken && c.SourceId == clueId)
            : null;

        NoteText = Clue != null ? cases.Render(Clue) : "";

        if (text != null)
            text.text = NoteText;

        if (hideWhenNotInCase)
            gameObject.SetActive(Clue != null);
        else if (Clue == null)
            Debug.LogWarning($"GeneratedNote: the case has no object clue '{clueId}'.", this);
    }
}
