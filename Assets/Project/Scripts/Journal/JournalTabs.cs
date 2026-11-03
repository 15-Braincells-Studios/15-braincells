using UnityEngine;

public class JournalTabs : MonoBehaviour
{
    public GameObject inventory;
    public GameObject clues;
    public GameObject dialogue;
    public GameObject bookmarks;
    public GameObject suspects;

    public void OpenInventory()
    {
        CloseAllTabs();
        inventory.SetActive(true);
    }

    public void OpenClues()
    {
        CloseAllTabs();
        clues.SetActive(true);
    }

    public void OpenDialogue()
    {
        CloseAllTabs();
        dialogue.SetActive(true);
    }

    public void OpenBookmarks()
    {
        CloseAllTabs();
        bookmarks.SetActive(true);
    }

    public void OpenSuspects()
    {
        CloseAllTabs();
        suspects.SetActive(true);
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