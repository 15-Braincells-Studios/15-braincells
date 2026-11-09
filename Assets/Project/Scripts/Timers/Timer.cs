using System;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    private float time;
    public static float finalTime;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject gameoverScreen;
    [SerializeField] private float timeLimit = 15f;

    private bool running = true;

    private void Start()
    {
        gameoverScreen.SetActive(false);
    }
    // Update is called once per frame
    private void Update()
    {
        // Put the time in seconds and format it to mm:ss
        if (running)
        {
           time += Time.deltaTime;
           TimeSpan t = TimeSpan.FromSeconds(time);
           timerText.text = t.ToString(@"mm\:ss"); 
        }
        if (time >= timeLimit)
        {
            GameOver();
        }
        
    }
    private void StopTimer()
    {
        // Stop the Timer and set a float you can use in other scenes
        running = false;
        finalTime = time;
    }
    private void GameOver()
    {
        running = false;
        finalTime = time;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        gameoverScreen.SetActive(true);

        Time.timeScale = 0f;
    }
    public void RestartLevel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
