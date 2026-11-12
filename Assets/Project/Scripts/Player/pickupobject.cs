using UnityEngine;
using UnityEngine.InputSystem;

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
        
        if (pickupAction.WasPressedThisFrame())
        {
            PickUp();
        }
    }

    void PickUp()
    {
        if (fpsCam == null || inventory == null)
            return;

        // Check if the player is looking at a pickup object
        if (!Physics.Raycast(
            fpsCam.transform.position,
            fpsCam.transform.forward,
            out RaycastHit hit,
            range))
            return;

        Transform target = hit.transform;

        // Find the object with the pickup tag
        while (target != null &&
               !target.CompareTag("pickup"))
        {
            target = target.parent;
        }

        if (target == null)
            return;

        // Get the item's information
        InventoryItemData itemData =
            target.GetComponent<InventoryItemData>();

        if (itemData == null)
            itemData = target.GetComponentInChildren<InventoryItemData>();

        if (itemData == null)
        {
            Debug.LogWarning("No InventoryItemData found!");
            return;
        }

        InventoryItem item = new InventoryItem
        {
            itemName = itemData.itemName,
            itemImage = itemData.itemImage,
            description = itemData.description
        };

        // Add the item to the inventory first
        if (inventory.AddItem(item))
        {
            // Add important items to the Clues page
            if (itemData.isImportantClue && clueManager != null)
            {
                ClueData clue = new ClueData
                {
                    clueName = itemData.itemName,
                    clueImage = itemData.itemImage,
                    description = itemData.description,
                    clueType = "Item"
                };

                clueManager.AddClue(clue);
            }

            // Remove the object from the scene after pickup
            Destroy(target.gameObject);

            Debug.Log("Item added to Journal!");
        }
    }
}