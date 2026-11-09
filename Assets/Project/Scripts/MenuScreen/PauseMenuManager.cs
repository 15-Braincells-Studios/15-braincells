using System;
using UnityEngine;
using TMPro;
using Project.Scripts.Core;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    private InputAction pauseMenuesc;
    [SerializeField] private GameObject resumeButton;
    [Header("Pause Menu Panels")]
    [SerializeField] private GameObject  pauseMenuPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject quitConfirmationPanel;
    
    // [Header("Game Settings")]
    // [SerializeField] private string gameSceneName = "GameScene";

    private string selectedLanguage;
    
    private void Start()
    {
        var uiMap = InputSystem.actions.FindActionMap("UI");
        uiMap.Enable();
        ShowPauseMenu();

        Localization.OnLanguageChanged += UpdateAllText;
        pauseMenuesc = uiMap.FindAction("Escape");
        UpdateAllText();
    }
    private void OnDestroy()
    {
        Localization.OnLanguageChanged -= UpdateAllText;
        
    }

    public void Update()
    {
        if (pauseMenuesc.WasPressedThisFrame())
        {
            ShowPauseMenu();
            TogglePause();
        }
    }

    public void ShowPauseMenu()
    {
        pauseMenuPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        quitConfirmationPanel.SetActive(false);
    }

    public void UpdateAllText()
    {
        
    }

    public bool TogglePause()
    {
        if(Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
            return(false);
        }
        else
        {
            Time.timeScale = 0f;
            return(true);	
        }
    }

}