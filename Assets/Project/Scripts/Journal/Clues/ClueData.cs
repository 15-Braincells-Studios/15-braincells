using UnityEngine;

[System.Serializable]
public class ClueData
{
    public string clueName;

    [TextArea(3, 8)]
    public string description;

    public Sprite clueImage;

    // Defines the type of clue
    public string clueType;
}