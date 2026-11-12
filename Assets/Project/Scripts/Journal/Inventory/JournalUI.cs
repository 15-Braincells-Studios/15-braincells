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
    private bool isOpen = false;

    void Start()
    {
        journal.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (playerMovement != null)
            playerMovement.enabled = true;
    }

    void Update()
    {
        openInventoryAction = InputSystem.actions.FindAction("Openinventory");
        if (openInventoryAction.WasPressedThisFrame())
        {
            ToggleJournal();
        }
    }

    void ToggleJournal()
    {
        isOpen = !isOpen;

        journal.SetActive(isOpen);

        Cursor.visible = isOpen;
        Cursor.lockState = isOpen
            ? CursorLockMode.None
            : CursorLockMode.Locked;

        if (playerMovement != null)
            playerMovement.enabled = !isOpen;
    }

    public void ShowItem(InventoryItem item)
    {
        selectedItemImage.sprite = item.itemImage;
        selectedItemImage.enabled = true;

        itemName.text = item.itemName;
        itemDescription.text = item.description;
    }
}