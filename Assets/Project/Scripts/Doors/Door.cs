using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Lock")]
    public ItemSO requiredKey;

    [Header("Swing")]
    public Transform hinge;
    public float openAngle = 90f;
    public float swingSpeed = 180f;

    public bool IsLocked { get; private set; }
    public bool IsOpen { get; private set; }

    private Quaternion closedRotation;
    private Vector3 closedOffset;
    private float currentAngle;
    private float targetAngle;

    void Start()
    {
        // no key set means a normal door
        IsLocked = requiredKey != null;

        closedRotation = hinge.localRotation;

        // from the hinge to the middle of the door, while it's closed
        Renderer doorRenderer = hinge.GetComponentInChildren<Renderer>();
        closedOffset = doorRenderer.bounds.center - hinge.position;
        closedOffset.y = 0f;
    }

    void Update()
    {
        if (currentAngle == targetAngle)
            return;

        // turn a bit further every frame until the target is reached
        currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, swingSpeed * Time.deltaTime);
        hinge.localRotation = closedRotation * Quaternion.Euler(0f, currentAngle, 0f);
    }

    public void Unlock()
    {
        IsLocked = false;
    }

    public void Toggle(Vector3 playerPosition)
    {
        if (IsLocked)
            return;

        IsOpen = !IsOpen;

        if (!IsOpen)
        {
            targetAngle = 0f;
            return;
        }

        // still partly open: keep swinging the same way
        if (currentAngle != 0f)
            targetAngle = Mathf.Sign(currentAngle) * openAngle;
        else
            targetAngle = AwayFromPlayer(playerPosition);
    }

    float AwayFromPlayer(Vector3 playerPosition)
    {
        // where the middle of the door ends up for both swing directions
        Vector3 openPlus = hinge.position + Quaternion.AngleAxis(openAngle, hinge.up) * closedOffset;
        Vector3 openMinus = hinge.position + Quaternion.AngleAxis(-openAngle, hinge.up) * closedOffset;

        // pick the one furthest from the player
        if (Vector3.Distance(openPlus, playerPosition) > Vector3.Distance(openMinus, playerPosition))
            return openAngle;

        return -openAngle;
    }
}