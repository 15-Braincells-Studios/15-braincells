using Project.Scripts.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
public class SuspectEntryUI : MonoBehaviour
{
    [SerializeField] private Button entryButton;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text occupationText;
    [SerializeField] private Image portraitImage;
    private string characterId;
    public string CharacterId => characterId;
    
    public void Setup(SuspectData suspect, Action<SuspectData> onSelected)
    {
        characterId = suspect.character_id;
        Refresh(suspect);

        entryButton.onClick.RemoveAllListeners();
        entryButton.onClick.AddListener(() => onSelected?.Invoke(suspect));
    }
    public void Refresh(SuspectData suspect)
    {
        nameText.text = suspect.has_been_met ? suspect.name : "????";
        occupationText.text = suspect.has_been_met ? suspect.occupation : "";
    }
    
}