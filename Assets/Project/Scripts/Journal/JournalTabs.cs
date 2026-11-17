
using UnityEngine;
using UnityEngine.UI;

public class JournalTabs : MonoBehaviour
{
    public GameObject inventory;
    public GameObject clues;
    public GameObject dialogue;
    public GameObject bookmarks;
    public GameObject suspects;

    public Image inventoryTabImage;
    public Image cluesTabImage;
    public Image dialogueTabImage;
    public Image bookmarksTabImage;
    public Image suspectsTabImage;

    public Sprite normalSprite;
    public Sprite activeSprite;

    [Header("Tab Sizes")]
    public float normalWidth = 60;
    public float normalHeight = 60;

    public float activeWidth = 90;
    public float activeHeight = 90;

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

    void UpdateTabSprites(Image selectedTab)
    {
        Image[] tabs =
        {
            inventoryTabImage,
            cluesTabImage,
            dialogueTabImage,
            bookmarksTabImage,
            suspectsTabImage
        };

        foreach (Image tab in tabs)
        {
            bool isSelected = tab == selectedTab;

            tab.sprite = isSelected ? activeSprite : normalSprite;

            RectTransform rect = tab.GetComponent<RectTransform>();
            rect.sizeDelta = isSelected
                ? new Vector2(activeWidth, activeHeight)
                : new Vector2(normalWidth, normalHeight);
        }
    }

    void CloseAllTabs()
    {
        inventory.SetActive(false);
        clues.SetActive(false);
        dialogue.SetActive(false);
        bookmarks.SetActive(false);
        suspects.SetActive(false);
    }
}
