using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ColliderCleaner : EditorWindow
{
    [MenuItem("Tools/Remove Duplicate & Missing Mesh Colliders")]
    public static void RemoveDuplicatesAndMissing()
    {
        // Pakt alle geselecteerde objecten in de Hierarchy (inclusief hun 'children')
        GameObject[] selectedObjects = Selection.gameObjects;
        
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("Selecteer eerst de GameObjects in de Hierarchy die je wilt opschonen!");
            return;
        }

        int duplicateRemovedCount = 0;
        int missingRemovedCount = 0;

        foreach (GameObject rootObj in selectedObjects)
        {
            // Zoek alle MeshColliders in dit object en alle sub-objecten
            MeshCollider[] allColliders = rootObj.GetComponentsInChildren<MeshCollider>(true);
            
            // Houd per GameObject bij welke unieke meshes al een collider hebben
            Dictionary<GameObject, HashSet<Mesh>> trackedColliders = new Dictionary<GameObject, HashSet<Mesh>>();

            foreach (MeshCollider collider in allColliders)
            {
                GameObject go = collider.gameObject;

                // 1. Controleer of de mesh ontbreekt (Missing Mesh)
                if (collider.sharedMesh == null)
                {
                    Undo.DestroyObjectImmediate(collider);
                    missingRemovedCount++;
                    continue; // Ga direct door naar de volgende collider
                }

                if (!trackedColliders.ContainsKey(go))
                {
                    trackedColliders[go] = new HashSet<Mesh>();
                }

                // 2. Controleer of de collider een duplicaat is op DIT specifieke GameObject
                if (trackedColliders[go].Contains(collider.sharedMesh))
                {
                    Undo.DestroyObjectImmediate(collider);
                    duplicateRemovedCount++;
                }
                else
                {
                    // Registreer deze mesh zodat een volgende identieke mesh op DIT object wordt herkend
                    trackedColliders[go].Add(collider.sharedMesh);
                }
            }
        }

        // Toon het resultaat netjes in de Console
        Debug.Log($"[Collider Tool] Opschonen voltooid! Verwijderd: {missingRemovedCount} met ontbrekende mesh, {duplicateRemovedCount} duplicaten.");
    }

    // Voegt een MeshCollider toe aan elk object met een MeshFilter (met OF zonder MeshRenderer),
    // en past de mesh aan op bestaande colliders als die niet meer klopt (na overschrijven van een asset).
    [MenuItem("Tools/Add/Sync Mesh Colliders (incl. no MeshRenderer)")]
    public static void AddAndSyncColliders()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("Selecteer eerst de GameObjects in de Hierarchy!");
            return;
        }

        int added = 0, fixedMesh = 0, skipped = 0, excluded = 0;

        foreach (GameObject rootObj in selectedObjects)
        {
            foreach (MeshFilter mf in rootObj.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.sharedMesh == null) { skipped++; continue; }
                if (IsExcluded(mf.transform)) { excluded++; continue; }

                GameObject go = mf.gameObject;
                MeshCollider[] existing = go.GetComponents<MeshCollider>();

                if (existing.Length == 0)
                {
                    MeshCollider mc = Undo.AddComponent<MeshCollider>(go);
                    mc.sharedMesh = mf.sharedMesh;
                    added++;
                }
                else
                {
                    // Eerste collider hergebruiken, mesh bijwerken als die leeg/anders is
                    if (existing[0].sharedMesh != mf.sharedMesh)
                    {
                        Undo.RecordObject(existing[0], "Fix collider mesh");
                        existing[0].sharedMesh = mf.sharedMesh;
                        fixedMesh++;
                    }
                }
            }
        }

        Debug.Log($"[Collider Tool] Toegevoegd: {added}, mesh gefixt: {fixedMesh}, overgeslagen (geen mesh): {skipped}, uitgesloten: {excluded}.");
    }

    // Uitgesloten = dit object of een parent heeft de NoCollision marker component.
    static bool IsExcluded(Transform t)
    {
        return t.GetComponentInParent<NoCollision>(true) != null;
    }

    // Haalt bestaande colliders weg van uitgesloten objecten (bv. de banners die er al een hebben).
    [MenuItem("Tools/Remove Colliders From Excluded Objects")]
    public static void RemoveCollidersFromExcluded()
    {
        int removed = 0;
        foreach (GameObject rootObj in Selection.gameObjects)
            foreach (MeshCollider mc in rootObj.GetComponentsInChildren<MeshCollider>(true))
                if (IsExcluded(mc.transform))
                {
                    Undo.DestroyObjectImmediate(mc);
                    removed++;
                }
        Debug.Log($"[Collider Tool] {removed} colliders verwijderd van uitgesloten objecten.");
    }
}
