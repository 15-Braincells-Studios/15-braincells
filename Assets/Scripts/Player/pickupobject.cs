using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PickupObject : MonoBehaviour
{
    public Camera fpsCam;
    public float range = 3f;
    public Inventory inventory;

    [Header("Available Items")]
    public List<ItemSO> availableItems = new List<ItemSO>();

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

        ItemSO foundItem = availableItems.Find(
            item => item != null &&
                    item.itemName == target.name);

        if (foundItem == null)
        {
            Debug.LogWarning("Geen ItemSO gevonden voor: " + target.name);
            return;
        }

        if (inventory.AddItem(foundItem))
        {
            Destroy(target.gameObject);
            Debug.Log(foundItem.itemName + " toegevoegd aan inventory!");
        }
        else
        {
            Debug.Log("Inventory zit vol!");
        }
    }
}