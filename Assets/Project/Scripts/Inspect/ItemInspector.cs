using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ItemInspector : MonoBehaviour
{
    [System.Serializable]
    public class InspectEntry
    {
        public GameObject inspectObject;
        public ItemSO itemSO;
    }

    [Header("Raycast")]
    public Camera fpsCam;
    public float range = 3f;

    [Header("Items")]
    public List<InspectEntry> inspectEntries = new List<InspectEntry>();

    [Header("Inspect Stage")]
    public Camera inspectCamera;
    public Transform inspectPivot;
    public string inspectLayer = "Inspect";

    [Header("Controls")]
    public float rotateSpeed = 0.3f;
    public float zoomStep = 0.1f;
    public float minZoom = 0.4f;
    public float maxZoom = 1.5f;

    [Header("UI")]
    public GameObject promptText;
    public GameObject inspectPanel;
    public TMP_Text itemNameText;
    public TMP_Text foundText;

    [Header("Clues")]
    public RawImage itemView;
    public Texture2D clueCursor;
    public Vector2 clueCursorHotspot = new Vector2(11, 11);
    public float foundTextTime = 2f;

    [Header("Player")]
    public PlayerMovement playerMovement;
    public PickupObject pickup;
    public Inventory inventory;

    private GameObject inspectCopy;
    private float fitDistance;
    private float zoom = 1f;

    private Clue[] originalClues;
    private Clue[] copyClues;
    private Clue hoveredClue;
    private float foundTextHideTime;

    private InputAction interactAction;
    private InputAction cancelAction;
    private InputAction lookAction;
    private InputAction rightClickAction;
    private InputAction scrollAction;
    private InputAction pointAction;
    private InputAction clickAction;

    void Start()
    {
        promptText.SetActive(false);
        inspectPanel.SetActive(false);
        inspectCamera.enabled = false;
        foundText.text = "";

        interactAction = InputSystem.actions.FindAction("Player/Interact");
        cancelAction = InputSystem.actions.FindAction("UI/Cancel");
        lookAction = InputSystem.actions.FindAction("Player/Look");
        rightClickAction = InputSystem.actions.FindAction("UI/RightClick");
        scrollAction = InputSystem.actions.FindAction("UI/ScrollWheel");
        pointAction = InputSystem.actions.FindAction("UI/Point");
        clickAction = InputSystem.actions.FindAction("UI/Click");
    }
    
    void Update()
    {
        if (inspectCopy == null)
            UpdateLooking();
        else
            UpdateInspecting();
    }

    void UpdateLooking()
    {
        GameObject target = FindLookedAtTarget();
        promptText.SetActive(target != null);

        if (target == null || !interactAction.WasPressedThisFrame())
            return;

        ItemSO item = GetItem(target);

        if (item == null)
        {
            Debug.LogWarning("Dit object staat niet in Inspect Entries!");
            return;
        }

        OpenInspect(target, item);
    }

    GameObject FindLookedAtTarget()
    {
        if (fpsCam == null)
            return null;

        if (!Physics.Raycast(
                fpsCam.transform.position,
                fpsCam.transform.forward,
                out RaycastHit hit,
                range))
            return null;

        Transform target = hit.transform;

        while (target != null &&
               !target.CompareTag("inspect"))
        {
            target = target.parent;
        }

        return target != null ? target.gameObject : null;
    }

    ItemSO GetItem(GameObject inspectObject)
    {
        foreach (InspectEntry entry in inspectEntries)
        {
            if (entry.inspectObject == inspectObject)
                return entry.itemSO;
        }

        return null;
    }
    
        void OpenInspect(GameObject target, ItemSO item)
    {
        // copy keeps the rotation the player sees it at
        Quaternion viewRotation = Quaternion.Inverse(fpsCam.transform.rotation) * target.transform.rotation;
        inspectCopy = Instantiate(target, inspectPivot.position, inspectCamera.transform.rotation * viewRotation);

        // same order on both, so copyClues[i] belongs to originalClues[i]
        originalClues = target.GetComponentsInChildren<Clue>(true);
        copyClues = inspectCopy.GetComponentsInChildren<Clue>(true);

        // copy is only for looking, turn off scripts and physics
        // colliders stay on so clues can be clicked
        foreach (MonoBehaviour script in inspectCopy.GetComponentsInChildren<MonoBehaviour>())
            script.enabled = false;

        foreach (Rigidbody body in inspectCopy.GetComponentsInChildren<Rigidbody>())
            body.isKinematic = true;

        int layer = LayerMask.NameToLayer(inspectLayer);

        foreach (Transform child in inspectCopy.GetComponentsInChildren<Transform>(true))
            child.gameObject.layer = layer;

        // put the visual center on the pivot and fit it in view
        Bounds bounds = GetBounds(inspectCopy);
        inspectCopy.transform.position += inspectPivot.position - bounds.center;

        float radius = bounds.extents.magnitude;
        fitDistance = radius / Mathf.Sin(inspectCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        zoom = 1f;
        UpdateCameraDistance();

        itemNameText.text = item.itemName;
        promptText.SetActive(false);
        inspectPanel.SetActive(true);
        inspectCamera.enabled = true;

        playerMovement.enabled = false;
        if (pickup != null)
            pickup.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    Bounds GetBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return new Bounds(obj.transform.position, Vector3.one);

        Bounds bounds = renderers[0].bounds;

        foreach (Renderer r in renderers)
            bounds.Encapsulate(r.bounds);

        return bounds;
    }
    
    void UpdateInspecting()
    {
        if (interactAction.WasPressedThisFrame() ||
            cancelAction.WasPressedThisFrame())
        {
            CloseInspect();
            return;
        }

        // rotate around the pivot while right mouse is held
        if (rightClickAction.IsPressed())
        {
            SetHoveredClue(null);

            Vector2 mouseDelta = lookAction.ReadValue<Vector2>();
            Transform cam = inspectCamera.transform;

            inspectCopy.transform.RotateAround(inspectPivot.position, cam.up, -mouseDelta.x * rotateSpeed);
            inspectCopy.transform.RotateAround(inspectPivot.position, cam.right, mouseDelta.y * rotateSpeed);
        }
        else
        {
            SetHoveredClue(FindHoveredClue());
        }

        // take the clue under the mouse
        if (hoveredClue != null && clickAction.WasPressedThisFrame())
            TakeClue(hoveredClue);

        // hide the found text after a while
        if (foundText.text != "" && Time.time >= foundTextHideTime)
            foundText.text = "";

        // zoom one step per scroll notch
        float scroll = scrollAction.ReadValue<Vector2>().y;

        if (scroll != 0f)
        {
            zoom = Mathf.Clamp(zoom - Mathf.Sign(scroll) * zoomStep, minZoom, maxZoom);
            UpdateCameraDistance();
        }
    }

    void UpdateCameraDistance()
    {
        Transform cam = inspectCamera.transform;
        cam.position = inspectPivot.position - cam.forward * fitDistance * zoom;
    }
    
    Clue FindHoveredClue()
    {
        Vector2 mousePos = pointAction.ReadValue<Vector2>();
        RectTransform rect = itemView.rectTransform;

        // canvas is Screen Space Overlay, so no camera here
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, mousePos, null, out Vector2 localPos);

        if (!rect.rect.Contains(localPos))
            return null;

        // spot on the image = spot in the inspect camera's view
        Vector2 viewportPos = Rect.PointToNormalized(rect.rect, localPos);
        Ray ray = inspectCamera.ViewportPointToRay(viewportPos);

        // the copy was moved this frame, let the colliders catch up
        Physics.SyncTransforms();

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask(inspectLayer)))
            return null;

        return hit.collider.GetComponentInParent<Clue>();
    }

    void SetHoveredClue(Clue clue)
    {
        if (clue == hoveredClue)
            return;

        hoveredClue = clue;

        if (hoveredClue != null && clueCursor != null)
            Cursor.SetCursor(clueCursor, clueCursorHotspot, CursorMode.Auto);
        else
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    void TakeClue(Clue clue)
    {
        if (clue.itemSO == null)
        {
            Debug.LogWarning("Deze clue heeft geen Item SO!");
            return;
        }
        
        // put it in the journal's inventory; if it's full, the clue stays where it is
        if (inventory != null)
        {
            InventoryItem inventoryItem = new InventoryItem
            {
                itemName = clue.itemSO.itemName,
                itemImage = clue.itemSO.icon
            };

            if (!inventory.AddItem(inventoryItem))
                return;
        }

        SetHoveredClue(null);

        // hide it on the real item too, so it stays gone
        int index = System.Array.IndexOf(copyClues, clue);
        originalClues[index].gameObject.SetActive(false);
        Destroy(clue.gameObject);

        foundText.text = "Found: " + clue.itemSO.itemName;
        foundTextHideTime = Time.time + foundTextTime;

        // the journal's clues tab hooks in here later
        Debug.Log("Clue found: " + clue.itemSO.itemName);
    }
    
    void CloseInspect()
    {
        SetHoveredClue(null);
        foundText.text = "";

        Destroy(inspectCopy);
        inspectCopy = null;

        inspectPanel.SetActive(false);
        inspectCamera.enabled = false;

        playerMovement.enabled = true;
        if (pickup != null)
            pickup.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}