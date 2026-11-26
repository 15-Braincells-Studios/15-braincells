using UnityEngine;
using UnityEngine.UI;

public class SuspectStatusButton : MonoBehaviour
{
    [SerializeField] private Image statusImage;
    [SerializeField] private Button button;

    [Header("Status Sprites")]
    [SerializeField] private Sprite unknownSprite;
    [SerializeField] private Sprite victimSprite;
    [SerializeField] private Sprite perpetratorSprite;

    private Sprite[] statusSprites;
    private int currentStatus = 0;

    private void Awake()
    {
        statusSprites = new Sprite[]
        {
            unknownSprite,
            victimSprite,
            perpetratorSprite
        };

        button.onClick.AddListener(ChangeStatus);
        UpdateStatus();
    }

    private void ChangeStatus()
    {
        currentStatus = (currentStatus + 1) % statusSprites.Length;
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        statusImage.sprite = statusSprites[currentStatus];
    }
}