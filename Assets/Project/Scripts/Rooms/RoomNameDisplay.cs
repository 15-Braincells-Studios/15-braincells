using System;
using TMPro;
using UnityEngine;

// Shows the name of the room the player walks into, on the Player HUD.
// Doorway triggers call EnterRoom when you step out of a doorway.
// Fades in, stays a moment, fades out. Walking back into the room you were already in shows nothing.
public class RoomNameDisplay : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text roomNameText;
    public CanvasGroup group;

    [Header("Timing")]
    public float fadeInTime = 0.4f;
    public float holdTime = 2.5f;
    public float fadeOutTime = 0.8f;

    // other scripts (map, journal, hidden rooms) can listen to this
    public event Action<RoomSide> OnRoomChanged;
    public RoomSide CurrentRoom { get; private set; }

    private float shownTime = -999f;

    void Awake()
    {
        if (group != null)
            group.alpha = 0f;
    }

    public void EnterRoom(RoomSide room)
    {
        if (room == null)
            return;

        // same room (e.g. stepped into a doorway and back out): no popup
        bool sameRoom = CurrentRoom != null && CurrentRoom.Id == room.Id;
        CurrentRoom = room;
        if (sameRoom)
            return;

        Show(room);
        OnRoomChanged?.Invoke(room);
    }

    public void Show(RoomSide room)
    {
        if (room == null || roomNameText == null)
            return;

        roomNameText.text = room.DisplayName;

        // continue the fade-in from how visible it is now, so it never flashes
        float currentAlpha = group != null ? group.alpha : 0f;
        shownTime = Time.time - fadeInTime * currentAlpha;
    }

    void Update()
    {
        if (group == null)
            return;

        float t = Time.time - shownTime;
        float alpha;

        if (t < fadeInTime)
            alpha = t / fadeInTime;
        else if (t < fadeInTime + holdTime)
            alpha = 1f;
        else
            alpha = 1f - (t - fadeInTime - holdTime) / fadeOutTime;

        alpha = Mathf.Clamp01(alpha);

        // only touch the UI when something changes, so the canvas doesn't rebuild every frame
        if (!Mathf.Approximately(group.alpha, alpha))
            group.alpha = alpha;
    }
}
