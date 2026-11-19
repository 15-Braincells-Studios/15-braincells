using UnityEngine;

// Put this on an object the player can inspect, and pick its ItemSO.
public class Inspectable : MonoBehaviour
{
    public ItemSO itemSO;

    // ItemInspector and the crosshair look for this tag
    void Reset()
    {
        gameObject.tag = "inspect";
    }
}
