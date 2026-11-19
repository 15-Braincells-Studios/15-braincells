using System.Collections.Generic;
using UnityEngine;

public class ConversationListUI : MonoBehaviour
{
    public List<string> people = new List<string>();
    public Dictionary<string, List<string>> conversations =
        new Dictionary<string, List<string>>();

    public GameObject conversationEntryPrefab;
    public Transform content;

    public void AddConversation(string personID, string displayName)
    {
        // Prevent duplicate people in the list
        if (people.Contains(personID))
            return;

        people.Add(personID);

        GameObject entry = Instantiate(
            conversationEntryPrefab.gameObject,
            content
        );

        entry.GetComponent<ConversationEntryUI>().Setup(
            personID,
            displayName,
            this
        );

        Debug.Log("New conversation added: " + displayName);
    }
    public void AddConversationLine(string personID, string dialogueLine)
    {
        if (!conversations.ContainsKey(personID))
        {
            conversations[personID] = new List<string>();
        }

        conversations[personID].Add(dialogueLine);
    }

    public void OpenConversation(string personID)
    {
        Debug.Log("Selected conversation: " + personID);
    }
}