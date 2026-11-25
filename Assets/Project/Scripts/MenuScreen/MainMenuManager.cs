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
    [SerializeField] private GameObject testingLevelsPanel;
    
    [Header("Main Menu Text")]
    [SerializeField] private TMP_Text playButtonText;
    [SerializeField] private TMP_Text settingsButtonText;
    [SerializeField] private TMP_Text creditsButtonText;
    [SerializeField] private TMP_Text quitButtonText;

    // [Header("Language Settings")]
    // [SerializeField] private TMP_Text languageQuestionText;
    // [SerializeField] private TMP_Text yesButtonText;
    // [SerializeField] private TMP_Text noButtonText;

    [Header("Language Selection")]
    [SerializeField] private TMP_Dropdown languageDropdown;
    
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

   [Header("Game Settings")]
   [SerializeField] private string gameSceneName = "A MapScene";

    [Header("Testing Levels")]
    [Tooltip("Fixed cases from MurderGenerator/Data/FixedCases.")]
    [SerializeField] private TextAsset[] testLevelCases;

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
        testingLevelsPanel.SetActive(false);
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

        // The normal Play button always gives a random case.
        SelectedCase.Random();

        Debug.Log("Starting investigation Day " + currentDay);
        SceneManager.LoadScene(gameSceneName);
        Debug.Log("it should load scene");
    }

    // TESTING LEVELS

    public void showTestingPanel()
    {
        mainMenuPanel.SetActive(false);
        testingLevelsPanel.SetActive(true);
    }
    
    // Hook this up on the level buttons with the int set to 0 for Level 1, 1 for Level 2.
    public void PlayTestLevel(int index)
    {
        if (testLevelCases == null || index < 0 || index >= testLevelCases.Length || testLevelCases[index] == null)
        {
            Debug.LogError($"MainMenuManager: no fixed case assigned for test level {index + 1}.", this);
            return;
        }

        SelectedCase.Fixed(testLevelCases[index]);

        Debug.Log($"Starting test level {index + 1} ({testLevelCases[index].name})");
        SceneManager.LoadScene(gameSceneName);
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

        Debug.Log("Current language: " + selectedLanguage);

        if (selectedLanguage == "Dutch")
        {
            languageDropdown.value = 1;
        }
        else
        {
            languageDropdown.value = 0;
        }

        languageDropdown.RefreshShownValue();

        UpdateAllText();
    } 
    private void UpdateAllText()
    {
        // Language selection
        // languageQuestionText.text = Localization.Get("language_question");
        // yesButtonText.text = Localization.Get("menu_yes");
        // noButtonText.text = Localization.Get("menu_no");

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
        selectedLanguage = languageDropdown.options[languageDropdown.value].text;
        
        Localization.Load(selectedLanguage);

        Debug.Log("Selected language: " + selectedLanguage);

        UpdateAllText();
        ShowMainMenu();
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
