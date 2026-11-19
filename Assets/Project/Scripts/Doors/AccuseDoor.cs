using UnityEngine;

public class AccuseDoor : MonoBehaviour
{
    [Header("UI")]
    public GameObject accuseCanvas;

    [Header("Player control")]
    [Tooltip("Scripts to disable while the popup is open (player movement, camera look, interaction...)")]
    public MonoBehaviour[] disableWhileOpen;

    private bool isOpen;
    
    public void Interact()
    {
        if (isOpen) return;
        OpenAccuseScreen();
    }

    public void OpenAccuseScreen()
    {
        isOpen = true;
        accuseCanvas.SetActive(true);

        foreach (var script in disableWhileOpen)
            if (script != null) script.enabled = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    // Back to the game (for example a Cancel button): give control back
    public void CloseAccuseScreen()
    {
        isOpen = false;
        accuseCanvas.SetActive(false);

        foreach (var script in disableWhileOpen)
            if (script != null) script.enabled = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    // After an accusation: hide the canvas, but leave the player frozen
    public void FinishAccusation()
    {
        accuseCanvas.SetActive(false);
    }
}