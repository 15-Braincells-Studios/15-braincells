using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookmarkConversationEntryUI : MonoBehaviour
{
    public TMP_Text personNameText;
    public Button button;
    public Button bookmarkButton;

    private string personID;
    private BookmarkConversationListUI bookmarkListUI;
    private BookmarkManager bookmarkManager;

    public void Setup(string id, string displayName, BookmarkConversationListUI listUI)
    {
        personID = id;
        bookmarkListUI = listUI;

        bookmarkManager = FindFirstObjectByType<BookmarkManager>();

        personNameText.text = displayName;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            bookmarkListUI.OpenConversation(personID);
        });

        bookmarkButton.onClick.RemoveAllListeners();
        bookmarkButton.onClick.AddListener(() =>
        {
            bookmarkManager.ToggleConversationBookmark(personID);
            bookmarkListUI.RefreshBookmarkList();
        });

        UpdateBookmarkColor();
    }

    private void UpdateBookmarkColor()
    {
        bool isBookmarked =
            bookmarkManager.bookmarkedConversations.Contains(personID);

        Image buttonImage = bookmarkButton.GetComponent<Image>();
        buttonImage.color = isBookmarked ? Color.yellow : Color.black;
    }
}