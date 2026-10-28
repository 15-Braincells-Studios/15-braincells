using System.Collections.Generic;
using UnityEngine;

public class NPC_Set_Patrol : MonoBehaviour
{
    [Header("Waypoints")]
    public List<Transform> waypoints;
    public bool randomOrder = false;

    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float turnSpeed = 6f;
    public float minWait = 1;
    public float maxWait = 3;

    private List<Vector3> points = new List<Vector3>();
    private Vector3 targetPosition;
    private int currentIndex = -1;
    private float waitTimer;
    private float floorY;

    void Start()
    {
        floorY = transform.position.y;
        foreach (Transform waypoint in waypoints)
        {
            points.Add(waypoint.position);
        }
        PickNewTarget();
    }

    void Update()
    {
        if (points.Count == 0) return;
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

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turnSpeed * Time.deltaTime);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    // if preferred pick random target between waypoints
    void PickNewTarget()
    {
        if (points.Count == 0) return;
        if (randomOrder && points.Count > 1)
        {
            int newIndex = currentIndex;
            while (newIndex == currentIndex)
            {
                newIndex = Random.Range(0, points.Count);
            }
            currentIndex = newIndex;
        }
        else
        {
            currentIndex = (currentIndex + 1) % points.Count;
        }
        Vector3 point = points[currentIndex];
        targetPosition = new Vector3(point.x, floorY, point.z);
    }

    // draw lines between position points using a for loop to iterate how many waypoints there are
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        List<Vector3> positions = new List<Vector3>();

        if (Application.isPlaying)
        {
            positions = points;
        }
        else
        {
            foreach (Transform waypoint in waypoints)
            {
                if (waypoint != null) positions.Add(waypoint.position);
            }
        }
        
        for (int i = 0; i < positions.Count; i++)
        {
            Gizmos.DrawWireSphere(positions[i], 0.3f);

            if (!randomOrder)
            {
                Gizmos.DrawLine(positions[i], positions[(i + 1) % positions.Count]);
            }
        }
    }
}