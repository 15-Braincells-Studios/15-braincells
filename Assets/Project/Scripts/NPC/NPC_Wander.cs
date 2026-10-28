using UnityEngine;

public class NPC_Wander : MonoBehaviour
{
    [Header("Wander Area")]
    public float wanderWidth = 5;
    public float wanderDepth = 5;
    private Vector2 startingPosition;

    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float turnSpeed = 6f;
    public float minWait = 1;
    public float maxWait = 3;

    private Vector3 targetPosition;
    private float waitTimer;
    private float floorY;

    void Start()
    {
        floorY = transform.position.y;
        startingPosition = new Vector2(transform.position.x, transform.position.z);
        PickNewTarget();
    }

    void Update()
    {
        // timer in between walking positions
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        Vector3 direction = targetPosition - transform.position;

        if (direction.magnitude < 0.1f)
        {
            waitTimer = Random.Range(minWait, maxWait);
            PickNewTarget();
            return;
        }

        // rotate playermodel to face walking direction
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turnSpeed * Time.deltaTime);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    void PickNewTarget()
    {
        // randomly choose target X and Z position
        float randomX = Random.Range(-wanderWidth / 2, wanderWidth / 2);
        float randomZ = Random.Range(-wanderDepth / 2, wanderDepth / 2);
        targetPosition = new Vector3(startingPosition.x + randomX, floorY, startingPosition.y + randomZ);
    }

    public void OnDrawGizmosSelected()
    {
        // Color wander area
        Gizmos.color = Color.red;

        Vector2 center = startingPosition;
        if (!Application.isPlaying)
        {
            center = new Vector2(transform.position.x, transform.position.z);
        }

        Gizmos.DrawWireCube(new Vector3(center.x, transform.position.y, center.y), new Vector3(wanderWidth, 0.1f, wanderDepth));
    }
}