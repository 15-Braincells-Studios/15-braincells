using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JournalTabs : MonoBehaviour
{
    public GameObject objective;
    public GameObject inventory;
    public GameObject clues;
    public GameObject dialogue;
    public GameObject bookmarks;
    public GameObject suspects;

    public Image objectiveTabImage;
    public Image inventoryTabImage;
    public Image cluesTabImage;
    public Image dialogueTabImage;
    public Image bookmarksTabImage;
    public Image suspectsTabImage;
    
    [Header("Tab Positions")]
    public float normalX = -790;
    public float activeX = -920;

    private Image selectedTab;

    public void OpenObjective()
    {
        CloseAllTabs();
        objective.SetActive(true);
        UpdateTabSprites(objectiveTabImage);
    }

    public void OpenInventory()
    {
        CloseAllTabs();
        inventory.SetActive(true);
        UpdateTabSprites(inventoryTabImage);
    }

    public void OpenClues()
    {
        CloseAllTabs();
        clues.SetActive(true);
        UpdateTabSprites(cluesTabImage);
    }

    public void OpenDialogue()
    {
        CloseAllTabs();
        dialogue.SetActive(true);
        UpdateTabSprites(dialogueTabImage);
    }

    public void OpenBookmarks()
    {
        CloseAllTabs();
        bookmarks.SetActive(true);
        UpdateTabSprites(bookmarksTabImage);
    }

    public void OpenSuspects()
    {
        CloseAllTabs();
        suspects.SetActive(true);
        UpdateTabSprites(suspectsTabImage);
    }

    void UpdateTabSprites(Image selected)
    {
        selectedTab = selected;

        Image[] tabs =
        {
            objectiveTabImage,
            inventoryTabImage,
            cluesTabImage,
            dialogueTabImage,
            bookmarksTabImage,
            suspectsTabImage
        };

        foreach (Image tab in tabs)
        {
            RectTransform rect = tab.GetComponent<RectTransform>();
        }
    }

    void SetTabPosition(Image tab, float x)
    {
        RectTransform rect = tab.GetComponent<RectTransform>();
        Vector2 position = rect.anchoredPosition;
        position.x = x;
        rect.anchoredPosition = position;
    }

    void TabHoverEnter(Image tab)
    {
        SetTabPosition(tab, activeX);
    }

    void TabHoverExit(Image tab)
    {
        SetTabPosition(tab, tab == selectedTab ? activeX : normalX);
    }

    public void ObjectiveHoverEnter(BaseEventData data) => TabHoverEnter(objectiveTabImage);
    public void ObjectiveHoverExit(BaseEventData data) => TabHoverExit(objectiveTabImage);

    public void InventoryHoverEnter(BaseEventData data) => TabHoverEnter(inventoryTabImage);
    public void InventoryHoverExit(BaseEventData data) => TabHoverExit(inventoryTabImage);

    public void CluesHoverEnter(BaseEventData data) => TabHoverEnter(cluesTabImage);
    public void CluesHoverExit(BaseEventData data) => TabHoverExit(cluesTabImage);

    public void DialogueHoverEnter(BaseEventData data) => TabHoverEnter(dialogueTabImage);
    public void DialogueHoverExit(BaseEventData data) => TabHoverExit(dialogueTabImage);

    public void BookmarksHoverEnter(BaseEventData data) => TabHoverEnter(bookmarksTabImage);
    public void BookmarksHoverExit(BaseEventData data) => TabHoverExit(bookmarksTabImage);

    public void SuspectsHoverEnter(BaseEventData data) => TabHoverEnter(suspectsTabImage);
    public void SuspectsHoverExit(BaseEventData data) => TabHoverExit(suspectsTabImage);

    void CloseAllTabs()
    {
        objective.SetActive(false);
        inventory.SetActive(false);
        clues.SetActive(false);
        dialogue.SetActive(false);
        bookmarks.SetActive(false);
        suspects.SetActive(false);
    }
}