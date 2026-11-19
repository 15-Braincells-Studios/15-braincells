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
        // Check all important clues in the scene
        InventoryItemData[] importantItems =
            FindObjectsByType<InventoryItemData>(FindObjectsSortMode.None);
        foreach (InventoryItemData item in importantItems)
        {
            if (bool.TryParse(Localization.Get(item.itemID + "_isClue"), out bool isClue) && isClue)
            {
                string itemName = Localization.Get(item.itemID + "_name");

                bool found = clueManager.clues.Exists(clue =>
                    clue.clueName == itemName &&
                    clue.clueType == "Item");

                if (!found)
                {
                    allCluesCollected = false;
                    break;
                }
            }
        }
        // Check the collected clues
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
        // Pause the game and timer
        timer.StopTimer();
        Time.timeScale = 0f;
    }
}