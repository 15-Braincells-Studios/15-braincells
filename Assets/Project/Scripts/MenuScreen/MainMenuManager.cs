using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Project.Scripts.Core;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject playPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject languageSelectPanel;
    [SerializeField] private GameObject quitConfirmationPanel;
    
    [Header("Main Menu Text")]
    [SerializeField] private TMP_Text playButtonText;
    [SerializeField] private TMP_Text settingsButtonText;
    [SerializeField] private TMP_Text creditsButtonText;
    [SerializeField] private TMP_Text quitButtonText;

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

        Localization.OnLanguageChanged += UpdateAllText;

        UpdateAllText();
    }
        
    private void OnDestroy()
    {
        Localization.OnLanguageChanged -= UpdateAllText;
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
        investigationDayText.text = string.Format(
            Localization.Get("play_investigation"),
            currentDay,
            totalDays
        );

        playDayButtonText.text = string.Format(
            Localization.Get("play_day"),
            currentDay
        );

        for (int i = 0; i < dayProgressDots.Length; i++)
        {
            dayProgressDots[i].color = i < currentDay
                ? completedDayColor
                : remainingDayColor;
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
        mainMenuPanel.SetActive(false);
        languageSelectPanel.SetActive(true);

        selectedLanguage = Localization.CurrentLanguage;

        UpdateAllText();
    }
    private void UpdateAllText()
    {
        // Language selection
        languageQuestionText.text = Localization.Get("language_question");
        yesButtonText.text = Localization.Get("menu_yes");
        noButtonText.text = Localization.Get("menu_no");

        // Main menu
        playButtonText.text = Localization.Get("menu_play");
        settingsButtonText.text = Localization.Get("menu_settings");
        creditsButtonText.text = Localization.Get("menu_credits");
        quitButtonText.text = Localization.Get("menu_quit");

        // Play menu
        UpdatePlayPanel();
    }
    
    public void ConfirmLanguage()
    {
        Localization.Load(selectedLanguage);

        Debug.Log("Selected language: " + selectedLanguage);

        ShowMainMenu();
    }

    public void RejectLanguage()
    {
        if (selectedLanguage == "English")
            selectedLanguage = "Dutch";
        else
            selectedLanguage = "English";

        Localization.Load(selectedLanguage);
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
