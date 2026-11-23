using System.Collections.Generic;

// The content model. Instances are created and validated by DatabaseBuilder only; outside the
// assembly everything is read-only.
namespace FifteenBraincells.MurderGenerator.Data
{
    public sealed class Room
    {
        public string Id { get; internal set; }
        public string NameKey { get; internal set; }
    }

    /// <summary>A single line of text plus who says it, or where it lies.</summary>
    public sealed class LineSpec
    {
        public ClueForm Form { get; internal set; }
        public SpeakerSpec Speaker { get; internal set; }
        public RoomRef Room { get; internal set; }
        public string TextKey { get; internal set; }
    }

    public sealed class Method
    {
        public string Id { get; internal set; }
        public string NameKey { get; internal set; }
        public ClockTime Time { get; internal set; }

        /// <summary>One of these rooms is picked per level.</summary>
        public IReadOnlyList<string> RoomIds { get; internal set; }

        public TagFilter Suspects { get; internal set; }
        public TagFilter Victims { get; internal set; }

        public TagFilter PoolFor(CrimeRole role) => role == CrimeRole.Suspect ? Suspects : Victims;
    }

    public sealed class ClueGroupRule
    {
        public ClueGroup Group { get; internal set; }
        public float ShowChance { get; internal set; }
    }

    public sealed class MethodClue
    {
        public string Id { get; internal set; }
        public Method Method { get; internal set; }
        public ClueGroup Group { get; internal set; }
        public LineSpec Line { get; internal set; }
        public string Clarity { get; internal set; }

        public MethodClueEffect Effect { get; internal set; }

        /// <summary>Whether the effect is about the suspect or the victim.</summary>
        public CrimeRole EffectRole { get; internal set; }

        /// <summary>The characters for <see cref="MethodClueEffect.ExcludeFixed"/> and <see cref="MethodClueEffect.OnlyFixed"/>.</summary>
        public IReadOnlyList<Character> EffectCharacters { get; internal set; }

        /// <summary>The clue is skipped when one of these is the suspect.</summary>
        public IReadOnlyList<Character> NotSuspect { get; internal set; }

        /// <summary>The clue is skipped when one of these is the victim.</summary>
        public IReadOnlyList<Character> NotVictim { get; internal set; }

        /// <summary>Who may be named by "(excluded)". Empty: anyone in the method's pool.</summary>
        public TagFilter ExcludedFilter { get; internal set; }

        public bool ExcludedGetNoOtherAlibi { get; internal set; }

        /// <summary>A "presence" list: it says who will be at the murder place, so nobody on it may be elsewhere.</summary>
        public bool ListedGetNoOtherAlibi { get; internal set; }

        /// <summary>The text already mentions the speaker ("Besides myself, only..."), so the list leaves them out.</summary>
        public bool ListOmitsSpeaker { get; internal set; }

        /// <summary>What the characters named by "(excluded)" cannot do for the rest of the day.</summary>
        public Need Blocks { get; internal set; }
    }

    public sealed class Motive
    {
        public string Id { get; internal set; }
        public TagFilter Suspects { get; internal set; }
        public TagFilter Victims { get; internal set; }
        public bool SameRole { get; internal set; }
        public bool DifferentSex { get; internal set; }
        public LineSpec Clue { get; internal set; }

        public bool Fits(Character suspect, Character victim) =>
            suspect != victim
            && Suspects.Matches(suspect)
            && Victims.Matches(victim)
            && (!SameRole || suspect.Role == victim.Role)
            && (!DifferentSex || suspect.Sex != victim.Sex);
    }

    public sealed class AlibiPartner
    {
        public static readonly AlibiPartner None = new AlibiPartner { Kind = PartnerKind.None, Placeholder = "" };

        public PartnerKind Kind { get; internal set; }

        /// <summary>The partner for <see cref="PartnerKind.Fixed"/>.</summary>
        public Character Character { get; internal set; }

        /// <summary>Who may be the partner for <see cref="PartnerKind.Pool"/>.</summary>
        public TagFilter Pool { get; internal set; }

        /// <summary>The text placeholder that holds the partner's name, e.g. "partner" or "tutor".</summary>
        public string Placeholder { get; internal set; }
    }

    public sealed class AlibiVariant
    {
        public LineSpec Line { get; internal set; }

        /// <summary>The holder needs this tag for this variant (e.g. a note on their own bedroom door). Empty: no rule.</summary>
        public string HolderTag { get; internal set; }
    }

    public sealed class Alibi
    {
        public string Id { get; internal set; }
        public string NameKey { get; internal set; }
        public TagFilter Holders { get; internal set; }
        public AlibiPartner Partner { get; internal set; }
        public ClockTime Start { get; internal set; }
        public ClockTime End { get; internal set; }
        public bool IsOutside { get; internal set; }

        /// <summary>The room for an inside alibi.</summary>
        public string RoomId { get; internal set; }

        /// <summary>String key for where an outside alibi claims to be, e.g. "the tavern".</summary>
        public string PlaceKey { get; internal set; }

        public Need Needs { get; internal set; }
        public IReadOnlyList<Method> NotWithMethods { get; internal set; }

        /// <summary>Only usable when someone else really has this alibi (Caring for the sick needs Sick).</summary>
        public Alibi RequiresOtherHolderOf { get; internal set; }

        public IReadOnlyList<AlibiVariant> Variants { get; internal set; }

        /// <summary>Whether this alibi puts its holder somewhere else at the time of the murder.</summary>
        public bool Covers(Method method, string murderRoomId) =>
            Start.Minutes <= method.Time.Minutes
            && method.Time.Minutes < End.Minutes
            && (IsOutside || RoomId != murderRoomId)
            && !Contains(NotWithMethods, method);

        private static bool Contains(IReadOnlyList<Method> methods, Method method)
        {
            for (var i = 0; i < methods.Count; i++)
            {
                if (methods[i] == method)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>The conditions under which a contradiction may be used. All of them must hold.</summary>
    public sealed class ContradictionRules
    {
        public IReadOnlyList<Character> NotSuspect { get; internal set; }
        public bool AllOtherAlibisInside { get; internal set; }
        public bool NoOtherAlibiAtAlibiPlace { get; internal set; }

        /// <summary>Nobody else has an alibi in this room. Null: no rule.</summary>
        public Room NoOtherAlibiInRoom { get; internal set; }

        public bool NoOtherOutsideAlibi { get; internal set; }

        /// <summary>Nobody else has this alibi. Null: no rule.</summary>
        public Alibi NoOtherHolderOf { get; internal set; }

        /// <summary>The suspect needs all of these tags (e.g. a note in their handwriting needs "writes").</summary>
        public IReadOnlyList<string> SuspectHasTags { get; internal set; }

        /// <summary>Only for these fake alibis. Empty: any.</summary>
        public IReadOnlyList<Alibi> OnlyForAlibis { get; internal set; }

        public IReadOnlyList<Alibi> NotForAlibis { get; internal set; }
        public IReadOnlyList<Method> NotWithMethods { get; internal set; }
    }

    public sealed class Contradiction
    {
        public string Id { get; internal set; }

        /// <summary>True: only for <see cref="Alibi"/>. False: general, filtered by <see cref="AlibiTag"/>.</summary>
        public bool IsSpecific { get; internal set; }

        public Alibi Alibi { get; internal set; }

        /// <summary>"Any", "Inside" or "Outside" for general contradictions.</summary>
        public string AlibiTag { get; internal set; }

        public LineSpec Line { get; internal set; }
        public ContradictionRules Rules { get; internal set; }
    }

    public sealed class HerringLine
    {
        public LineSpec Line { get; internal set; }

        /// <summary>Characters who may say the line instead of a random explainer ("Doctor or (Explainer)").</summary>
        public IReadOnlyList<Character> FixedExplainerOptions { get; internal set; }
    }

    public sealed class RedHerring
    {
        public string Id { get; internal set; }

        /// <summary>Who the red herring may be about. Suspect and victim are always excluded.</summary>
        public TagFilter Subjects { get; internal set; }

        public Need Needs { get; internal set; }
        public HerringLine Suspicion { get; internal set; }
        public HerringLine Explanation { get; internal set; }
    }
}
