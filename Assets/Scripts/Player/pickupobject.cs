using UnityEngine;
using UnityEngine.InputSystem;

public class PickupObject : MonoBehaviour
{
    public Camera fpsCam;
    public float range = 3f;

    private GameObject heldObject;
    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (heldObject == null)
            {
                PickUp();
            }
            else
            {
                Drop();
            }
        }
    }

    
    void PickUp()
    {
        RaycastHit hit;

        if (Physics.Raycast(
                fpsCam.transform.position,
                fpsCam.transform.forward,
                out hit,
                range))
        {
            Transform target = hit.transform;

            while (target != null && !target.CompareTag("pickup"))
            {
                target = target.parent;
            }

            if (target != null)
            {
                heldObject = target.gameObject;

                heldObject.transform.SetParent(fpsCam.transform);
                heldObject.transform.localPosition =
                    new Vector3(0.3f, -0.3f, 1f);
                heldObject.transform.localRotation = Quaternion.identity;
            }
        }
    }


    void Drop()
    {
        // Haal het object los van de camera
        heldObject.transform.SetParent(null);

        // Zoek de grond recht onder het object
        RaycastHit hit;

        if (Physics.Raycast(
                heldObject.transform.position,
                Vector3.down,
                out hit,
                10f))
        {
            // Zet het object plat op de grond
            heldObject.transform.position =
                hit.point + Vector3.up * 0.05f;

            // Geen schuine rotatie
            heldObject.transform.rotation =
                Quaternion.Euler(0f, heldObject.transform.eulerAngles.y, 0f);
        }

        heldObject = null;
    }
}