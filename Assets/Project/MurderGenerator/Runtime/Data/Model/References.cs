namespace FifteenBraincells.MurderGenerator.Data
{
    /// <summary>Who speaks a line. <see cref="Character"/> is only set for <see cref="SpeakerKind.Fixed"/>.</summary>
    public sealed class SpeakerSpec
    {
        public static readonly SpeakerSpec Nobody = new SpeakerSpec(SpeakerKind.None, null);

        internal SpeakerSpec(SpeakerKind kind, Character character)
        {
            Kind = kind;
            Character = character;
        }

        public SpeakerKind Kind { get; }

        public Character Character { get; }

        public bool IsSpoken => Kind != SpeakerKind.None;
    }

    /// <summary>
    /// Where an object clue lies. Besides normal room ids there are a few special rooms that are
    /// resolved per level: see the constants.
    /// </summary>
    public sealed class RoomRef
    {
        public const string MurderPlace = "murder_place";
        public const string AlibiPlace = "alibi_place";
        public const string Bedroom = "bedroom";
        public const string Home = "home";

        public static readonly RoomRef Nowhere = new RoomRef("", RoomOwner.None);

        internal RoomRef(string roomId, RoomOwner owner)
        {
            RoomId = roomId;
            Owner = owner;
        }

        public string RoomId { get; }

        public RoomOwner Owner { get; }

        public bool IsSet => RoomId.Length > 0;
    }

    /// <summary>A time of day in minutes after midnight. "24:00" is 1440.</summary>
    public readonly struct ClockTime
    {
        public ClockTime(int minutes) => Minutes = minutes;

        public int Minutes { get; }

        public int Hours => Minutes / 60;

        public int MinutesPastHour => Minutes % 60;

        public static bool TryParse(string text, out ClockTime time)
        {
            time = default;
            var parts = (text ?? "").Split(':');
            if (parts.Length != 2 || !int.TryParse(parts[0], out var h) || !int.TryParse(parts[1], out var m)
                || h < 0 || h > 24 || m < 0 || m > 59 || (h == 24 && m != 0))
            {
                return false;
            }

            time = new ClockTime(h * 60 + m);
            return true;
        }

        public override string ToString() => $"{Hours:00}:{MinutesPastHour:00}";
    }
}
