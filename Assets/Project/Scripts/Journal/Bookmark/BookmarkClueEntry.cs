using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookmarkClueEntryUI : MonoBehaviour
{
    public TMP_Text clueNameText;
    public Button button;
    public Button bookmarkButton;

    private ClueData clueData;
    private BookmarkClueListUI bookmarkListUI;
    private BookmarkManager bookmarkManager;

    public void Setup(ClueData clue, BookmarkClueListUI listUI)
    {
        clueData = clue;
        bookmarkListUI = listUI;

        bookmarkManager = FindFirstObjectByType<BookmarkManager>();

        clueNameText.text = clue.clueName;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            bookmarkListUI.ShowClue(clueData);
        });

        bookmarkButton.onClick.RemoveAllListeners();
        bookmarkButton.onClick.AddListener(() =>
        {
            bookmarkManager.ToggleClueBookmark(clueData);
        });

        UpdateBookmarkColor();
    }

    private void UpdateBookmarkColor()
    {
        Image buttonImage = bookmarkButton.GetComponent<Image>();

        bool isBookmarked = bookmarkManager.bookmarkedClues.Contains(clueData);

        buttonImage.color = isBookmarked ? Color.yellow : Color.black;
    }
}