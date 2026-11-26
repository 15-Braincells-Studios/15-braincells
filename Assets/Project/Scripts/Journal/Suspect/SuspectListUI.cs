using Project.Scripts.Core;
using UnityEngine;
using Project.Scripts.NPC;
using TMPro;

public class SuspectListUI : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject suspectEntryPrefab;
    [SerializeField] private TMP_Text detailNameText;
    [SerializeField] private TMP_Text detailOccupationText;
    [SerializeField] private TMP_Text detailDescriptionText;
    [SerializeField] private TMP_Text detailBioText;
    private void Start()
    {
        Debug.Log("Number of suspects: " + Localization.Suspects.Length);
        CreateSuspectEntries();
        
    }
    private void CreateSuspectEntries()
    {
        Dialog[] dialogs = FindObjectsByType<Dialog>(FindObjectsSortMode.None);

        foreach (SuspectData suspect in Localization.Suspects)
        {
            GameObject entry = Instantiate(suspectEntryPrefab, content);

            SuspectEntryUI entryUI = entry.GetComponent<SuspectEntryUI>();

            Dialog matchingDialog = System.Array.Find(
                dialogs,
                dialog => dialog.CharacterId == suspect.character_id
            );

            if (matchingDialog != null)
            {
                entryUI.Setup(suspect, SelectSuspect);
            }
            else
            {
                Debug.LogWarning("No NPC found for: " + suspect.character_id);
            }
        }
    }
    public void RefreshSuspectEntries()
    {
        foreach (Transform child in content)
        {
            SuspectEntryUI entry = child.GetComponent<SuspectEntryUI>();

            if (entry == null)
                continue;

            SuspectData suspect = System.Array.Find(
                Localization.Suspects,
                s => s.character_id == entry.CharacterId
            );

            if (suspect != null)
                entry.Refresh(suspect);
        }
    }
    private void SelectSuspect(SuspectData suspect)
    {
        detailNameText.text = suspect.has_been_met ? suspect.name : "????";
        detailOccupationText.text = suspect.has_been_met ? suspect.occupation : "";
        detailDescriptionText.text = suspect.has_been_met ? suspect.description : "";
        detailBioText.text = suspect.has_been_met ? suspect.bio : "";
    }
}