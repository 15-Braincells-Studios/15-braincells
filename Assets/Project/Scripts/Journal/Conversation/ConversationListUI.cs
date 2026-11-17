using System;
using System.Collections.Generic;
using UnityEngine;

public class ConversationListUI : MonoBehaviour
{
    [Serializable]
    public class ConversationPerson
    {
        public string personID;
        public string displayName;
    }

    [Header("UI")]
    public Transform content;
    public GameObject conversationEntryPrefab;

    [Header("People")]
    public List<ConversationPerson> people = new List<ConversationPerson>();

    void Start()
    {
    }

    public void AddConversationToList(string personID, string displayName)
    {
        GameObject entry = Instantiate(conversationEntryPrefab, content);

        ConversationEntryUI entryUI = entry.GetComponent<ConversationEntryUI>();
        entryUI.Setup(personID, displayName, this);
    }

    public void OpenConversation(string personID)
    {
        Debug.Log("Selected conversation: " + personID);
    }
}