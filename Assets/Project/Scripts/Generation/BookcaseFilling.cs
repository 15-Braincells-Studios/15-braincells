using System.Collections.Generic;
using UnityEngine;

public class BookcaseFilling : MonoBehaviour
{
    [System.Serializable]
    public class ShelfRow
    {
        public Transform start;
        public Transform end;
    }

    [SerializeField] private GameObject[] bookPrefabs;   // normal books only, without clues
    [SerializeField] private GameObject redBookPrefab;   // exactly 1 per bookcase, with clue
    [SerializeField] private ShelfRow[] rows;
    [SerializeField] private float gap;

    void Start()
    {
        // 1. plan every row (no spawning yet)
        List<GameObject>[] plan = new List<GameObject>[rows.Length];
        for (int i = 0; i < rows.Length; i++)
            plan[i] = PlanRow(RowLength(rows[i]));

        // 2. swap exactly one book for the red one
        AddRedBook(plan);

        // 3. spawn everything from the plan
        for (int i = 0; i < rows.Length; i++)
            SpawnRow(rows[i], plan[i]);
    }

    // ---------- planning ----------

    List<GameObject> PlanRow(float length)
    {
        List<GameObject> list = new List<GameObject>();
        float x = 0f;

        while (true)
        {
            GameObject prefab = PickRandomBook();
            float width = GetWidth(prefab);
            if (width <= 0f || x + width > length) break;

            list.Add(prefab);
            x += width + gap;
        }

        return list;
    }

    void AddRedBook(List<GameObject>[] plan)
    {
        // only rows that actually have books can get the red one
        List<int> candidates = new List<int>();
        for (int i = 0; i < plan.Length; i++)
            if (plan[i].Count > 0) candidates.Add(i);

        if (candidates.Count == 0)
        {
            Debug.LogWarning("No room for the red book in this bookcase!");
            return;
        }

        int rowIndex = candidates[Random.Range(0, candidates.Count)];
        List<GameObject> row = plan[rowIndex];

        int spot = Random.Range(0, row.Count);
        row[spot] = redBookPrefab;

        // the red book might be wider than the one it replaced:
        // remove books from the end until it fits again (never the red one)
        float length = RowLength(rows[rowIndex]);
        while (UsedLength(row) > length && row.Count > 1)
        {
            int last = row.Count - 1;
            if (row[last] == redBookPrefab) last--;
            row.RemoveAt(last);
        }
    }

    GameObject PickRandomBook()
    {
        return bookPrefabs[Random.Range(0, bookPrefabs.Length)];
    }

    // ---------- spawning ----------

    void SpawnRow(ShelfRow row, List<GameObject> prefabs)
    {
        Vector3 direction = (row.end.position - row.start.position).normalized;
        Quaternion rotation = row.start.rotation;

        float x = 0f;
        foreach (GameObject prefab in prefabs)
        {
            float width = GetWidth(prefab);
            Vector3 pos = row.start.position + direction * (x + width / 2f);

            GameObject book = Instantiate(prefab, pos, rotation, row.start);
            AlignBook(book, row);

            x += width + gap;
        }
    }

    void AlignBook(GameObject book, ShelfRow row)
    {
        Renderer rend = book.GetComponentInChildren<Renderer>();
        if (rend == null) return;
        Bounds b = rend.bounds;

        // bottom of the book onto the shelf
        float up = row.start.position.y - b.min.y;

        // front of the book onto the marker line
        Vector3 outward = -row.start.forward; // flip the sign if the wrong side lines up
        Vector3 absOut = new Vector3(Mathf.Abs(outward.x), Mathf.Abs(outward.y), Mathf.Abs(outward.z));
        float half = Vector3.Dot(b.extents, absOut);
        float bookFront = Vector3.Dot(b.center, outward) + half;
        float lineFront = Vector3.Dot(row.start.position, outward);

        book.transform.position += Vector3.up * up + outward * (lineFront - bookFront);
    }

    // ---------- helpers ----------

    float RowLength(ShelfRow row)
    {
        return Vector3.Distance(row.start.position, row.end.position);
    }

    float UsedLength(List<GameObject> row)
    {
        float total = 0f;
        foreach (GameObject prefab in row) total += GetWidth(prefab);
        return total + gap * Mathf.Max(0, row.Count - 1);
    }

    float GetWidth(GameObject prefab)
    {
        MeshFilter meshFilter = prefab.GetComponentInChildren<MeshFilter>();
        return meshFilter.sharedMesh.bounds.size.x * prefab.transform.localScale.x;
    }
}