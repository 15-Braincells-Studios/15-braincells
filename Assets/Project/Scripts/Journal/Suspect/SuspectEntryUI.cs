using Project.Scripts.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SuspectEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text occupationText;
    [SerializeField] private Image portraitImage;
    private string characterId;
    public string CharacterId => characterId;
    public void Setup(SuspectData suspect)
    {
        characterId = suspect.character_id;
        Refresh(suspect);
    }
    public void Refresh(SuspectData suspect)
    {
        nameText.text = suspect.has_been_met ? suspect.name : "????";
        occupationText.text = suspect.has_been_met ? suspect.occupation : "";
    }
    
}