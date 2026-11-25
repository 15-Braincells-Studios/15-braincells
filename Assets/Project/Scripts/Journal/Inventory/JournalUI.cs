using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class JournalUI : MonoBehaviour
{
    public GameObject journal;
    public MonoBehaviour playerMovement;

    public Image selectedItemImage;
    public TMP_Text itemName;
    public TMP_Text itemDescription;

    private InputAction openInventoryAction;
    public bool IsOpen { get; private set; }
    public GameObject userFeedback;
    private bool userFeedbackWasActive;

    void Start()
    {
        openInventoryAction = InputSystem.actions.FindAction("Openinventory");

        journal.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (playerMovement != null)
            playerMovement.enabled = true;
    }

    void Update()
    {
        // Don't open the journal on top of the pause menu
        if (Time.timeScale == 0f) return;

        openInventoryAction = InputSystem.actions.FindAction("OpenJournal");
        if (openInventoryAction.WasPressedThisFrame())
            SetOpen(!IsOpen);
    }

    // Called by PauseMenuManager when Escape is pressed
    public void Close()
    {
        if (IsOpen) SetOpen(false);
    }

    private void SetOpen(bool open)
    {
        IsOpen = open;

        journal.SetActive(open);

        if (userFeedback != null)
        {
            if (open)
            {
                userFeedbackWasActive = userFeedback.activeSelf;
                userFeedback.SetActive(false);
            }
            else
            {
                userFeedback.SetActive(userFeedbackWasActive);
            }
        }

        Cursor.visible = open;
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;

        if (playerMovement != null)
            playerMovement.enabled = !open;

        // No crosshair or prompts while the journal is open
        if (open) PlayerHud.Hide(this);
        else PlayerHud.Show(this);
    }

    public void ShowItem(InventoryItem item)
    {
        selectedItemImage.sprite = item.itemImage;
        selectedItemImage.enabled = true;

        itemName.text = item.itemName;
        itemDescription.text = item.description;
    }
}