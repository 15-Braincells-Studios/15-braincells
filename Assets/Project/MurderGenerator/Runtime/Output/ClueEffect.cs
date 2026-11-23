using System.Collections.Generic;
using FifteenBraincells.MurderGenerator.Data;

namespace FifteenBraincells.MurderGenerator.Output
{
    public enum ClueEffectKind
    {
        /// <summary>Rules nobody out: atmosphere, the method, a red herring.</summary>
        None,
        /// <summary>Only <see cref="ClueEffect.Characters"/> can have <see cref="ClueEffect.Role"/>.</summary>
        OnlyThese,
        /// <summary><see cref="ClueEffect.Characters"/> cannot have <see cref="ClueEffect.Role"/>.</summary>
        NotThese,
        /// <summary><see cref="ClueEffect.Characters"/> are elsewhere: neither murderer nor victim.</summary>
        Alibi,
        /// <summary>Looks like an alibi, until the player finds the clue that breaks it.</summary>
        FakeAlibi,
        /// <summary>Shows that the fake alibi is a lie.</summary>
        BreaksFakeAlibi,
        /// <summary><see cref="ClueEffect.MotiveSuspect"/> has a reason to kill <see cref="ClueEffect.MotiveVictim"/>.</summary>
        Motive,
    }

    /// <summary>The logical meaning of a clue, as the solver reads it.</summary>
    public sealed class ClueEffect
    {
        public static readonly ClueEffect Nothing = new ClueEffect(ClueEffectKind.None, CrimeRole.Suspect, new Character[0]);

        private ClueEffect(ClueEffectKind kind, CrimeRole role, IReadOnlyList<Character> characters,
            Character motiveSuspect = null, Character motiveVictim = null)
        {
            Kind = kind;
            Role = role;
            Characters = characters;
            MotiveSuspect = motiveSuspect;
            MotiveVictim = motiveVictim;
        }

        public ClueEffectKind Kind { get; }
        public CrimeRole Role { get; }
        public IReadOnlyList<Character> Characters { get; }
        public Character MotiveSuspect { get; }
        public Character MotiveVictim { get; }

        public static ClueEffect OnlyThese(CrimeRole role, IReadOnlyList<Character> characters) =>
            new ClueEffect(ClueEffectKind.OnlyThese, role, characters);

        public static ClueEffect NotThese(CrimeRole role, IReadOnlyList<Character> characters) =>
            new ClueEffect(ClueEffectKind.NotThese, role, characters);

        public static ClueEffect Alibi(IReadOnlyList<Character> characters) =>
            new ClueEffect(ClueEffectKind.Alibi, CrimeRole.Suspect, characters);

        public static ClueEffect FakeAlibi(Character holder) =>
            new ClueEffect(ClueEffectKind.FakeAlibi, CrimeRole.Suspect, new[] { holder });

        public static ClueEffect BreaksFakeAlibi(Character holder) =>
            new ClueEffect(ClueEffectKind.BreaksFakeAlibi, CrimeRole.Suspect, new[] { holder });

        public static ClueEffect Motive(Character suspect, Character victim) =>
            new ClueEffect(ClueEffectKind.Motive, CrimeRole.Suspect, new[] { suspect, victim }, suspect, victim);
    }
}
