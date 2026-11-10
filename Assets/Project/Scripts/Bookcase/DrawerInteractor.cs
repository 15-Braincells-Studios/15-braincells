using UnityEngine;
using UnityEngine.InputSystem;

public class DrawerInteractor : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactRange = 3f;
    
    private InputAction interactAction;
    private void Start()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
    }

    private void Update()
    {
        if (!interactAction.WasPressedThisFrame()) return;
        
        Drawer drawer = FindLookedAtDrawer();
        if (drawer != null) drawer.Toggle();
;
    }
    
    Drawer FindLookedAtDrawer()
    {
        if (playerCamera == null)
            return null;

        // shoot a ray from the middle of the screen
        if (!Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, interactRange))
            return null;

        // find the Drawer script on the hit object or one of its parents
        return hit.collider.GetComponentInParent<Drawer>();
    }
}
