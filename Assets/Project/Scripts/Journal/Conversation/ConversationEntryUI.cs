using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConversationEntryUI : MonoBehaviour
{
    public TMP_Text personNameText;
    public Button button;
    public Button bookmarkButton;

    private string personID;
    private ConversationListUI conversationListUI;
    private BookmarkManager bookmarkManager;

    public void Setup(string id, string displayName, ConversationListUI listUI)
    {
        personID = id;
        personNameText.text = displayName;
        conversationListUI = listUI;

        bookmarkManager = FindFirstObjectByType<BookmarkManager>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            conversationListUI.OpenConversation(personID);
        });

        UpdateBookmarkColor();

        bookmarkButton.onClick.RemoveAllListeners();
        bookmarkButton.onClick.AddListener(() =>
        {
            bookmarkManager.ToggleConversationBookmark(personID);
            UpdateBookmarkColor();
        });
    }

    private void UpdateBookmarkColor()
    {
        bool isBookmarked =
            bookmarkManager.bookmarkedConversations.Contains(personID);

        Image buttonImage = bookmarkButton.GetComponent<Image>();
        buttonImage.color = isBookmarked ? Color.yellow : Color.black;
    }
}