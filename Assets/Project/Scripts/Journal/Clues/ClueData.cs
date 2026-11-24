using UnityEngine;
using GenClue = FifteenBraincells.MurderGenerator.Output.Clue;

[System.Serializable]
public class ClueData
{
    public string clueID;
    public string clueName;

    [TextArea(3, 8)]
    public string description;

    public Sprite clueImage;
    public string clueType;

    // The generator clue this came from, so the accusation can check it as evidence.
    // Null for items and fixed dialogue lines. Not saved by Unity.
    [System.NonSerialized] public GenClue generatorClue;
}
