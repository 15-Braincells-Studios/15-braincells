using System.Collections.Generic;
using UnityEngine;

public class BookmarkManager : MonoBehaviour
{
    public List<ClueData> bookmarkedClues = new List<ClueData>();

    public BookmarkClueListUI bookmarkClueListUI;

    // Adds or removes a clue from bookmarks
    public bool ToggleClueBookmark(ClueData clue)
    {
        ClueData existingClue = bookmarkedClues.Find(
            saved => saved.clueName == clue.clueName &&
                     saved.clueType == clue.clueType
        );

        if (existingClue != null)
        {
            bookmarkedClues.Remove(existingClue);
        }
        else
        {
            bookmarkedClues.Add(clue);
        }

        if (bookmarkClueListUI != null)
        {
            bookmarkClueListUI.RefreshBookmarkList();
        }

        return existingClue == null;
    }
}