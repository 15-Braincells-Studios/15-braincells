using System;
using FifteenBraincells.MurderGenerator.Data;
using UnityEngine;

// Which kinds of generator objects a spawn point can hold. Several can be ticked.
[Flags]
public enum ClueFormMask
{
    Note = 1 << 0,
    Document = 1 << 1,
    Item = 1 << 2,
    Visual = 1 << 3,
}

// An empty spot in a room where ClueSpawner can put a note, document, item or visual clue for the current case.
// Put it where the object should lie (on a desk, a shelf, the floor). The blue arrow is the object's forward.
public class ClueSpawnPoint : MonoBehaviour
{
    [Tooltip("Generator room id, like kitchen, library or bedroom. Pick it from the list.")]
    public string roomId = "";

    [Tooltip("Only for room 'bedroom': whose bedroom this is, like king or chef.")]
    public string ownerId = "";

    [Tooltip("What can lie here. A desk takes notes and documents, the floor takes visuals.")]
    public ClueFormMask forms = ClueFormMask.Note | ClueFormMask.Document;

    public bool Accepts(ClueForm form)
    {
        switch (form)
        {
            case ClueForm.Note: return (forms & ClueFormMask.Note) != 0;
            case ClueForm.Document: return (forms & ClueFormMask.Document) != 0;
            case ClueForm.Item: return (forms & ClueFormMask.Item) != 0;
            case ClueForm.Visual: return (forms & ClueFormMask.Visual) != 0;
            default: return false;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.75f, 0.2f, 0.9f);
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.05f, new Vector3(0.3f, 0.1f, 0.2f));
        Gizmos.color = new Color(0.3f, 0.6f, 1f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.25f);
    }
}
