using System.Linq;
using FifteenBraincells.MurderGenerator.Text;
using Project.Scripts.Core;
using TMPro;
using UnityEngine;
using GenClue = FifteenBraincells.MurderGenerator.Output.Clue;

// A note, letter, document, item or visual clue from the generator.
// Two ways to use it:
// - On a prefab for ClueSpawner: leave Clue Id empty. The spawner puts it on a ClueSpawnPoint and fills it.
// - Placed by hand in the level: fill in Clue Id. It shows that clue, and hides itself when the case doesn't have it.
// The player inspects it with E (it needs the "inspect" tag and a collider); that shows the text and puts it in the journal.
// The text is never readable in the level itself.
public class GeneratedNote : MonoBehaviour
{
    [Tooltip("Only when placed by hand: row id in generator_data.json, like \"forbidden_experiments\" or \"poisoned_wine_b1\". " +
             "The case log in the Console lists every object per room with its id in [brackets].")]
    public string clueId;

    [Tooltip("Optional text on the paper itself. It stays empty in the level and is only filled on the inspect view. " +
             "Left empty: the first TMP text in the children, if there is one. Not needed when the inspect panel shows the text.")]
    public TMP_Text text;

    [Tooltip("Name in the inspect view and the journal, like Note, Letter or Dagger. The room is added in the journal.")]
    public string title = "Note";

    [Tooltip("Picture in the journal. Optional.")]
    public Sprite icon;

    [Tooltip("Only when placed by hand: hide this object when the current case doesn't have this clue.")]
    public bool hideWhenNotInCase = true;

    [Header("Inspect view")]
    [Tooltip("Let the player turn it with the mouse. Off for notes and letters, on for items like a vial.")]
    public bool canRotate;

    [Tooltip("Only when it can't be turned: how it faces the camera. At 0,0,0 the camera looks along its blue (forward) axis, " +
             "the way a TextMeshPro text with the same rotation reads correctly. A paper lying flat with its text up: -90, 0, 0.")]
    public Vector3 inspectRotation;

    // The generator clue behind this object, or null when this case doesn't have it
    public GenClue Clue { get; private set; }

    // The whole finished sentence, like: Note on the kitchen door: "Gone to the market." For the journal.
    public string NoteText { get; private set; } = "";

    // What the object is, like "Note on the kitchen door"
    public string Description { get; private set; } = "";

    // Only the words written on it, like "Gone to the market." Empty when nothing is written on it (a vial, a stain).
    public string Writing { get; private set; } = "";

    // What the player reads in the inspect view: the written words, or the description when nothing is written on it
    public string ReadText => Writing != "" ? Writing : NoteText;

    // The heading in the inspect view: the description when it has written words under it, else the title
    public string InspectTitle => Writing != "" && Description != "" ? Description : title;

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
        (Description, Writing) = ClueTextRenderer.SplitObjectText(NoteText);

        // unreadable in the level: the text only shows when the player inspects it
        if (text != null)
            text.text = "";
    }

    // Called by ItemInspector on the copy on the inspect stage, so the paper shows the text there
    public void ShowTextOnCopy(GeneratedNote copy)
    {
        if (copy != null && copy.text != null)
            copy.text.text = ReadText;
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
