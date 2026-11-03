using UnityEngine;


public class PauseMenuManager : MonoBehaviour
{
    [Header("Pause Menu Panels")]
    [SerializeField] private GameObject  pauseMenuPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject quitConfirmationPanel;
    
    [Header("Game Settings")]
    [SerializeField] private string gameSceneName = "GameScene";
    
    
}