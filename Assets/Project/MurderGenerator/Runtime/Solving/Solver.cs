using System.Collections.Generic;
using System.Linq;
using FifteenBraincells.MurderGenerator.Data;
using FifteenBraincells.MurderGenerator.Output;

namespace FifteenBraincells.MurderGenerator.Solving
{
    /// <summary>A murderer and victim that the known clues still allow, joined by a motive.</summary>
    public readonly struct Suspicion
    {
        public Suspicion(Character suspect, Character victim)
        {
            Suspect = suspect;
            Victim = victim;
        }

        public Character Suspect { get; }
        public Character Victim { get; }

        public bool Is(Character suspect, Character victim) => Suspect == suspect && Victim == victim;
    }

    public sealed class SolverResult
    {
        internal SolverResult(IReadOnlyCollection<Character> possibleSuspects, IReadOnlyCollection<Character> possibleVictims,
            IReadOnlyList<Suspicion> remaining)
        {
            PossibleSuspects = possibleSuspects;
            PossibleVictims = possibleVictims;
            Remaining = remaining;
        }

        public IReadOnlyCollection<Character> PossibleSuspects { get; }
        public IReadOnlyCollection<Character> PossibleVictims { get; }

        /// <summary>Every motive whose suspect and victim are both still possible.</summary>
        public IReadOnlyList<Suspicion> Remaining { get; }

        public bool IsSolved => Remaining.Count == 1;
    }

    /// <summary>
    /// Reasons like a player who has found the given clues. Everyone starts as a possible murderer and
    /// victim; lists and alibis rule people out, the contradiction cancels the fake alibi, and the motives
    /// that are left are the possible answers.
    /// </summary>
    public static class Solver
    {
        /// <summary>Solves with the clues the player has found.</summary>
        /// <param name="knownClues">The clues found so far.</param>
        /// <param name="cast">Everyone who could be murderer or victim.</param>
        public static SolverResult Solve(IEnumerable<Clue> knownClues, IEnumerable<Character> cast) =>
            Solve(knownClues.Select(c => c.Effect), cast);

        public static SolverResult Solve(IEnumerable<ClueEffect> knownEffects, IEnumerable<Character> cast)
        {
            var effects = knownEffects.ToList();
            var suspects = new HashSet<Character>(cast);
            var victims = new HashSet<Character>(suspects);

            var brokenFakeAlibis = new HashSet<Character>(effects
                .Where(e => e.Kind == ClueEffectKind.BreaksFakeAlibi)
                .SelectMany(e => e.Characters));

            foreach (var effect in effects)
            {
                switch (effect.Kind)
                {
                    case ClueEffectKind.Alibi:
                        RuleOutCompletely(effect.Characters, suspects, victims);
                        break;
                    case ClueEffectKind.FakeAlibi:
                        RuleOutCompletely(effect.Characters.Where(c => !brokenFakeAlibis.Contains(c)), suspects, victims);
                        break;
                    case ClueEffectKind.OnlyThese:
                        Pool(effect.Role, suspects, victims).IntersectWith(effect.Characters);
                        break;
                    case ClueEffectKind.NotThese:
                        Pool(effect.Role, suspects, victims).ExceptWith(effect.Characters);
                        break;
                }
            }

            var remaining = effects
                .Where(e => e.Kind == ClueEffectKind.Motive)
                .Select(e => new Suspicion(e.MotiveSuspect, e.MotiveVictim))
                .Where(s => s.Suspect != s.Victim && suspects.Contains(s.Suspect) && victims.Contains(s.Victim))
                .Distinct()
                .ToList();

            return new SolverResult(suspects, victims, remaining);
        }

        private static void RuleOutCompletely(IEnumerable<Character> characters, HashSet<Character> suspects, HashSet<Character> victims)
        {
            foreach (var character in characters)
            {
                suspects.Remove(character);
                victims.Remove(character);
            }
        }

        private static HashSet<Character> Pool(CrimeRole role, HashSet<Character> suspects, HashSet<Character> victims) =>
            role == CrimeRole.Suspect ? suspects : victims;
    }
}
