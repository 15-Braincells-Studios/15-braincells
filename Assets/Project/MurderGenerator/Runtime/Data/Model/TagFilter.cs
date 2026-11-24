using System.Collections.Generic;
using System.Linq;

namespace FifteenBraincells.MurderGenerator.Data
{
    /// <summary>
    /// Selects characters by tag. A character's id is also a tag, so a filter can name characters directly.
    /// An empty filter matches everyone ("Any").
    /// </summary>
    public sealed class TagFilter
    {
        public static readonly TagFilter Everyone = new TagFilter(new string[0], new string[0], new string[0]);

        internal TagFilter(IReadOnlyList<string> anyOf, IReadOnlyList<string> allOf, IReadOnlyList<string> noneOf)
        {
            AnyOf = anyOf;
            AllOf = allOf;
            NoneOf = noneOf;
        }

        /// <summary>At least one of these tags is needed (ignored when empty).</summary>
        public IReadOnlyList<string> AnyOf { get; }

        /// <summary>All of these tags are needed.</summary>
        public IReadOnlyList<string> AllOf { get; }

        /// <summary>None of these tags is allowed.</summary>
        public IReadOnlyList<string> NoneOf { get; }

        public bool IsEmpty => AnyOf.Count == 0 && AllOf.Count == 0 && NoneOf.Count == 0;

        public bool Matches(Character character) =>
            (AnyOf.Count == 0 || AnyOf.Any(character.HasTag))
            && AllOf.All(character.HasTag)
            && !NoneOf.Any(character.HasTag);

        internal IEnumerable<string> AllTags => AnyOf.Concat(AllOf).Concat(NoneOf);
    }
}
