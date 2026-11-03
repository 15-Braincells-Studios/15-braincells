using UnityEngine;

public class Slot : MonoBehaviour
{
    public Transform previewPoint;

    private GameObject currentItem;

    public bool IsEmpty()
    {
        return currentItem == null;
    }

    public void SetItem(GameObject item)
    {
        currentItem = Instantiate(item, previewPoint);

        currentItem.transform.localPosition = Vector3.zero;
        currentItem.transform.localRotation = Quaternion.identity;
        currentItem.transform.localScale = Vector3.one;

        SetLayer(currentItem, LayerMask.NameToLayer("InventoryPreview"));

        foreach (Collider col in currentItem.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        foreach (Rigidbody rb in currentItem.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Debug.Log(item.name + " added to slot!");
    }

    void SetLayer(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayer(child.gameObject, layer);
        }
    }
}