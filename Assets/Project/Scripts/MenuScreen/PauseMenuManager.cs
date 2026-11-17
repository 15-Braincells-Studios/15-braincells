using System;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Project.Scripts.Core;
using Unity.Cinemachine;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Pause Menu Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject quitConfirmationPanel;

    [Header("Pause Menu Text")]
    [SerializeField] private TMP_Text pauseTitleText;
    [SerializeField] private TMP_Text resumeButtonText;
    [SerializeField] private TMP_Text howToPlayButtonText;
    [SerializeField] private TMP_Text quitButtonText;

    [Header("Quit Confirmation Text")]
    [SerializeField] private TMP_Text quitQuestionText;
    [SerializeField] private TMP_Text yesButtonText;
    [SerializeField] private TMP_Text noButtonText;

    private bool isPaused = false;
    private InputAction _escapeAction;

    private void Start()
    {
        Debug.Log("PauseMenuManager Start is running!");
        // Make sure the game starts unpaused.
        Time.timeScale = 1f;
        isPaused = false;
        
        // using the new input systems interact button esc for keyboard
        _escapeAction = InputSystem.actions.FindAction("Escape");

        // Hide all pause panels when the game starts.
        pauseMenuPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        quitConfirmationPanel.SetActive(false);

        // Listen for language changes.
        Localization.OnLanguageChanged += UpdateAllText;

        UpdateAllText();
    }

    private void OnDestroy()
    {
        Localization.OnLanguageChanged -= UpdateAllText;

        // Prevent the game from remaining paused
        // if this object gets destroyed.
        Time.timeScale = 1f;
    }

    // private void Update()
    // {
    //     // Check whether Escape was pressed.
    //     if (_escapeAction.WasPressedThisFrame())
    //     {
    //         Debug.Log("Escape pressed!");
    //         if (isPaused)
    //         {
    //             ResumeGame();
    //         }
    //         else
    //         {
    //             PauseGame();
    //         }
    //     }
    // }
    private void Update()
    {
        if (Keyboard.current == null)
        {
            Debug.LogWarning("Keyboard.current is NULL!");
            return;
        }

        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            Debug.Log("A keyboard key was pressed!");
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("ESCAPE DETECTED!");
        }
    }

    // PAUSE GAME
    public void PauseGame()
    {
        Debug.Log("PauseGame called!");
        isPaused = true;
        Time.timeScale = 0f;

        pauseMenuPanel.SetActive(true);
        howToPlayPanel.SetActive(false);
        quitConfirmationPanel.SetActive(false);
    }

    // RESUME GAME
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        pauseMenuPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        quitConfirmationPanel.SetActive(false);
    }

    public void ShowSettings()
    {
        pauseMenuPanel.SetActive(false);
        
    }

    // HOW TO PLAY
    public void ShowHowToPlay()
    {
        pauseMenuPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
    }

    // QUIT CONFIRMATION
    public void ShowQuitConfirmation()
    {
        pauseMenuPanel.SetActive(false);
        quitConfirmationPanel.SetActive(true);
    }

    public void CancelQuit()
    {
        ShowPauseMenu();
    }

    public void ConfirmQuit()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    // SHOW PAUSE MENU
    public void ShowPauseMenu()
    {
        pauseMenuPanel.SetActive(true);
        howToPlayPanel.SetActive(false);
        quitConfirmationPanel.SetActive(false);
    }

    // LOCALIZATION
    public void UpdateAllText()
    {
        if (pauseTitleText != null)
            pauseTitleText.text = Localization.Get("pause_title");

        if (resumeButtonText != null)
            resumeButtonText.text = Localization.Get("pause_resume");

        if (howToPlayButtonText != null)
            howToPlayButtonText.text = Localization.Get("pause_how_to_play");

        if (quitButtonText != null)
            quitButtonText.text = Localization.Get("pause_quit");

        if (quitQuestionText != null)
            quitQuestionText.text = Localization.Get("pause_quit_question");

        if (yesButtonText != null)
            yesButtonText.text = Localization.Get("menu_yes");

        if (noButtonText != null)
            noButtonText.text = Localization.Get("menu_no");
    }
}
