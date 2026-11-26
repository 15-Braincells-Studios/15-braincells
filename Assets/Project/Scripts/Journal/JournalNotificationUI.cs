using System.Collections;
using TMPro;
using UnityEngine;

public class JournalNotificationUI : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private float displayDuration = 3f;

    public void Show(string message)
    {
        messageText.text = message;
        gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        Destroy(gameObject);
    }
}