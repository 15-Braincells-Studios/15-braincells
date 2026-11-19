using UnityEngine;

public class BookmarkConversationListUI : MonoBehaviour
{
    public GameObject conversationEntryPrefab;
    public Transform conversationList;
    public BookmarkManager bookmarkManager;
    public ConversationListUI conversationListUI;

    public GameObject conversationsCategoryButton;

    private void Start()
    {
        RefreshBookmarkList();
    }

    public void ToggleConversationsCategory()
    {
        conversationList.gameObject.SetActive(
            !conversationList.gameObject.activeSelf
        );
    }

    public void RefreshBookmarkList()
    {
        foreach (Transform child in conversationList)
        {
            Destroy(child.gameObject);
        }

        bool hasBookmarks =
            bookmarkManager.bookmarkedConversations.Count > 0;

        conversationsCategoryButton.SetActive(hasBookmarks);

        if (!hasBookmarks)
            return;

        foreach (string personID in bookmarkManager.bookmarkedConversations)
        {
            GameObject entry = Instantiate(
                conversationEntryPrefab,
                conversationList
            );
            string displayName = personID;
            // Find the person's display name from the conversation list.
            // We will connect this properly in the next step.

            BookmarkConversationEntryUI entryUI =
                entry.GetComponent<BookmarkConversationEntryUI>();

            entryUI.Setup(personID, displayName, this);
        }
    }

    public void OpenConversation(string personID)
    {
        conversationListUI.OpenConversation(personID);
    }
}