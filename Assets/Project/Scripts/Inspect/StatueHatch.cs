using UnityEngine;

// A small door on an inspectable prop, like the one in the base of the dragon statue.
// Put this on the hinge: its pivot is the hinge line, the door mesh is a child with a collider.
// The player opens it with right-click in the inspect view (ItemInspector), the same click that takes clues.
public class StatueHatch : MonoBehaviour
{
    [Tooltip("Axis the door turns around, in the hinge's own space.")]
    public Vector3 hingeAxis = Vector3.up;

    [Tooltip("How far it opens, in degrees. Negative turns the other way.")]
    public float openAngle = -110f;

    public float swingSpeed = 240f;

    [Tooltip("Hidden until the door opens, like a key behind it.")]
    public GameObject[] revealOnOpen;

    // Serialized so the copy on the inspect stage starts in the same state as the real statue
    [SerializeField, HideInInspector] private bool isOpen;
    [SerializeField, HideInInspector] private float currentAngle;
    [SerializeField, HideInInspector] private Quaternion closedRotation;
    [SerializeField, HideInInspector] private bool hasClosedRotation;

    public bool IsOpen => isOpen;

    void Awake()
    {
        // the inspect copy already has this from the real statue, so only the real one sets it
        if (!hasClosedRotation)
        {
            closedRotation = transform.localRotation;
            hasClosedRotation = true;
        }
    }

    public void Open()
    {
        if (isOpen)
            return;

        isOpen = true;

        foreach (GameObject obj in revealOnOpen)
            if (obj != null)
                obj.SetActive(true);

        // the inspect copy has its scripts turned off, but it needs Update to swing
        enabled = true;
    }

    void Update()
    {
        if (!isOpen || currentAngle == openAngle)
            return;

        currentAngle = Mathf.MoveTowards(currentAngle, openAngle, swingSpeed * Time.deltaTime);
        transform.localRotation = closedRotation * Quaternion.AngleAxis(currentAngle, hingeAxis);
    }
}
