using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public string itemID;
    public string itemName;
    public Sprite itemImage;

    [TextArea(3, 6)]
    public string description;
}