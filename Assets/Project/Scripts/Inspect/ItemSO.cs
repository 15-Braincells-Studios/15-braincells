using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "NewItem")]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject itemprefab;
    public GameObject handitemprefab;
}