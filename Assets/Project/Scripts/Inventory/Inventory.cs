using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [System.Serializable]
    public class ItemEntry
    {
        public GameObject pickupObject;
        public ItemSO itemSO;
    }

    public List<Slot> slots = new List<Slot>();
    public List<ItemEntry> itemEntries = new List<ItemEntry>();

    public ItemSO GetItem(GameObject pickupObject)
    {
        foreach (ItemEntry entry in itemEntries)
        {
            if (entry.pickupObject == pickupObject)
                return entry.itemSO;
        }

        return null;
    }

    public bool AddItem(ItemSO item)
    {
        if (item == null)
            return false;

        foreach (Slot slot in slots)
        {
            if (slot.Item == null)
            {
                slot.SetItem(item);
                return true;
            }
        }

        return false;
    }
}