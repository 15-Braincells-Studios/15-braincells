using UnityEngine;

public class InventoryItemData : MonoBehaviour
{
    public string itemName;

    public Sprite itemImage;

    [TextArea(3, 6)]
    public string description;
}