using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryEntryUI : MonoBehaviour
{
    public TMP_Text itemNameText;
    private InventoryItem currentItem;
    private InventoryListUI inventoryListUI;

    // Sets up the clickable inventory entry
    public void Setup(InventoryItem item, InventoryListUI listUI)
    {
        currentItem = item;
        inventoryListUI = listUI;

        itemNameText.text = item.itemName;

        GetComponent<Button>().onClick.AddListener(SelectItem);
    }

    public void SelectItem()
    {
        inventoryListUI.ShowItem(currentItem);
    }
}