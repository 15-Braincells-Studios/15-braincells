using System.Collections.Generic;
using FifteenBraincells.MurderGenerator.Data;

namespace FifteenBraincells.MurderGenerator.Output
{
    /// <summary>Which table a clue came from.</summary>
    public enum ClueCategory
    {
        Method,
        Motive,
        Alibi,
        Contradiction,
        RedHerring,
    }

    /// <summary>
    /// One clue in a level: a line a character says, or something lying in a room.
    /// The text is not finished yet: <see cref="Text.ClueTextRenderer"/> turns it into a string for a language.
    /// </summary>
    public sealed class Clue
    {
        internal Clue(ClueCategory category, string sourceId, ClueForm form, Character speaker, PlacedRoom room,
            string textKey, IReadOnlyDictionary<string, TextArg> args, ClueEffect effect)
        {
            Category = category;
            SourceId = sourceId;
            Form = form;
            Speaker = speaker;
            Room = room;
            TextKey = textKey;
            Args = args;
            Effect = effect;
        }

        public ClueCategory Category { get; }

        /// <summary>Id of the row in generator_data.json this clue was made from.</summary>
        public string SourceId { get; }

        public ClueForm Form { get; }

        /// <summary>Who says it. Null for objects.</summary>
        public Character Speaker { get; }

        /// <summary>Where it lies. Null for spoken lines.</summary>
        public PlacedRoom Room { get; }

        public string TextKey { get; }

        public IReadOnlyDictionary<string, TextArg> Args { get; }

        /// <summary>What the clue proves. Used by the solver; the player never sees this.</summary>
        public ClueEffect Effect { get; }

        public bool IsSpoken => Speaker != null;

        public override string ToString() => $"{Category} {SourceId}";
    }
}
