using System.Collections.Generic;
using UnityEngine;
using TMPro;
using GenClue = FifteenBraincells.MurderGenerator.Output.Clue;
public class ConversationListUI : MonoBehaviour
{
    public List<string> people = new List<string>();
    public Dictionary<string, List<string>> conversations =
        new Dictionary<string, List<string>>();

    // Every line heard, as a clue, so the accuse screen can offer it as evidence.
    // Not shown on the clue page. Not saved by Unity.
    public List<ClueData> ConversationClues { get; } = new List<ClueData>();

    public TMP_Text conversationText;
    public GameObject conversationEntryPrefab;
    public Transform content;

    public void AddConversation(string personID, string displayName)
    {
        // Prevent duplicate people in the list
        if (people.Contains(personID))
            return;

        people.Add(personID);
        if (!conversations.ContainsKey(personID))
        {
            conversations[personID] = new List<string>();
        }

        GameObject entry = Instantiate(
            conversationEntryPrefab.gameObject,
            content
        );

        entry.GetComponent<ConversationEntryUI>().Setup(
            personID,
            displayName,
            this
        );
        JournalNotificationManager.Instance.ShowNotification(
            "Conversation is added to journal"
        );
        Debug.Log("New conversation added: " + displayName);
    }
    // speakerName and generatorClue are for the evidence picker. generatorClue is null for fixed lines from the JSON.
    public void AddConversationLine(string personID, string dialogueLine, string speakerName = null, GenClue generatorClue = null)
    {
        if (!conversations.ContainsKey(personID))
        {
            conversations[personID] = new List<string>();
        }

        if (conversations[personID].Contains(dialogueLine))
            return;

        conversations[personID].Add(dialogueLine);

        ConversationClues.Add(new ClueData
        {
            clueID = $"{personID}_conversation_{conversations[personID].Count}",
            clueName = string.IsNullOrEmpty(speakerName) ? personID : speakerName,
            description = dialogueLine,
            clueImage = null,
            clueType = "Dialogue",
            generatorClue = generatorClue
        });
    }

    public void OpenConversation(string personID)
    {
        ShowConversation(personID);
    }
    public void ShowConversation(string personID)
    {
        if (!conversations.ContainsKey(personID))
            return;

        conversationText.text = string.Join(
            "\n\n",
            conversations[personID]
        );
    }
}