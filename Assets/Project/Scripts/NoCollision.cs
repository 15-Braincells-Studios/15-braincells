using UnityEngine;

/// <summary>
/// Marker: zet dit op een object (of prefab-root) dat GEEN MeshCollider mag krijgen.
/// Geldt ook voor alle children. Wordt gelezen door de collider-tools in Assets/Editor.
/// </summary>
[DisallowMultipleComponent]
[AddComponentMenu("Tools/No Collision")]
public class NoCollision : MonoBehaviour { }
