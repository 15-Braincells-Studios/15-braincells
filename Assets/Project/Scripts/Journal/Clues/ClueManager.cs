using System.Collections.Generic;
using UnityEngine;

public class ClueManager : MonoBehaviour
{
    public List<ClueData> clues = new List<ClueData>();

    public ClueListUI clueListUI;

    // Adds a newly discovered clue to the journal
    public void AddClue(ClueData clue)
    {
        clues.Add(clue);
        clueListUI.RefreshClueList();

        Debug.Log("New clue discovered: " + clue.clueName);
    }
}