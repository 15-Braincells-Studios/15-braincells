using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SuspectStatusButton : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button button;

    private readonly string[] statuses = { "?", "X", "+" };
    private int currentStatus = 0;

    private void Awake()
    {
        button.onClick.AddListener(ChangeStatus);
        UpdateStatus();
    }

    private void ChangeStatus()
    {
        currentStatus = (currentStatus + 1) % statuses.Length;
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        statusText.text = statuses[currentStatus];
    }
}