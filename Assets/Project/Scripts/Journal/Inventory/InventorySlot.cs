using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image itemIcon;
    public JournalUI journalUI;

    private InventoryItem currentItem;

    public bool IsEmpty()
    {
        return currentItem == null;
    }

    public void SetItem(InventoryItem item)
    {
        currentItem = item;

        itemIcon.sprite = item.itemImage;
        itemIcon.enabled = true;
        itemIcon.preserveAspect = true;
    }
    
    public InventoryItem GetItem()
    {
        return currentItem;
    }

    public void ClearSlot()
    {
        currentItem = null;

        itemIcon.sprite = null;
        itemIcon.enabled = false;
    }

    public void SelectItem()
    {
        if (currentItem != null)
        {
            journalUI.ShowItem(currentItem);
        }
    }
}