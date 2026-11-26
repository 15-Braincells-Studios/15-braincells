using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class ItemInspector : MonoBehaviour
{
    [Header("Raycast")]
    public Camera fpsCam;
    public float range = 3f;

    [Header("Inspect Stage")]
    public string inspectLayer = "Inspect";

    // taken from the InspectStage in the scene
    private Camera inspectCamera;
    private Transform inspectPivot;

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
    [Tooltip("Optional: shows the text of a generated note while it is inspected. Hidden for other items.")]
    public TMP_Text noteText;

    [Header("Clues")]
    public RawImage itemView;
    public Texture2D clueCursor;
    public Vector2 clueCursorHotspot = new Vector2(11, 11);
    public float foundTextTime = 2f;

    [Header("Player")]
    public PlayerMovement playerMovement;
    public PickupObject pickup;
    public Inventory inventory;
    [Tooltip("Journal clues. Empty: found in the scene.")]
    public ClueManager clueManager;

    private GameObject inspectCopy;
    private bool canRotate = true;
    private float fitDistance;
    private float zoom = 1f;

    private Clue[] originalClues;
    private Clue[] copyClues;
    private Clue hoveredClue;
    private float foundTextHideTime;

    // Anyone can check it; it's open while there's a copy on the inspect stage
    public bool IsOpen => inspectCopy != null;

    private InputAction interactAction;
    private InputAction lookAction;
    private InputAction leftClickAction;
    private InputAction scrollAction;
    private InputAction pointAction;
    private InputAction clickAction;

    void Start()
    {
        promptText.SetActive(false);
        inspectPanel.SetActive(false);
        foundText.text = "";
        if (noteText != null)
            noteText.gameObject.SetActive(false);

        // the stage is part of the scene, not the player prefab, so look it up
        InspectStage stage = FindFirstObjectByType<InspectStage>();

        if (stage != null)
        {
            inspectCamera = stage.inspectCamera;
            inspectPivot = stage.inspectPivot;
        }

        if (inspectCamera == null || inspectPivot == null)
        {
            Debug.LogWarning("Geen InspectStage in deze scene, inspecteren staat uit. Sleep de InspectStage prefab in de scene.");
            enabled = false;
            return;
        }

        inspectCamera.enabled = false;

        interactAction = InputSystem.actions.FindAction("Player/Interact");
        lookAction = InputSystem.actions.FindAction("Player/Look");
        leftClickAction = InputSystem.actions.FindAction("UI/LeftClick");
        scrollAction = InputSystem.actions.FindAction("UI/ScrollWheel");
        pointAction = InputSystem.actions.FindAction("UI/Point");
        clickAction = InputSystem.actions.FindAction("UI/RightClick");
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
        // Don't open the inspector behind the pause menu
        if (Time.timeScale == 0f)
        {
            promptText.SetActive(false);
            return;
        }

        GameObject target = FindLookedAtTarget();
        promptText.SetActive(target != null);

        if (target == null || !interactAction.WasPressedThisFrame())
            return;

        ItemSO item = GetItem(target);
        GeneratedNote note = target.GetComponent<GeneratedNote>();

        if (item == null && note == null)
        {
            Debug.LogWarning(target.name + " heeft geen Inspectable met een Item SO of een GeneratedNote!");
            return;
        }

        OpenInspect(target, item != null ? item.itemName : note.InspectTitle, note);

        // reading a generated note, letter or object puts it in the journal
        if (note != null)
            AddToJournal(note);
    }

    void AddToJournal(GeneratedNote note)
    {
        ClueData clue = note.ToClueData();
        if (clue == null)
            return;

        if (clueManager == null)
            clueManager = FindFirstObjectByType<ClueManager>();

        if (clueManager == null)
        {
            Debug.LogWarning("ItemInspector: no ClueManager in this scene, the note is not added to the journal.");
            return;
        }

        if (clueManager.clues.Exists(c => c.clueID == clue.clueID))
            return;

        clueManager.AddClue(clue);
        foundText.text = "Found: " + clue.clueName;
        foundTextHideTime = Time.time + foundTextTime;
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
        Inspectable inspectable = inspectObject.GetComponent<Inspectable>();
        return inspectable != null ? inspectable.itemSO : null;
    }
    
    void OpenInspect(GameObject target, string itemName, GeneratedNote note)
    {
        // notes and letters are shown straight on and can't be turned; other things keep the angle the player sees them at
        canRotate = note == null || note.canRotate;
        Quaternion rotation = canRotate
            ? inspectCamera.transform.rotation * (Quaternion.Inverse(fpsCam.transform.rotation) * target.transform.rotation)
            : inspectCamera.transform.rotation * Quaternion.Euler(note.inspectRotation);
        inspectCopy = Instantiate(target, inspectPivot.position, rotation);

        // a generated note is blank in the level: write its text on the copy and in the panel
        if (note != null)
            note.ShowTextOnCopy(inspectCopy.GetComponent<GeneratedNote>());

        if (noteText != null)
        {
            bool hasText = note != null && note.ReadText != "";
            noteText.text = hasText ? note.ReadText : "";
            noteText.gameObject.SetActive(hasText);
        }

        // same order on both, so copyClues[i] belongs to originalClues[i]
        originalClues = target.GetComponentsInChildren<Clue>(true);
        copyClues = inspectCopy.GetComponentsInChildren<Clue>(true);

        // copy is only for looking, turn off scripts and physics
        // colliders stay on so clues can be clicked
        foreach (MonoBehaviour script in inspectCopy.GetComponentsInChildren<MonoBehaviour>())
        {
            // text is a script too; turning it off would blank the note on the stage
            if (script is TMP_Text)
                continue;

            script.enabled = false;
        }

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

        itemNameText.text = itemName;
        promptText.SetActive(false);
        inspectPanel.SetActive(true);
        inspectCamera.enabled = true;

        playerMovement.enabled = false;
        if (pickup != null)
            pickup.enabled = false;

        // Only the crosshair: the inspect panel lives inside the Player HUD too
        PlayerHud.HideCrosshair(this);

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
        // Escape is handled by PauseMenuManager, which calls CloseInspect()
        if (interactAction.WasPressedThisFrame())
        {
            CloseInspect();
            return;
        }

        // rotate around the pivot while left mouse is held (not for notes and letters)
        if (canRotate && leftClickAction.IsPressed())
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
    
    // Called by Interact here, or by PauseMenuManager when Escape is pressed
    public void CloseInspect()
    {
        if (!IsOpen) return;

        SetHoveredClue(null);
        foundText.text = "";

        if (noteText != null)
        {
            noteText.text = "";
            noteText.gameObject.SetActive(false);
        }

        Destroy(inspectCopy);
        inspectCopy = null;

        inspectPanel.SetActive(false);
        inspectCamera.enabled = false;

        playerMovement.enabled = true;
        if (pickup != null)
            pickup.enabled = true;

        PlayerHud.ShowCrosshair(this);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}