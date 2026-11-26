using UnityEngine;

public class JournalNotificationManager : MonoBehaviour
{
    public static JournalNotificationManager Instance { get; private set; }

    [SerializeField] private Transform notificationContainer;
    [SerializeField] private JournalNotificationUI notificationPrefab;
    
    private void Awake()
    {
        Instance = this;
    }

    public void ShowNotification(string message)
    {
        JournalNotificationUI notification = Instantiate(
            notificationPrefab,
            notificationContainer
        );

        notification.Show(message);
    }
}