using UnityEngine;

public class Drawer : MonoBehaviour
{
    [SerializeField] private Vector3 openOffset = new Vector3(0f, 0f, 0.3f);
    [SerializeField] private float openSpeed = 1f;
    private Vector3 startPosition;
    private bool isOpen;
    
    private void Start()
    {
        // Position of the closed drawer
        startPosition = transform.localPosition;
    }

    public void Toggle()
    {
        // Flip isOpen when opening/closing drawer
        isOpen = !isOpen;
    }
    
    void Update()
    {
        // Set a target location, then move towards that location if true
        Vector3 target = isOpen ? startPosition + openOffset : startPosition;
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, openSpeed * Time.deltaTime);
    }
}
