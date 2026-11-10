using UnityEngine;
using UnityEngine.UI;

public class CrosshairRendering : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactRange = 2f;

    [Header("Look")]
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.6f);
    [SerializeField] private Color interactColor = new Color(1f, 0.85f, 0.3f, 1f);
    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float interactScale = 1.8f;
    [SerializeField] private float changeSpeed = 12f;

    private Image crosshairImage;

    private void Start()
    {
        crosshairImage = GetComponent<Image>();
    }

    private void Update()
    {
        bool canInteract = IsLookingAtInteractable();

        Color targetColor = canInteract ? interactColor : normalColor;
        float targetScale = canInteract ? interactScale : normalScale;

        // How far to move towards the target this frame
        float blendAmount = changeSpeed * Time.deltaTime;

        crosshairImage.color = Color.Lerp(crosshairImage.color, targetColor, blendAmount);
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * targetScale, blendAmount);
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
        // Check the object we hit
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