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
        ShowPauseMenu();

        Localization.OnLanguageChanged += UpdateAllText;
        pauseMenuesc = InputSystem.actions.FindAction("PauseMenu");
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
        }
    }

    public void ShowPauseMenu()
    {
        pauseMenuPanel.SetActive(true);
        howToPlayPanel.SetActive(false);
        quitConfirmationPanel.SetActive(false);
    }

    public void UpdateAllText()
    {
        
    }



}