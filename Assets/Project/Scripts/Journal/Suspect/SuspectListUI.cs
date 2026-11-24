using Project.Scripts.Core;
using UnityEngine;
using Project.Scripts.NPC;
public class SuspectListUI : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject suspectEntryPrefab;

    private void Start()
    {
        Debug.Log("SuspectListUI started!");
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
                Debug.Log("Found NPC: " + matchingDialog.CharacterId);
                entryUI.Setup(suspect);
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
}