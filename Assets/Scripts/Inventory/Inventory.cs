using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public List<Slot> slots = new List<Slot>();

    public bool AddItem(ItemSO item)
    {
        if (item == null)
            return false;

        // Zoek een leeg slot
        foreach (Slot slot in slots)
        {
            if (slot.Item == null)
            {
                slot.SetItem(item);
                return true;
            }
        }

        // Geen lege slots beschikbaar
        return false;
    }
}