using TMPro;
using UnityEngine;

public class ResultScreen : MonoBehaviour
{
    [Header("Title")]
    public TMP_Text titleText;

    [Header("Point values")]
    public TMP_Text victimValue;
    public TMP_Text suspectValue;
    public TMP_Text caseValue;
    public TMP_Text totalValue;

    [Header("Colours")]
    public Color winColor  = new Color32(0x2E, 0x6B, 0x2F, 0xFF); // dark green
    public Color loseColor = new Color32(0x8B, 0x1E, 0x1E, 0xFF); // dark red

    public void Show(bool won, bool targetCorrect, bool culpritCorrect, int pointsPerAnswer, int score)
    {
        titleText.text  = won ? "Win" : "Lose";
        titleText.color = won ? winColor : loseColor;

        victimValue.text  = (targetCorrect  ? pointsPerAnswer : 0).ToString();
        suspectValue.text = (culpritCorrect ? pointsPerAnswer : 0).ToString();
        caseValue.text    = score.ToString();
        totalValue.text   = score.ToString();

        gameObject.SetActive(true);
    }
}