using System.Collections.Generic;
using FifteenBraincells.MurderGenerator.Data;
using FifteenBraincells.MurderGenerator.Output;

// Working types used while a level is being built. None of them leave the generator.
namespace FifteenBraincells.MurderGenerator.Generation
{
    /// <summary>The characters a line is about, used to fill in its speaker and room.</summary>
    internal sealed class LineContext
    {
        public Character Suspect;
        public Character Victim;
        public Character Subject;
        public Character Holder;
        public Character Partner;

        public Character Owner(RoomOwner owner)
        {
            switch (owner)
            {
                case RoomOwner.Suspect: return Suspect;
                case RoomOwner.Victim: return Victim;
                case RoomOwner.Subject: return Subject;
                case RoomOwner.Holder: return Holder;
                default: return null;
            }
        }
    }

    /// <summary>A speaker that is still to be chosen: an unnamed witness or explainer.</summary>
    internal sealed class SpeakerSlot
    {
        public SpeakerSlot(bool isWitness, IEnumerable<Character> forbidden)
        {
            IsWitness = isWitness;
            Forbidden = new HashSet<Character>(forbidden);
        }

        /// <summary>Witnesses saw something, so someone blocked for Sight or Afternoon cannot be one.</summary>
        public bool IsWitness { get; }

        /// <summary>Characters who may not say this line, e.g. the person it is about.</summary>
        public HashSet<Character> Forbidden { get; }

        /// <summary>The other speaker of the same red herring. The two must be different people.</summary>
        public SpeakerSlot Pair { get; set; }

        public Character Assigned { get; set; }
    }

    /// <summary>A clue whose speaker may still be an open <see cref="SpeakerSlot"/>.</summary>
    internal sealed class DraftClue
    {
        public ClueCategory Category;
        public string SourceId;
        public LineSpec Line;
        public Character FixedSpeaker;
        public SpeakerSlot Slot;
        public PlacedRoom Room;
        public Dictionary<string, TextArg> Args;
        public ClueEffect Effect;

        public Character Speaker => FixedSpeaker ?? Slot?.Assigned;
    }

    /// <summary>An alibi given to a character, fake or real.</summary>
    internal sealed class AlibiPlan
    {
        public AlibiPlan(Character holder, Alibi alibi, AlibiVariant variant, Character partner, bool isFake)
        {
            Holder = holder;
            Alibi = alibi;
            Variant = variant;
            Partner = partner;
            IsFake = isFake;
        }

        public Character Holder { get; }
        public Alibi Alibi { get; }
        public AlibiVariant Variant { get; }
        public Character Partner { get; }
        public bool IsFake { get; }
    }
}
