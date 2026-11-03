using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject highScoresPanel;
    [SerializeField] private GameObject languageSelectPanel;
    [SerializeField] private GameObject quitConfirmationPanel;

    [Header("Language Settings")]
    [SerializeField] private TMP_Text languageQuestionText;
    [SerializeField] private TMP_Text yesButtonText;
    [SerializeField] private TMP_Text noButtonText;

    private string selectedLanguage;
    
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
        languageSelectPanel.SetActive(false);
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
    
    public void ShowLanguageSelect()
    {
        mainMenuPanel.SetActive(false);
        languageSelectPanel.SetActive(true);

        // Load previously selected language (English by default)
        selectedLanguage = PlayerPrefs.GetString("language", "en");

        UpdateLanguagePanel();
    }

    private void UpdateLanguagePanel()
    {
        if (selectedLanguage == "en")
        {
            languageQuestionText.text = "Do you understand me?";
            yesButtonText.text = "Yes";
            noButtonText.text = "No";
        }
        else if (selectedLanguage == "nl")
        {
            languageQuestionText.text = "Begrijp je mij?";
            yesButtonText.text = "Ja";
            noButtonText.text = "Nee";
        }
    }

    public void ConfirmLanguage()
    {
        // Save the selected language
        PlayerPrefs.SetString("language", selectedLanguage);
        PlayerPrefs.Save();

        Debug.Log("Selected language: " + selectedLanguage);

        ShowMainMenu();
    }

    public void RejectLanguage()
    {
        // Switch to the other language
        if (selectedLanguage == "en")
        {
            selectedLanguage = "nl";
        }
        else
        {
            selectedLanguage = "en";
        }

        UpdateLanguagePanel();
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
