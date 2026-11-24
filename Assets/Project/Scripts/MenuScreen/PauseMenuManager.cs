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
	[Header("Menus closed by Escape")]
	[SerializeField] private ItemInspector itemInspector;
	[SerializeField] private JournalUI journalUI;
	[SerializeField] private AccuseDoor accuseDoor;
    [Header("Pause Menu Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject quitConfirmationPanel;

    [Header("Pause Menu Text")]
    [SerializeField] private TMP_Text pauseTitleText;
    [SerializeField] private TMP_Text resumeButtonText;
    [SerializeField] private TMP_Text howToPlayButtonText;
    [SerializeField] private TMP_Text settingsButtonText;
    [SerializeField] private TMP_Text quitButtonText;

    [Header("Quit Confirmation Text")]
    [SerializeField] private TMP_Text quitQuestionText;
    [SerializeField] private TMP_Text yesButtonText;
    [SerializeField] private TMP_Text noButtonText;

    private bool isPaused = false;
    private InputAction _escapeAction;
    public PlayerMovement player;

    private void Start()
    {
        Debug.Log("PauseMenuManager Start is running!");
        // Make sure the game starts unpaused.
        Time.timeScale = 1f;
        isPaused = false;
        
        // using the new input systems interact button esc for keyboard
        _escapeAction = InputSystem.actions.FindAction("Escape");
        _escapeAction.Enable();

        // The menu canvas is a prefab, so it can't keep references to scene objects.
        // If a field is left empty in the Inspector, look the menu up in the scene.
        if (itemInspector == null) itemInspector = FindFirstObjectByType<ItemInspector>();
        if (journalUI == null) journalUI = FindFirstObjectByType<JournalUI>();
        if (accuseDoor == null) accuseDoor = FindFirstObjectByType<AccuseDoor>();

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
    
    private void Update()
    {
	    if (!_escapeAction.WasPressedThisFrame()) return;

	    // The game is over and the result screen is showing: don't pause on top of it
	    if (accuseDoor != null && accuseDoor.HasAccused) return;

	    if (!isPaused)
	    {
		    // Close whatever menu is open first, and only pause if nothing was open
		    if (itemInspector != null && itemInspector.IsOpen)
		    {
			    itemInspector.CloseInspect();
			    return;
		    }

		    if (accuseDoor != null && accuseDoor.IsOpen)
		    {
			    accuseDoor.CloseAccuseScreen(); 
			    return;
		    }

		    if (journalUI != null && journalUI.IsOpen)
		    {
			    journalUI.Close();             
			    return;
		    }
		    PauseGame();
	    }
	    else if (settingsPanel.activeSelf || howToPlayPanel.activeSelf || quitConfirmationPanel.activeSelf)
		    ShowPauseMenu();
	    else
		    ResumeGame();
    }
    
    // PAUSE GAME
    public void PauseGame()
    {
        Debug.Log("PauseGame called!");
        isPaused = true;

        Time.timeScale = 0f;

        // Stop walking and looking around, and show the mouse so the buttons can be clicked
        if (player != null) player.SetCanMove(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        pauseMenuPanel.SetActive(true);
        howToPlayPanel.SetActive(false);
		settingsPanel.SetActive(false);
        quitConfirmationPanel.SetActive(false);
        PlayerHud.Hide(this);
    }

    // RESUME GAME
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        // Give control back and hide the mouse again (also when Resume is clicked instead of Escape)
        if (player != null) player.SetCanMove(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        pauseMenuPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
		settingsPanel.SetActive(false);
        quitConfirmationPanel.SetActive(false);
        PlayerHud.Show(this);
    }

    public void ShowSettings()
    {
        pauseMenuPanel.SetActive(false);
    	settingsPanel.SetActive(true);
    	howToPlayPanel.SetActive(false);
  	 	quitConfirmationPanel.SetActive(false);
        
    }

    // HOW TO PLAY
    public void ShowHowToPlay()
    {
        pauseMenuPanel.SetActive(false);
    	settingsPanel.SetActive(false);
    	howToPlayPanel.SetActive(true);
    	quitConfirmationPanel.SetActive(false);
    }

    // QUIT CONFIRMATION
    public void ShowQuitConfirmation()
    {
        pauseMenuPanel.SetActive(false);
		settingsPanel.SetActive(false);
		howToPlayPanel.SetActive(false);
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

	public void BackFromSettings()
	{
     	settingsPanel.SetActive(false);
    	pauseMenuPanel.SetActive(true);
 	}

	public void BackFromHowToPlay()
	{
    	howToPlayPanel.SetActive(false);
   	 	pauseMenuPanel.SetActive(true);
	}

    // SHOW PAUSE MENU
    public void ShowPauseMenu()
    {
        pauseMenuPanel.SetActive(true);
        howToPlayPanel.SetActive(false);
		settingsPanel.SetActive(false);
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
        
         if (settingsButtonText != null)
                settingsButtonText.text = Localization.Get("pause_settings");

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
