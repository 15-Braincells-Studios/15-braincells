using System.Collections.Generic;
using FifteenBraincells.MurderGenerator.Data;

namespace FifteenBraincells.MurderGenerator.Output
{
    /// <summary>
    /// A value for a placeholder in a clue text. Values stay semantic (a character, a room, a time)
    /// instead of finished English words, so every language can render them its own way.
    /// </summary>
    public abstract class TextArg
    {
    }

    public sealed class CharacterArg : TextArg
    {
        public CharacterArg(Character character) => Character = character;

        public Character Character { get; }
    }

    /// <summary>A list of characters. When the speaker is on the list, they refer to themselves ("me").</summary>
    public sealed class CharacterListArg : TextArg
    {
        public CharacterListArg(IReadOnlyList<Character> characters) => Characters = characters;

        public IReadOnlyList<Character> Characters { get; }
    }

    public sealed class RoomArg : TextArg
    {
        public RoomArg(PlacedRoom room) => Room = room;

        public PlacedRoom Room { get; }
    }

    public sealed class TimeArg : TextArg
    {
        public TimeArg(ClockTime time) => Time = time;

        public ClockTime Time { get; }
    }

    /// <summary>A piece of text that has its own string key, e.g. where an outside alibi claims to be.</summary>
    public sealed class TextKeyArg : TextArg
    {
        public TextKeyArg(string key) => Key = key;

        public string Key { get; }
    }

    /// <summary>A concrete room in a level. <see cref="Owner"/> is set for bedrooms.</summary>
    public sealed class PlacedRoom
    {
        public PlacedRoom(Room room, Character owner = null)
        {
            Room = room;
            Owner = owner;
        }

        public Room Room { get; }

        public Character Owner { get; }

        public override string ToString() => Owner == null ? Room.Id : $"{Room.Id} ({Owner.Id})";
    }
}
