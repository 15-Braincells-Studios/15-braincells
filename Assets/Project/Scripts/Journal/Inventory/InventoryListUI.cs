using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryListUI : MonoBehaviour
{
    public GameObject itemEntryPrefab;
    public Transform itemList;

    public Image itemImage;
    public TMP_Text itemName;
    public TMP_Text itemDescription;

    // Creates a new button for an inventory item
    public void AddItemToList(InventoryItem item)
    {
        GameObject newEntry = Instantiate(itemEntryPrefab, itemList);

        InventoryEntryUI entryUI = newEntry.GetComponent<InventoryEntryUI>();
        entryUI.Setup(item, this);
    }

    // Displays the selected item
    public void ShowItem(InventoryItem item)
    {
        itemImage.sprite = item.itemImage;
        itemImage.enabled = item.itemImage != null;

        itemName.text = item.itemName;
        itemDescription.text = item.description;
    }
}