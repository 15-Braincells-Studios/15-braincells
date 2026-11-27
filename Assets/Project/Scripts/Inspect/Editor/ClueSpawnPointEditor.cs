using System.Linq;
using FifteenBraincells.MurderGenerator.Data.Dto;
using UnityEditor;
using UnityEngine;

// Room and owner as dropdowns, read from generator_data.json, so a typo can't make clues disappear.
[CustomEditor(typeof(ClueSpawnPoint))]
[CanEditMultipleObjects]
public class ClueSpawnPointEditor : Editor
{
    private const string Bedroom = "bedroom";

    private static string[] rooms;
    private static string[] owners;

    private SerializedProperty roomId;
    private SerializedProperty ownerId;
    private SerializedProperty forms;

    void OnEnable()
    {
        roomId = serializedObject.FindProperty("roomId");
        ownerId = serializedObject.FindProperty("ownerId");
        forms = serializedObject.FindProperty("forms");
        LoadIds();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (rooms == null)
        {
            EditorGUILayout.HelpBox("generator_data.json not found, type the ids by hand.", MessageType.Warning);
            EditorGUILayout.PropertyField(roomId);
            EditorGUILayout.PropertyField(ownerId);
        }
        else
        {
            Popup(roomId, rooms, "Room");
            if (roomId.hasMultipleDifferentValues || roomId.stringValue == Bedroom)
                Popup(ownerId, owners, "Owner (whose bedroom)");
            else
                ownerId.stringValue = "";
        }

        EditorGUILayout.PropertyField(forms, new GUIContent("Can hold"));
        serializedObject.ApplyModifiedProperties();
    }

    static void Popup(SerializedProperty property, string[] options, string label)
    {
        EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
        int index = System.Array.IndexOf(options, property.stringValue);

        // keep an unknown value visible instead of silently replacing it
        string[] shown = index >= 0 ? options : options.Prepend($"(unknown: {property.stringValue})").ToArray();
        if (index < 0)
            index = 0;

        EditorGUI.BeginChangeCheck();
        int picked = EditorGUILayout.Popup(label, index, shown);
        if (EditorGUI.EndChangeCheck())
            property.stringValue = shown == options || picked > 0 ? shown[picked] : property.stringValue;

        EditorGUI.showMixedValue = false;
    }

    static void LoadIds()
    {
        if (rooms != null)
            return;

        string guid = AssetDatabase.FindAssets("generator_data t:TextAsset").FirstOrDefault();
        if (guid == null)
            return;

        var data = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(guid));
        var dto = JsonUtility.FromJson<GeneratorDataDto>(data.text);

        // forest_road is outside: nothing ever lies there
        rooms = dto.rooms.Select(r => r.id).Where(id => id != "forest_road").OrderBy(id => id).ToArray();
        owners = dto.characters.Where(c => c.tags.Contains("has_bedroom")).Select(c => c.id).ToArray();
    }
}
