
using UnityEngine;
using UnityEngine.InputSystem;
using Project.Scripts.Core;

public class PickupObject : MonoBehaviour
{
    public Camera fpsCam;
    public float range = 3f;
    public Inventory inventory;
    public ClueManager clueManager;

    private InputAction pickupAction;

    void Update()
    {
        pickupAction = InputSystem.actions.FindAction("Pickup");

        if (pickupAction != null && pickupAction.WasPressedThisFrame())
        {
            PickUp();
        }
    }

    void PickUp()
    {
        if (fpsCam == null || inventory == null)
            return;

        if (!Physics.Raycast(
            fpsCam.transform.position,
            fpsCam.transform.forward,
            out RaycastHit hit,
            range))
            return;

        Transform target = hit.transform;

        while (target != null && !target.CompareTag("pickup"))
        {
            target = target.parent;
        }

        if (target == null)
            return;

        InventoryItemData itemData =
            target.GetComponent<InventoryItemData>();

        if (itemData == null)
            itemData = target.GetComponentInChildren<InventoryItemData>();

        if (itemData == null)
        {
            Debug.LogWarning("No InventoryItemData found!");
            return;
        }

        string itemName = Localization.Get(itemData.itemID);
        string itemDescription = Localization.GetDescription(itemData.itemID);
        bool isClue = Localization.IsClue(itemData.itemID);
        

        InventoryItem item = new InventoryItem
        {
            itemID = itemData.itemID,
            itemName = itemName,
            itemImage = itemData.itemImage,
            description = itemDescription
        };

        if (inventory.AddItem(item))
        {
            if (isClue && clueManager != null)
            {
                ClueData clue = new ClueData
                {
                    clueID = itemData.itemID,
                    clueName = itemName,
                    clueImage = itemData.itemImage,
                    description = itemDescription,
                    clueType = "Item"
                };

                clueManager.AddClue(clue);
            }

            Destroy(target.gameObject);

            Debug.Log("Item added to inventory: " + itemName);
        }
    }
}
