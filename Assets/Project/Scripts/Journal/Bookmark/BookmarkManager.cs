using System.Collections.Generic;
using UnityEngine;

public class BookmarkManager : MonoBehaviour
{
    public List<ClueData> bookmarkedClues = new List<ClueData>();

    public BookmarkClueListUI bookmarkClueListUI;
    public BookmarkConversationListUI bookmarkConversationListUI;
    public List<string> bookmarkedConversations = new List<string>();
    // Adds or removes a clue from bookmarks
    public bool ToggleClueBookmark(ClueData clue)
    {
        // Match on clueID: every clue has its own id (item id, line key, or generated id).
        // clueName is not unique, since two lines from the same character share the speaker's name.
        ClueData existingClue = bookmarkedClues.Find(saved => saved.clueID == clue.clueID);
        if (existingClue != null)
        {
            bookmarkedClues.Remove(existingClue);
            JournalNotificationManager.Instance.ShowNotification(
                "Bookmark is removed"
            );
        }
        else
        {
            bookmarkedClues.Add(clue);
            JournalNotificationManager.Instance.ShowNotification(
                "Bookmark is added"
            );
        }
        if (bookmarkClueListUI != null)
        {
            bookmarkClueListUI.RefreshBookmarkList();
        }
        return existingClue == null;
    }
    public bool ToggleConversationBookmark(string personID)
    {
        if (bookmarkedConversations.Contains(personID))
        {
            bookmarkedConversations.Remove(personID);
            JournalNotificationManager.Instance.ShowNotification(
                "Bookmark is removed"
            );
        }
        else
        {
            bookmarkedConversations.Add(personID);
            JournalNotificationManager.Instance.ShowNotification(
                "Bookmark is added"
            );
        }

        if (bookmarkConversationListUI != null)
        {
            bookmarkConversationListUI.RefreshBookmarkList();
        }

        return bookmarkedConversations.Contains(personID);
    }
}