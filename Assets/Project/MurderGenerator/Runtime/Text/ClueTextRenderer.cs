using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using FifteenBraincells.MurderGenerator.Data;
using FifteenBraincells.MurderGenerator.Output;

namespace FifteenBraincells.MurderGenerator.Text
{
    /// <summary>
    /// Turns a <see cref="Clue"/> into the text the player reads, using one language's string table.
    /// Everything language-specific (names with articles, list separators, "me") comes from the table,
    /// so another language only needs another strings_&lt;language&gt;.json.
    /// </summary>
    public sealed class ClueTextRenderer
    {
        public const string ListSeparatorKey = "grammar_list_separator";
        public const string ListLastSeparatorKey = "grammar_list_last_separator";
        public const string FirstPersonObjectKey = "grammar_first_person_object";
        private const string OwnerPlaceholder = "{owner}";

        private static readonly Regex Placeholder = new Regex(@"\{([a-z_]+)\}", RegexOptions.Compiled);

        // A letter that starts a sentence: at the start, after . ! ? and a space, or at the start of a quote.
        private static readonly Regex SentenceStart = new Regex(@"(^|[.!?]\s+|:\s+""|^"")(\p{Ll})", RegexOptions.Compiled);

        private readonly IStringTable _strings;

        public ClueTextRenderer(IStringTable strings) => _strings = strings ?? throw new ArgumentNullException(nameof(strings));

        /// <summary>The name of a character as shown above their dialogue, e.g. "Chef".</summary>
        public string RenderName(Character character) => _strings.Get(character.NameKey);

        /// <summary>The finished text of a clue.</summary>
        /// <exception cref="FormatException">The text uses a placeholder the clue has no value for.</exception>
        public string Render(Clue clue)
        {
            var template = _strings.Get(clue.TextKey);
            var text = Placeholder.Replace(template, match =>
            {
                var name = match.Groups[1].Value;
                if (!clue.Args.TryGetValue(name, out var arg))
                {
                    throw new FormatException($"Text '{clue.TextKey}' uses {{{name}}}, but clue {clue} has no value for it.");
                }

                return RenderArg(arg, clue.Speaker);
            });
            return SentenceStart.Replace(text, m => m.Groups[1].Value + m.Groups[2].Value.ToUpperInvariant());
        }

        private string RenderArg(TextArg arg, Character speaker)
        {
            switch (arg)
            {
                case CharacterArg character:
                    return Reference(character.Character);
                case CharacterListArg list:
                    return RenderList(list.Characters, speaker);
                case RoomArg room:
                    return RenderRoom(room.Room);
                case TimeArg time:
                    // Spoken: in words ("ten past seven"). On a note or object: in digits ("19:10").
                    return speaker != null ? TimeInWords(time.Time) : time.Time.ToString();
                case TextKeyArg key:
                    return _strings.Get(key.Key);
                default:
                    throw new ArgumentOutOfRangeException(nameof(arg), arg?.GetType().Name, "Unknown text argument.");
            }
        }

        private string RenderList(IReadOnlyList<Character> characters, Character speaker)
        {
            var parts = new List<string>(characters.Count);
            var includesSpeaker = false;
            foreach (var character in characters)
            {
                if (character == speaker)
                {
                    includesSpeaker = true;
                }
                else
                {
                    parts.Add(Reference(character));
                }
            }

            if (includesSpeaker)
            {
                parts.Add(_strings.Get(FirstPersonObjectKey));
            }

            return Join(parts);
        }

        private string Join(IReadOnlyList<string> parts)
        {
            if (parts.Count <= 1)
            {
                return parts.Count == 1 ? parts[0] : "";
            }

            var builder = new StringBuilder();
            for (var i = 0; i < parts.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(_strings.Get(i == parts.Count - 1 ? ListLastSeparatorKey : ListSeparatorKey));
                }

                builder.Append(parts[i]);
            }

            return builder.ToString();
        }

        /// <summary>The name of a room as used in a sentence, e.g. "the library" or "the King's bedroom".</summary>
        public string RenderRoom(PlacedRoom room)
        {
            var name = _strings.Get(room.Room.NameKey);
            return room.Owner == null ? name : name.Replace(OwnerPlaceholder, Reference(room.Owner));
        }

        private string Reference(Character character) => _strings.Get(character.ReferenceKey);

        /// <summary>A time in words from the string table: "time_minutes_10" = "ten past {hour}", "time_hour_7" = "seven".</summary>
        private string TimeInWords(ClockTime time)
        {
            var patternKey = "time_minutes_" + time.MinutesPastHour;
            if (!_strings.Contains(patternKey))
            {
                return time.ToString();
            }

            string Hour(int h) => _strings.Get("time_hour_" + ((h + 11) % 12 + 1));
            return _strings.Get(patternKey)
                .Replace("{next_hour}", Hour(time.Hours + 1))
                .Replace("{hour}", Hour(time.Hours));
        }
    }
}
