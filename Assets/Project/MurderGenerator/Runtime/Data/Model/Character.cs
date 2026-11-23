using System.Collections.Generic;

namespace FifteenBraincells.MurderGenerator.Data
{
    /// <summary>A member of the castle household. One instance per character in the database.</summary>
    public sealed class Character
    {
        private readonly HashSet<string> _tags;

        internal Character(string id, string nameKey, string referenceKey, string role, string sex,
            string homeRoomId, IEnumerable<string> tags)
        {
            Id = id;
            NameKey = nameKey;
            ReferenceKey = referenceKey;
            Role = role;
            Sex = sex;
            HomeRoomId = homeRoomId;
            // A character's own id works as a tag too, so filters can name single characters.
            _tags = new HashSet<string>(tags) { id };
        }

        public string Id { get; }

        /// <summary>String key for the name as shown in the UI, e.g. "Chef".</summary>
        public string NameKey { get; }

        /// <summary>String key for the name inside a sentence, e.g. "the Chef".</summary>
        public string ReferenceKey { get; }

        public string Role { get; }

        public string Sex { get; }

        /// <summary>Where this character's belongings are found. "bedroom" means their own bedroom.</summary>
        public string HomeRoomId { get; }

        public IReadOnlyCollection<string> Tags => _tags;

        public bool HasTag(string tag) => _tags.Contains(tag);

        public override string ToString() => Id;
    }
}
