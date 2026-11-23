using TMPro;
using UnityEngine;

public class BookmarkConversationListUI : MonoBehaviour
{
    public GameObject conversationEntryPrefab;
    public Transform conversationList;
    public BookmarkManager bookmarkManager;
    public ConversationListUI conversationListUI;
    public GameObject rightPageClue;
    public GameObject rightPageConversation;
    public GameObject conversationsCategoryButton;
    public TMP_Text conversationText;
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
        rightPageConversation.SetActive(true);
        rightPageClue.SetActive(false);
        
        conversationListUI.OpenConversation(personID);

        if (conversationListUI.conversations.ContainsKey(personID))
        {
            conversationText.text = string.Join(
                "\n\n",
                conversationListUI.conversations[personID]
            );
        }
        else
        {
            conversationText.text = "";
        }
    }
}