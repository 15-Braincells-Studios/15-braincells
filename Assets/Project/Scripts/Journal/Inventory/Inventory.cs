using UnityEngine;

public class Inventory : MonoBehaviour
{
    public InventorySlot[] slots;

    public bool AddItem(InventoryItem item)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.IsEmpty())
            {
                slot.SetItem(item);
                return true;
            }
        }

        Debug.Log("Inventory is full!");
        return false;
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