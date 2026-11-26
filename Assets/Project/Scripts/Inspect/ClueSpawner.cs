using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FifteenBraincells.MurderGenerator.Data;
using FifteenBraincells.MurderGenerator.Generation;
using Project.Scripts.Core;
using UnityEngine;
using GenClue = FifteenBraincells.MurderGenerator.Output.Clue;

// Puts every object clue of the current case (notes, documents, items, visuals) on a ClueSpawnPoint in its room.
// One per scene. It refills when a new case is made.
public class ClueSpawner : MonoBehaviour
{
    [Serializable]
    public class PrefabOverride
    {
        [Tooltip("Row id in generator_data.json, like burned_letter or poisoned_wine_b1")]
        public string clueId;
        public GeneratedNote prefab;
    }

    [Header("Prefab per kind")]
    [Tooltip("A sheet of paper. Needs a GeneratedNote, a collider and a TMP text.")]
    public GeneratedNote notePrefab;
    [Tooltip("A letter, scroll or ledger. Empty: the note prefab is used.")]
    public GeneratedNote documentPrefab;
    [Tooltip("Used for items that have no override below. Empty: the item is skipped with a warning.")]
    public GeneratedNote itemPrefab;
    [Tooltip("Used for visuals (stains, marks) that have no override below. Empty: skipped with a warning.")]
    public GeneratedNote visualPrefab;

    [Header("Own model for one clue")]
    public List<PrefabOverride> overrides = new List<PrefabOverride>();

    private readonly List<GameObject> spawned = new List<GameObject>();

    void Start()
    {
        if (CaseManager.Instance == null)
        {
            Debug.LogWarning("ClueSpawner: no CaseManager in this scene.", this);
            return;
        }

        CaseManager.Instance.OnCaseGenerated += Spawn;
        if (CaseManager.Instance.HasCase)
            Spawn();
    }

    void OnDestroy()
    {
        if (CaseManager.Instance != null)
            CaseManager.Instance.OnCaseGenerated -= Spawn;
    }

    void Spawn()
    {
        Clear();

        CaseManager cases = CaseManager.Instance;
        if (!cases.HasCase)
            return;

        ClueSpawnPoint[] points = FindObjectsByType<ClueSpawnPoint>(FindObjectsSortMode.None);
        CheckPoints(points, cases.Database);

        // the same case always gets the same layout: sort the points, then pick with the case's seed
        List<ClueSpawnPoint> free = points.OrderBy(p => p.transform.position.x)
            .ThenBy(p => p.transform.position.y)
            .ThenBy(p => p.transform.position.z)
            .ToList();
        var rng = new Rng(cases.Level.Seed);

        // clues a hand-placed GeneratedNote already shows are left alone
        var handPlaced = new HashSet<string>(
            FindObjectsByType<GeneratedNote>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(n => !n.IsSpawned && !string.IsNullOrEmpty(n.clueId))
                .Select(n => n.clueId));

        var log = new StringBuilder("ClueSpawner: objects for this case\n");
        int missing = 0;

        foreach (GenClue clue in cases.Level.Clues.Where(c => !c.IsSpoken && c.Room != null))
        {
            string where = clue.Room.ToString();

            if (handPlaced.Contains(clue.SourceId))
            {
                log.AppendLine($"  {clue.SourceId} ({clue.Form}) in {where}: hand-placed");
                continue;
            }

            GeneratedNote prefab = PrefabFor(clue);
            if (prefab == null)
            {
                Debug.LogWarning($"ClueSpawner: no prefab for {clue.Form} '{clue.SourceId}'. Add one under 'Prefab per kind' or 'Own model for one clue'.", this);
                missing++;
                continue;
            }

            ClueSpawnPoint point = TakePoint(free, clue, rng);
            if (point == null)
            {
                Debug.LogError($"ClueSpawner: no free ClueSpawnPoint for {clue.Form} '{clue.SourceId}' in {where}. " +
                               "Add one, or the case may be impossible to solve.", this);
                missing++;
                continue;
            }

            GeneratedNote note = Instantiate(prefab, point.transform.position, point.transform.rotation, point.transform);
            note.Assign(clue);
            spawned.Add(note.gameObject);
            log.AppendLine($"  {clue.SourceId} ({clue.Form}) in {where}: {point.name}");
        }

        if (missing > 0)
            log.AppendLine($"  {missing} object(s) could not be placed, see the warnings above.");
        Debug.Log(log.ToString(), this);
    }

    void Clear()
    {
        foreach (GameObject obj in spawned)
            if (obj != null)
                Destroy(obj);
        spawned.Clear();
    }

    GeneratedNote PrefabFor(GenClue clue)
    {
        PrefabOverride own = overrides.FirstOrDefault(o => o.prefab != null && o.clueId == clue.SourceId);
        if (own != null)
            return own.prefab;

        switch (clue.Form)
        {
            case ClueForm.Note: return notePrefab;
            case ClueForm.Document: return documentPrefab != null ? documentPrefab : notePrefab;
            case ClueForm.Item: return itemPrefab;
            case ClueForm.Visual: return visualPrefab;
            default: return null;
        }
    }

    // A random free point in the clue's room that takes its kind of object.
    // If the room has free points but none of the right kind, one of those is used, with a warning.
    static ClueSpawnPoint TakePoint(List<ClueSpawnPoint> free, GenClue clue, Rng rng)
    {
        string roomId = clue.Room.Room.Id;
        string ownerId = clue.Room.Owner?.Id ?? "";

        // only bedrooms have an owner: there is one "bedroom" room id for every character's bedroom
        List<ClueSpawnPoint> inRoom = free.Where(p => p.roomId == roomId &&
                                                      (roomId != RoomRef.Bedroom || p.ownerId == ownerId)).ToList();
        List<ClueSpawnPoint> fitting = inRoom.Where(p => p.Accepts(clue.Form)).ToList();

        if (fitting.Count == 0 && inRoom.Count > 0)
        {
            Debug.LogWarning($"ClueSpawner: no {clue.Form} spawn point in {clue.Room}, using another point in that room for '{clue.SourceId}'.");
            fitting = inRoom;
        }

        if (fitting.Count == 0)
            return null;

        ClueSpawnPoint point = fitting[rng.Next(fitting.Count)];
        free.Remove(point);
        return point;
    }

    // Typos in the Inspector would make clues silently disappear, so they are reported once at the start.
    void CheckPoints(ClueSpawnPoint[] points, GeneratorDatabase database)
    {
        var rooms = new HashSet<string>(database.Rooms.Select(r => r.Id));
        var characters = new HashSet<string>(database.Characters.Select(c => c.Id));

        foreach (ClueSpawnPoint p in points)
        {
            if (!rooms.Contains(p.roomId))
                Debug.LogError($"ClueSpawnPoint '{p.name}': unknown room id '{p.roomId}'.", p);
            else if (p.roomId == RoomRef.Bedroom && !characters.Contains(p.ownerId))
                Debug.LogError($"ClueSpawnPoint '{p.name}': a bedroom needs an owner, like king or chef (now '{p.ownerId}').", p);
            else if (p.roomId != RoomRef.Bedroom && !string.IsNullOrEmpty(p.ownerId))
                Debug.LogWarning($"ClueSpawnPoint '{p.name}': only bedrooms have an owner; '{p.ownerId}' is ignored.", p);
        }
    }
}
