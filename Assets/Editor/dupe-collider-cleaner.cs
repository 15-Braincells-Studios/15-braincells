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
}
