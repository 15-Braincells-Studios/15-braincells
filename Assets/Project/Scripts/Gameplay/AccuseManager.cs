using UnityEngine;
using Project.Scripts.Core;

public class AccuseManager : MonoBehaviour
{
    public ClueManager clueManager;
    public GameObject victoryScreen;
    public GameObject gameoverScreen;
    public Timer timer;

    public void Accuse()
    {
        bool allCluesCollected = true;
        foreach (string id in Localization.GetAllClueKeys())
        {
            Debug.Log("Required: " + id);
        }

        foreach (ClueData clue in clueManager.clues)
        {
            Debug.Log("Collected: " + clue.clueID +
                      " | Name: " + clue.clueName +
                      " | Type: " + clue.clueType);
        }
        foreach (string clueID in Localization.GetAllClueKeys())
        {
            bool found = clueManager.clues.Exists(clue =>
                clue.clueID == clueID
            );

            if (!found)
            {
                allCluesCollected = false;
                Debug.Log("Missing clue: " + clueID);
                break;
            }
        }

        if (allCluesCollected)
        {
            victoryScreen.SetActive(true);
            gameoverScreen.SetActive(false);
        }
        else
        {
            victoryScreen.SetActive(false);
            gameoverScreen.SetActive(true);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        timer.StopTimer();
        Time.timeScale = 0f;
    }
}