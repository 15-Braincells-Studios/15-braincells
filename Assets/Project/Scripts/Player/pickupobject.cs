
using UnityEngine;
using UnityEngine.InputSystem;

public class PickupObject : MonoBehaviour
{
    public Camera fpsCam;
    public float range = 3f;
    public Inventory inventory;

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.fKey.wasPressedThisFrame)
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

        while (target != null &&
               !target.CompareTag("pickup"))
        {
            target = target.parent;
        }

        if (target == null)
            return;

        InventoryItemData itemData =
            target.GetComponent<InventoryItemData>();

        if (itemData == null)
        {
            itemData = target.GetComponentInChildren<InventoryItemData>();
        }

        if (itemData == null)
        {
            Debug.LogWarning("No InventoryItemData found!");
            return;
        }

        InventoryItem item = new InventoryItem();

        item.itemName = itemData.itemName;
        item.itemImage = itemData.itemImage;
        item.description = itemData.description;

        if (inventory.AddItem(item))
        {
            Destroy(target.gameObject);
            Debug.Log("Item added to Journal!");
        }
    }
}
