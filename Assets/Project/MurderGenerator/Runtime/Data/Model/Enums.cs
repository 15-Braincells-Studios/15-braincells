using System;

namespace FifteenBraincells.MurderGenerator.Data
{
    /// <summary>Which side of the crime a clue or rule is about.</summary>
    public enum CrimeRole
    {
        Suspect,
        Victim,
    }

    /// <summary>
    /// Method clue groups. The Excel called them A to D.
    /// </summary>
    public enum ClueGroup
    {
        /// <summary>A: when and where the murder happens.</summary>
        WhenAndWhere,
        /// <summary>B: which method is used.</summary>
        Method,
        /// <summary>C: who can be the murderer.</summary>
        SuspectPool,
        /// <summary>D: who can be the victim.</summary>
        VictimPool,
    }

    /// <summary>How the player meets a clue: a spoken line, or something lying in a room.</summary>
    public enum ClueForm
    {
        Witness,
        Note,
        Document,
        Item,
        Visual,
    }

    /// <summary>Who speaks a line.</summary>
    public enum SpeakerKind
    {
        /// <summary>Nobody: the clue is an object in a room.</summary>
        None,
        /// <summary>One specific character, named in the data.</summary>
        Fixed,
        /// <summary>Any suitable character, chosen by the generator.</summary>
        Witness,
        /// <summary>Any suitable character who explains a red herring, chosen by the generator.</summary>
        Explainer,
        /// <summary>The holder of the alibi.</summary>
        Self,
        /// <summary>The partner in the alibi.</summary>
        Partner,
    }

    /// <summary>Whose room a <see cref="RoomRef"/> points to when the room depends on a character.</summary>
    public enum RoomOwner
    {
        None,
        Suspect,
        Victim,
        /// <summary>The character a red herring is about.</summary>
        Subject,
        /// <summary>The holder of an alibi.</summary>
        Holder,
    }

    /// <summary>What a method clue tells the player about the suspect or victim.</summary>
    public enum MethodClueEffect
    {
        /// <summary>Gives no information about who.</summary>
        None,
        /// <summary>Names everyone who fits the method ("(list)").</summary>
        FullList,
        /// <summary>Names the real one plus a few others ("(partial list)").</summary>
        PartialList,
        /// <summary>Names one or two innocent characters who are ruled out ("(excluded)").</summary>
        Excluded,
        /// <summary>Rules out the characters given in the data.</summary>
        ExcludeFixed,
        /// <summary>Only the characters given in the data are possible.</summary>
        OnlyFixed,
    }

    /// <summary>How an alibi's second character is chosen.</summary>
    public enum PartnerKind
    {
        None,
        /// <summary>Always the same character ("Chef is busy too").</summary>
        Fixed,
        /// <summary>The other character of a two-person alibi (Fencing Lessons).</summary>
        Joint,
        /// <summary>Anyone matching a tag filter ("Tutor is busy too", "Partner is busy too").</summary>
        Pool,
    }

    /// <summary>
    /// Things a character needs for an alibi or red herring, and that a method clue can block for the day.
    /// </summary>
    [Flags]
    public enum Need
    {
        None = 0,
        Hands = 1 << 0,
        Writing = 1 << 1,
        Lifting = 1 << 2,
        Sight = 1 << 3,
        Wine = 1 << 4,
        Afternoon = 1 << 5,
    }
}
