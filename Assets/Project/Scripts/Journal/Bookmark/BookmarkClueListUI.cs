using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BookmarkClueListUI : MonoBehaviour
{
    public GameObject bookmarkCluePrefab;
    public Transform bookmarkClueList;
    public BookmarkManager bookmarkManager;

    public GameObject cluesCategoryButton;

    public Image clueImage;
    public TMP_Text clueName;
    public TMP_Text clueDescription;

    private void Start()
    {
        // The journal is a prefab, so it can't link to the BookmarkManager in the scene. Find it instead.
        if (bookmarkManager == null)
            bookmarkManager = FindFirstObjectByType<BookmarkManager>();

        RefreshBookmarkList();
    }

    public void ToggleCluesCategory()
    {
        bookmarkClueList.gameObject.SetActive(
            !bookmarkClueList.gameObject.activeSelf
        );
    }
    // Creates the list of bookmarked clues
    public void RefreshBookmarkList()
    {
        foreach (Transform child in bookmarkClueList)
        {
            Destroy(child.gameObject);
        }

        // No BookmarkManager in this scene: show no bookmarks instead of crashing
        bool hasBookmarks = bookmarkManager != null && bookmarkManager.bookmarkedClues.Count > 0;

        cluesCategoryButton.SetActive(hasBookmarks);

        if (!hasBookmarks)
        {
            return;
        }

        foreach (ClueData clue in bookmarkManager.bookmarkedClues)
        {
            GameObject entry = Instantiate(bookmarkCluePrefab, bookmarkClueList);

            BookmarkClueEntryUI entryUI = entry.GetComponent<BookmarkClueEntryUI>();
            entryUI.Setup(clue, this);
        }
    }

    // Displays the selected bookmarked clue
    public void ShowClue(ClueData clue)
    {
        clueImage.sprite = clue.clueImage;
        clueImage.enabled = clue.clueImage != null;

        clueName.text = clue.clueName;
        clueDescription.text = clue.description;
    }
}