using UnityEngine;

public class AccuseDoor : MonoBehaviour
{
    [Header("UI")]
    public GameObject accuseCanvas;

    [Header("Player control")]
    [Tooltip("Scripts to disable while the popup is open (player movement, camera look, interaction...)")]
    public MonoBehaviour[] disableWhileOpen;
    public bool IsOpen { get; private set; }
    
    // True once the player has accused, so the door can't be used again
    public bool HasAccused { get; private set; }

    public void Interact()
    {
        if (IsOpen || HasAccused) return;
        OpenAccuseScreen();
    }

    public void OpenAccuseScreen()
    {
        IsOpen = true;
        accuseCanvas.SetActive(true);

        SetPlayerControl(false);
        PlayerHud.Hide(this);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Back to the game (Cancel button or Escape via PauseMenuManager)
    public void CloseAccuseScreen()
    {
        if (!IsOpen) return;

        IsOpen = false;
        accuseCanvas.SetActive(false);

        SetPlayerControl(true);
        PlayerHud.Show(this);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // After an accusation hide the canvas, but leave the player frozen.
    // The HUD stays hidden too (no PlayerHud.Show), since the result screen comes next.
    public void FinishAccusation()
    {
        IsOpen = false;
        HasAccused = true;
        accuseCanvas.SetActive(false);
    }

    private void SetPlayerControl(bool enabled)
    {
        foreach (var script in disableWhileOpen)
            if (script != null) script.enabled = enabled;
    }
}