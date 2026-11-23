using System;
using Project.Scripts.Core;
using UnityEngine;

// One side of a doorway: the room you end up in when you leave the doorway on that side.
[Serializable]
public class RoomSide
{
    [Tooltip("Key in the localization files, like room_kitchen. Leave empty to use Room Name below")]
    public string roomNameKey;
    [Tooltip("Used when the key is empty or not in the localization files yet")]
    public string roomName = "Room";
    [Tooltip("Shows ??? until RoomTrigger.DiscoverRoom is called, for rooms found through clues")]
    public bool hidden;

    // what identifies the room, so two doorways into the same room count as the same room
    public string Id => string.IsNullOrEmpty(roomNameKey) ? roomName : roomNameKey;

    public string DisplayName
    {
        get
        {
            if (hidden)
                return "???";
            if (!string.IsNullOrEmpty(roomNameKey) && Localization.Has(roomNameKey))
                return Localization.Get(roomNameKey);
            return roomName;
        }
    }
}

// Invisible box inside a door or arch, with a room on each side.
// When the player walks OUT of the box, the room on that side shows on the Player HUD.
// Stepping into a doorway and back out the way you came shows nothing.
// Front = the side the blue arrow in the Scene view points to.
// Put it on the Ignore Raycast layer (the prefab already is), so it doesn't block door/inspect raycasts.
[RequireComponent(typeof(BoxCollider))]
public class RoomTrigger : MonoBehaviour
{
    [Tooltip("The room on the side the blue arrow points to")]
    public RoomSide front = new RoomSide();
    [Tooltip("The room on the other side")]
    public RoomSide back = new RoomSide();

    [Header("Editor")]
    public Color gizmoColor = new Color(0.2f, 0.8f, 1f, 0.15f);

    void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
    }

    void OnTriggerExit(Collider other)
    {
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player == null)
            return;

        RoomNameDisplay display = player.GetComponentInChildren<RoomNameDisplay>(true);
        if (display != null)
            display.EnterRoom(SideOf(player.transform.position));
    }

    // which room is on the side of this point
    public RoomSide SideOf(Vector3 worldPosition)
    {
        BoxCollider box = GetComponent<BoxCollider>();
        float z = transform.InverseTransformPoint(worldPosition).z - box.center.z;
        return z >= 0f ? front : back;
    }

    // call this when the player finds a hidden room (clue, secret door, ...).
    // Use the room's key, or its name when it has no key. Unhides it on every doorway.
    public static void DiscoverRoom(string roomId)
    {
        foreach (RoomTrigger trigger in FindObjectsByType<RoomTrigger>(FindObjectsSortMode.None))
        {
            if (trigger.front.Id == roomId) trigger.front.hidden = false;
            if (trigger.back.Id == roomId) trigger.back.hidden = false;
        }

        RoomNameDisplay display = FindFirstObjectByType<RoomNameDisplay>();
        if (display != null && display.CurrentRoom != null && display.CurrentRoom.Id == roomId)
            display.Show(display.CurrentRoom);
    }

    void OnDrawGizmos()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null)
            return;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(box.center, box.size);
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
        Gizmos.DrawWireCube(box.center, box.size);

        // arrow towards the front room
        Vector3 start = box.center;
        Vector3 end = box.center + Vector3.forward * (box.size.z * 0.5f + 0.4f);
        Gizmos.DrawLine(start, end);
        Gizmos.DrawLine(end, end + new Vector3(0.12f, 0f, -0.15f));
        Gizmos.DrawLine(end, end + new Vector3(-0.12f, 0f, -0.15f));
        // the clickable room names are drawn by Editor/RoomTriggerLabels.cs
    }
}
