using System.Linq;
using Project.Scripts.Core;
using TMPro;
using UnityEngine;
using GenClue = FifteenBraincells.MurderGenerator.Output.Clue;

// A note, letter, document, item or visual clue from the generator.
// Two ways to use it:
// - On a prefab for ClueSpawner: leave Clue Id empty. The spawner puts it on a ClueSpawnPoint and fills it.
// - Placed by hand in the level: fill in Clue Id. It shows that clue, and hides itself when the case doesn't have it.
// The player inspects it with E (it needs the "inspect" tag and a collider); that puts it in the journal.
public class GeneratedNote : MonoBehaviour
{
    [Tooltip("Only when placed by hand: row id in generator_data.json, like \"forbidden_experiments\" or \"poisoned_wine_b1\". " +
             "The case log in the Console lists every object per room with its id in [brackets].")]
    public string clueId;

    [Tooltip("The text on the paper. Left empty: the first TMP text in the children. Items without text can leave this empty.")]
    public TMP_Text text;

    [Tooltip("Name in the inspect view and the journal, like Note, Letter or Dagger. The room is added in the journal.")]
    public string title = "Note";

    [Tooltip("Picture in the journal. Optional.")]
    public Sprite icon;

    [Tooltip("Only when placed by hand: hide this object when the current case doesn't have this clue.")]
    public bool hideWhenNotInCase = true;

    // The generator clue behind this object, or null when this case doesn't have it
    public GenClue Clue { get; private set; }

    // The finished sentence, for a plain-text view or the journal
    public string NoteText { get; private set; } = "";

    // Made by ClueSpawner instead of placed by hand
    public bool IsSpawned { get; private set; }

    void Awake()
    {
        if (text == null)
            text = GetComponentInChildren<TMP_Text>(true);
    }

    // ItemInspector and the crosshair look for this tag
    void Reset()
    {
        gameObject.tag = "inspect";
    }

    void Start()
    {
        // ClueSpawner already filled it
        if (IsSpawned)
            return;

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

    // Called by ClueSpawner right after it made this object
    public void Assign(GenClue clue)
    {
        IsSpawned = true;
        Show(clue);
    }

    void Refresh()
    {
        CaseManager cases = CaseManager.Instance;

        // only objects: an alibi id can also have a spoken line
        GenClue clue = cases.HasCase
            ? cases.Level.Clues.FirstOrDefault(c => !c.IsSpoken && c.SourceId == clueId)
            : null;

        Show(clue);

        if (hideWhenNotInCase)
            gameObject.SetActive(Clue != null);
        else if (Clue == null)
            Debug.LogWarning($"GeneratedNote: the case has no object clue '{clueId}'.", this);
    }

    void Show(GenClue clue)
    {
        Clue = clue;
        NoteText = clue != null ? CaseManager.Instance.Render(clue) : "";

        if (text != null)
            text.text = NoteText;
    }

    // The journal entry for this object. Null when this case doesn't have it.
    public ClueData ToClueData()
    {
        if (Clue == null)
            return null;

        string room = CaseManager.Instance.RenderRoom(Clue.Room);

        return new ClueData
        {
            // the room is part of the id: the same row can lie in two bedrooms in one case
            clueID = $"object_{Clue.Category}_{Clue.SourceId}_{Clue.Room}",
            clueName = $"{title} ({room})",
            description = NoteText,
            clueImage = icon,
            clueType = Clue.Form.ToString(),
            generatorClue = Clue
        };
    }
}
