using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject playPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject languageSelectPanel;
    [SerializeField] private GameObject quitConfirmationPanel;

    [Header("Language Settings")]
    [SerializeField] private TMP_Text languageQuestionText;
    [SerializeField] private TMP_Text yesButtonText;
    [SerializeField] private TMP_Text noButtonText;

    [Header("Play Settings")]
    [SerializeField] private TMP_Text investigationDayText;
    [SerializeField] private TMP_Text playDayButtonText;

    [SerializeField] private Image[] dayProgressDots;

    [SerializeField] private int currentDay = 5;
    [SerializeField] private int totalDays = 7;

    [SerializeField] private Color completedDayColor =
        new Color(0.85f, 0.35f, 0.10f);

    [SerializeField] private Color remainingDayColor =
        Color.gray;

   // [Header("Game Settings")]
   // [SerializeField] private string gameSceneName = "GameScene";

    private string selectedLanguage;

    private void Start()
    {
        ShowMainMenu();
        UpdatePlayPanel();
    }

    // MAIN MENU

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        playPanel.SetActive(false);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        languageSelectPanel.SetActive(false);
        quitConfirmationPanel.SetActive(false);
    }

    // PLAY MENU

    public void ShowPlayPanel()
    {
        mainMenuPanel.SetActive(false);
        playPanel.SetActive(true);

        UpdatePlayPanel();
    }

    private void UpdatePlayPanel()
    {
        // Update investigation text
        investigationDayText.text =
            "Current Investigation\nDay " + currentDay + "/" + totalDays;

        // Update play button
        playDayButtonText.text = "Play Day " + currentDay;

        // Update progress dots
        for (int i = 0; i < dayProgressDots.Length; i++)
        {
            if (i < currentDay)
            {
                dayProgressDots[i].color = completedDayColor;
            }
            else
            {
                dayProgressDots[i].color = remainingDayColor;
            }
        }
    }

    public void PlayGame()
    {
        PlayerPrefs.SetInt("currentDay", currentDay);
        PlayerPrefs.Save();

        Debug.Log("Starting investigation Day " + currentDay);
     //   SceneManager.LoadScene(gameSceneName);
    }

    // SETTINGS MENU

    public void ShowSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // CREDITS MENU

    public void ShowCredits()
    {
        mainMenuPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    // LANGUAGE SELECTION

    public void ShowLanguageSelect()
    {
        settingsPanel.SetActive(false);
        languageSelectPanel.SetActive(true);

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
        PlayerPrefs.SetString("language", selectedLanguage);
        PlayerPrefs.Save();

        Debug.Log("Selected language: " + selectedLanguage);

        ShowSettings();
    }

    public void RejectLanguage()
    {
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

    // QUIT MENU

    public void ShowQuitConfirmation()
    {
        mainMenuPanel.SetActive(false);
        quitConfirmationPanel.SetActive(true);
    }

    public void ConfirmQuit()
    {
        Application.Quit();
    }

    public void CancelQuit()
    {
        ShowMainMenu();
    }

    // BACK BUTTONS

    public void BackToMainMenu()
    {
        ShowMainMenu();
    }

    public void BackToSettings()
    {
        settingsPanel.SetActive(true);
        languageSelectPanel.SetActive(false);
    }
}
