using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConversationEntryUI : MonoBehaviour
{
    public TMP_Text personNameText;
    public Button button;

    private string personID;
    private ConversationListUI conversationListUI;

    public void Setup(string id, string displayName, ConversationListUI listUI)
    {
        personID = id;
        personNameText.text = displayName;
        conversationListUI = listUI;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
            conversationListUI.OpenConversation(personID)
        );
    }
}