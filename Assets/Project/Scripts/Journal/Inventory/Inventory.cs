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
    
    public bool HasItem(string itemName)
    {
        foreach (InventorySlot slot in slots)
        {
            if (!slot.IsEmpty() && slot.GetItem().itemName == itemName)
                return true;
        }

        return false;
    }

    public bool RemoveItem(string itemName)
    {
        foreach (InventorySlot slot in slots)
        {
            if (!slot.IsEmpty() && slot.GetItem().itemName == itemName)
            {
                slot.ClearSlot();
                return true;
            }
        }

        return false;
    }
}