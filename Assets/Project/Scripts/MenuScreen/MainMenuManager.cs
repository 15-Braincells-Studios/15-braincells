using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject highScoresPanel;
    [SerializeField] private GameObject quitConfirmationPanel;

    [Header("Game Settings")]
    [SerializeField] private string gameSceneName = "GameScene";

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        howToPlayPanel.SetActive(false);
        highScoresPanel.SetActive(false);
        quitConfirmationPanel.SetActive(false);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowHowToPlay()
    {
        mainMenuPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
    }

    public void ShowHighScores()
    {
        mainMenuPanel.SetActive(false);
        highScoresPanel.SetActive(true);
    }

    public void ShowQuitConfirmation()
    {
        quitConfirmationPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void ConfirmQuit()
    {
        //This only works when it's an application so not in the editor!!
        Application.Quit();
    }

    public void CancelQuit()
    {
        ShowMainMenu();
    }
}
