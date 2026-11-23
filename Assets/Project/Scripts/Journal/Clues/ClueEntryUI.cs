using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClueEntryUI : MonoBehaviour
{
    public TMP_Text clueNameText;
    public Button button;

    public Button bookmarkButton;

    private ClueData clueData;
    private ClueListUI clueListUI;
    private BookmarkManager bookmarkManager;

    // Initializes this clue entry
    public void Setup(ClueData clue, ClueListUI listUI)
    {
        clueData = clue;
        clueListUI = listUI;

        clueNameText.text = clue.clueName;

        bookmarkManager = FindFirstObjectByType<BookmarkManager>();

        // Display clue details when clicked
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            clueListUI.ShowClue(clueData);
        });

        // Update bookmark button
        UpdateBookmarkColor();

        bookmarkButton.onClick.RemoveAllListeners();
        bookmarkButton.onClick.AddListener(() =>
        {
            bookmarkManager.ToggleClueBookmark(clueData);
            UpdateBookmarkColor();
        });
    }
    private void OnEnable()
    {
        if (clueData != null && bookmarkManager != null)
        {
            UpdateBookmarkColor();
        }
    }

    // Updates the bookmark button color
    private void UpdateBookmarkColor()
    {
        bool isBookmarked = bookmarkManager.bookmarkedClues.Contains(clueData);

        Image buttonImage = bookmarkButton.GetComponent<Image>();
        buttonImage.color = isBookmarked ? Color.yellow : Color.black;
    }
}