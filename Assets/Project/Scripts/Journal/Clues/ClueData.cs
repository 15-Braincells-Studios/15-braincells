using UnityEngine;

[System.Serializable]
public class ClueData
{
    public string clueID;
    public string clueName;

    [TextArea(3, 8)]
    public string description;

    public Sprite clueImage;
    public string clueType;
}