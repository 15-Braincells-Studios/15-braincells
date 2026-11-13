using System.Collections.Generic;
using System.Linq;
using FifteenBraincells.MurderGenerator.Data;

namespace FifteenBraincells.MurderGenerator.Output
{
    /// <summary>The hidden truth of a level.</summary>
    public sealed class Solution
    {
        internal Solution(Method method, PlacedRoom murderRoom, Character suspect, Character victim, Motive motive,
            Alibi fakeAlibi, Contradiction contradiction)
        {
            Method = method;
            MurderRoom = murderRoom;
            Suspect = suspect;
            Victim = victim;
            Motive = motive;
            FakeAlibi = fakeAlibi;
            Contradiction = contradiction;
        }

        public Method Method { get; }
        public PlacedRoom MurderRoom { get; }
        public Character Suspect { get; }
        public Character Victim { get; }
        public Motive Motive { get; }
        public Alibi FakeAlibi { get; }
        public Contradiction Contradiction { get; }
    }

    /// <summary>Who is where tonight, and with whom. Real and fake alibis.</summary>
    public sealed class AlibiAssignment
    {
        internal AlibiAssignment(Character holder, Alibi alibi, Character partner, bool isFake)
        {
            Holder = holder;
            Alibi = alibi;
            Partner = partner;
            IsFake = isFake;
        }

        public Character Holder { get; }
        public Alibi Alibi { get; }

        /// <summary>The second character, or null.</summary>
        public Character Partner { get; }

        public bool IsFake { get; }
    }

    /// <summary>A generated level: the solution, the clues, and who is where tonight.</summary>
    public sealed class Level
    {
        internal Level(int seed, int attempts, Solution solution, IReadOnlyList<Clue> clues,
            IReadOnlyList<AlibiAssignment> alibis, IReadOnlyDictionary<Character, Need> blocked)
        {
            Seed = seed;
            Attempts = attempts;
            Solution = solution;
            Clues = clues;
            Alibis = alibis;
            Blocked = blocked;
        }

        public int Seed { get; }

        /// <summary>How many tries the generator needed. Useful for tuning, not for gameplay.</summary>
        public int Attempts { get; }

        public Solution Solution { get; }

        public IReadOnlyList<Clue> Clues { get; }

        public IReadOnlyList<AlibiAssignment> Alibis { get; }

        /// <summary>What characters cannot do today because of a method clue. Only characters with blocks are listed.</summary>
        public IReadOnlyDictionary<Character, Need> Blocked { get; }

        /// <summary>The lines a character says when the player talks to them.</summary>
        public IEnumerable<Clue> LinesOf(Character character) => Clues.Where(c => c.Speaker == character);

        /// <summary>The objects lying in a room.</summary>
        public IEnumerable<Clue> ObjectsIn(string roomId) => Clues.Where(c => c.Room != null && c.Room.Room.Id == roomId);
    }
}
