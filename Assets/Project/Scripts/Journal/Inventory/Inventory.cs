using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<InventoryItem> items = new List<InventoryItem>();

    public InventoryListUI inventoryListUI;

    // Adds a new item to the inventory
    public bool AddItem(InventoryItem item)
    {
        items.Add(item);

        if (inventoryListUI != null)
        {
            inventoryListUI.AddItemToList(item);
        }

        Debug.Log("Item added to inventory: " + item.itemName);
        return true;
    }

    // Checks if the inventory contains an item
    public bool HasItem(string itemName)
    {
        return items.Exists(item => item.itemName == itemName);
    }

    // Removes an item from the inventory
    public void RemoveItem(string itemName)
    {
        InventoryItem item = items.Find(
            item => item.itemName == itemName
        );

        if (item != null)
        {
            items.Remove(item);
            Debug.Log("Item removed from inventory: " + itemName);
        }
    }
}