using System;
using UnityEngine;
using TMPro;
public class Timer : MonoBehaviour
{
    private float time;
    public static float finalTime;
    [SerializeField] private TextMeshProUGUI timerText;

    private bool running = true;

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
        
    }

    private void StopTimer()
    {
        // Stop the Timer and set a float you can use in other scenes
        running = false;
        finalTime = time;
    }
}
