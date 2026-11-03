using UnityEngine;
using UnityEngine.InputSystem;

public class PickupObject : MonoBehaviour
{
    public Camera fpsCam;
    public float range = 3f;
    public Inventory inventory;

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.fKey.wasPressedThisFrame)
        {
            PickUp();
        }
    }

    void PickUp()
    {
        if (fpsCam == null || inventory == null)
            return;

        if (!Physics.Raycast(
                fpsCam.transform.position,
                fpsCam.transform.forward,
                out RaycastHit hit,
                range))
            return;

        Transform target = hit.transform;

        while (target != null && !target.CompareTag("pickup"))
        {
            target = target.parent;
        }

        if (target == null)
            return;

        if (inventory.AddItem(target.gameObject))
        {
            Destroy(target.gameObject);
            Debug.Log("Item added to inventory!");
        }
        else
        {
            Debug.Log("Inventory is full!");
        }
    }
}