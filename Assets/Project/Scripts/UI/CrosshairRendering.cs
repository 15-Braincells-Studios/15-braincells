using UnityEngine;
using UnityEngine.UI;

public class CrosshairFeedback : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactRange = 3f;

    [Header("Ring")]
    [SerializeField] private Image ringImage;
    [SerializeField] private float hiddenRingScale = 0.5f;   // ring starts small...
    [SerializeField] private float shownRingScale = 1f;      // ...and grows to full size
    [SerializeField] private float changeSpeed = 12f;

    private void Start()
    {
        // Start with the ring invisible
        SetRingAlpha(0f);
        ringImage.transform.localScale = Vector3.one * hiddenRingScale;
    }

    private void Update()
    {
        bool canInteract = IsLookingAtInteractable();

        float targetAlpha = canInteract ? 1f : 0f;
        float targetScale = canInteract ? shownRingScale : hiddenRingScale;

        // How far to move towards the target this frame
        float blendAmount = changeSpeed * Time.deltaTime;

        float newAlpha = Mathf.Lerp(ringImage.color.a, targetAlpha, blendAmount);
        SetRingAlpha(newAlpha);

        Transform ring = ringImage.transform;
        ring.localScale = Vector3.Lerp(ring.localScale, Vector3.one * targetScale, blendAmount);
    }

    private void SetRingAlpha(float alpha)
    {
        Color ringColor = ringImage.color;
        ringColor.a = alpha;
        ringImage.color = ringColor;
    }

    private bool IsLookingAtInteractable()
    {
        if (playerCamera == null)
            return false;

        // Shoot a ray from the middle of the screen
        if (!Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward,
                             out RaycastHit hit, interactRange))
            return false;

        bool isDrawer = hit.collider.GetComponentInParent<Drawer>() != null;
        bool isInspectOrPickup = HasInteractTag(hit.transform);

        return isDrawer || isInspectOrPickup;
    }

    private bool HasInteractTag(Transform hitObject)
    {
        // Check the object we hit, then its parents
        Transform current = hitObject;

        while (current != null)
        {
            if (current.CompareTag("inspect") || current.CompareTag("pickup"))
                return true;

            current = current.parent;
        }

        return false;
    }
}