using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public Image icon;

    public ItemSO Item { get; private set; }

    public void SetItem(ItemSO item)
    {
        Item = item;

        if (Item != null)
        {
            icon.sprite = Item.icon;
            icon.enabled = true;
        }
        else
        {
            icon.sprite = null;
            icon.enabled = false;
        }
    }

    public void ClearSlot()
    {
        SetItem(null);
    }
}