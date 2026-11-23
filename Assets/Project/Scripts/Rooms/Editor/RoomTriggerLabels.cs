using UnityEditor;
using UnityEngine;

// Draws both room names of every doorway RoomTrigger in the Scene view as buttons,
// each on its own side of the doorway. Click a name to select that trigger.
[InitializeOnLoad]
public static class RoomTriggerLabels
{
    private static GUIStyle style;
    private static GUIStyle selectedStyle;

    static RoomTriggerLabels()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView view)
    {
        if (!view.drawGizmos)
            return;

        if (style == null)
        {
            style = new GUIStyle(EditorStyles.miniButton) { fontSize = 11, fontStyle = FontStyle.Bold };
            selectedStyle = new GUIStyle(style);
            selectedStyle.normal.textColor = new Color(0.3f, 0.85f, 1f);
        }

        Handles.BeginGUI();
        foreach (RoomTrigger room in Object.FindObjectsByType<RoomTrigger>(FindObjectsSortMode.None))
        {
            BoxCollider box = room.GetComponent<BoxCollider>();
            if (box == null)
                continue;

            float offset = box.size.z * 0.5f + 0.5f;
            DrawLabel(view, room, room.TransformBoxPoint(box, offset), room.front, true);
            DrawLabel(view, room, room.TransformBoxPoint(box, -offset), room.back, false);
        }
        Handles.EndGUI();
    }

    private static Vector3 TransformBoxPoint(this RoomTrigger room, BoxCollider box, float z)
    {
        return room.transform.TransformPoint(box.center + new Vector3(0f, 0f, z));
    }

    private static void DrawLabel(SceneView view, RoomTrigger room, Vector3 worldPos, RoomSide side, bool isFront)
    {
        // skip labels behind the camera
        if (view.camera.WorldToViewportPoint(worldPos).z <= 0f)
            return;

        string label = string.IsNullOrEmpty(side.roomNameKey) ? side.roomName : side.roomNameKey;
        if (side.hidden)
            label += " (hidden)";
        if (isFront)
            label = "▶ " + label;

        Vector2 guiPos = HandleUtility.WorldToGUIPoint(worldPos);
        Vector2 size = style.CalcSize(new GUIContent(label)) + new Vector2(8, 2);
        Rect rect = new Rect(guiPos - size * 0.5f, size);

        bool selected = Selection.Contains(room.gameObject);
        if (GUI.Button(rect, label, selected ? selectedStyle : style))
        {
            if (Event.current.shift || EditorGUI.actionKey)
            {
                var list = new System.Collections.Generic.List<Object>(Selection.objects) { room.gameObject };
                Selection.objects = list.ToArray();
            }
            else
            {
                Selection.activeGameObject = room.gameObject;
            }
        }
    }
}
