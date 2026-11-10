using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Project.Scripts.Core;

public class DoorInteractor : MonoBehaviour
{
    [Header("Raycast")]
    public Camera fpsCam;
    public float range = 3f;

    [Header("Player")]
    public PlayerMovement playerMovement;
    public Inventory inventory;

    [Header("UI")]
    public TMP_Text promptText;
    public TMP_Text messageText;
    public float messageTime = 2f;

    private InputAction interactAction;
    private float messageHideTime;
    private bool blockedLastFrame;

    void Start()
    {
        promptText.text = "";
        messageText.text = "";

        interactAction = InputSystem.actions.FindAction("Player/Interact");

        if (inventory == null)
            Debug.LogWarning("Geen Inventory gekoppeld aan DoorInteractor!");
    }

    void Update()
    {
        // hide the message after a while
        if (messageText.text != "" && Time.time >= messageHideTime)
            messageText.text = "";

        // inspecting or journal open: no doors
        bool blocked = !playerMovement.enabled;
        bool skipInput = blocked || blockedLastFrame;
        blockedLastFrame = blocked;

        if (blocked)
        {
            promptText.text = "";
            return;
        }

        Door door = FindLookedAtDoor();

        if (door == null)
        {
            promptText.text = "";
            return;
        }

        string key = door.IsOpen ? "door_close" : "door_open";
        promptText.text = Localization.Get(key).Replace("{0}", InputPrompts.GetDisplayName("Player/Interact"));

        if (skipInput || !interactAction.WasPressedThisFrame())
            return;

        UseDoor(door);
    }

    Door FindLookedAtDoor()
    {
        if (fpsCam == null)
            return null;

        if (!Physics.Raycast(
                fpsCam.transform.position,
                fpsCam.transform.forward,
                out RaycastHit hit,
                range))
            return null;

        return hit.collider.GetComponentInParent<Door>();
    }

    void UseDoor(Door door)
    {
        if (door.IsLocked)
        {
            string keyName = door.requiredKey.itemName;

            if (inventory == null || !inventory.HasItem(keyName))
            {
                ShowMessage(Localization.Get("door_locked").Replace("{0}", keyName));
                return;
            }

            // the key is used up
            inventory.RemoveItem(keyName);
            door.Unlock();
            ShowMessage(Localization.Get("door_unlocked").Replace("{0}", keyName));
        }

        door.Toggle(transform.position);
    }

    void ShowMessage(string message)
    {
        messageText.text = message;
        messageHideTime = Time.time + messageTime;
    }
}