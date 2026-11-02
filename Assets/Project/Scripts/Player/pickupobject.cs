using UnityEngine;
using UnityEngine.InputSystem;

public class pickupobject : MonoBehaviour
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

        while (target != null &&
               !target.CompareTag("pickup"))
        {
            target = target.parent;
        }

        if (target == null)
            return;

        ItemSO item = inventory.GetItem(target.gameObject);

        if (item == null)
        {
            Debug.LogWarning("Dit object staat niet in Item Entries!");
            return;
        }

        if (inventory.AddItem(item))
        {
            Destroy(target.gameObject);
            Debug.Log(item.itemName + " toegevoegd aan inventory!");
        }
        else
        {
            Debug.Log("Inventory zit vol!");
        }
    }
}