using System.Collections.Generic;
using System.Linq;
using FifteenBraincells.MurderGenerator.Data;

namespace FifteenBraincells.MurderGenerator.Output
{
    /// <summary>The hidden truth of a level.</summary>
    public sealed class Solution
    {
        internal Solution(Method method, PlacedRoom murderRoom, ClockTime murderTime, VictimPlan victimPlan,
            Character suspect, Character victim, Motive motive, Alibi fakeAlibi, Contradiction contradiction)
        {
            MurderTime = murderTime;
            VictimPlan = victimPlan;
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

        /// <summary>When the murder happens tonight.</summary>
        public ClockTime MurderTime { get; }

        /// <summary>The victim's plan for tonight, or null for a Fixed method.</summary>
        public VictimPlan VictimPlan { get; }
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

    /// <summary>A plan for tonight: the victim's, or a decoy's. Not an alibi.</summary>
    public sealed class PlanAssignment
    {
        internal PlanAssignment(Character holder, VictimPlan plan, bool isVictims)
        {
            Holder = holder;
            Plan = plan;
            IsVictims = isVictims;
        }

        public Character Holder { get; }
        public VictimPlan Plan { get; }

        /// <summary>False: a decoy, someone else with the same kind of plan.</summary>
        public bool IsVictims { get; }
    }

    /// <summary>A generated level: the solution, the clues, and who is where tonight.</summary>
    public sealed class Level
    {
        internal Level(int seed, int attempts, Solution solution, IReadOnlyList<Clue> clues,
            IReadOnlyList<AlibiAssignment> alibis, IReadOnlyDictionary<Character, Need> blocked,
            IReadOnlyList<PlanAssignment> plans)
        {
            Plans = plans;
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

        /// <summary>Plans for tonight (victim and decoys). Empty for Fixed methods.</summary>
        public IReadOnlyList<PlanAssignment> Plans { get; }

        /// <summary>What characters cannot do today because of a method clue. Only characters with blocks are listed.</summary>
        public IReadOnlyDictionary<Character, Need> Blocked { get; }

        /// <summary>
        /// The lines a character says when the player talks to them (at most two): first the line about their own
        /// evening, then the one about others or about something they saw or heard.
        /// </summary>
        public IEnumerable<Clue> LinesOf(Character character) =>
            Clues.Where(c => c.Speaker == character).OrderBy(c => c.IsAboutSpeaker ? 0 : 1);

        /// <summary>
        /// The five correct pieces of evidence, in the order of <see cref="EvidenceRole"/>: the group A method clue,
        /// the group B method clue, the murderer's fake alibi, the contradiction that breaks it, and the motive.
        /// </summary>
        public IReadOnlyList<Clue> Evidence => Clues.Where(c => c.IsEvidence).OrderBy(c => (int)c.Evidence).ToList();

        /// <summary>The clue that is this piece of evidence.</summary>
        public Clue EvidenceFor(EvidenceRole role) => Clues.Single(c => c.Evidence == role);

        /// <summary>
        /// Whether the player handed in exactly the right evidence: all five correct clues and nothing else.
        /// The order does not matter. Pass the <see cref="Clue"/> objects from <see cref="Clues"/>.
        /// </summary>
        public bool IsCorrectEvidence(IEnumerable<Clue> handedIn)
        {
            var given = new HashSet<Clue>(handedIn ?? Enumerable.Empty<Clue>());
            return given.Count == Evidence.Count && given.SetEquals(Evidence);
        }

        /// <summary>The objects lying in a room.</summary>
        public IEnumerable<Clue> ObjectsIn(string roomId) => Clues.Where(c => c.Room != null && c.Room.Room.Id == roomId);
    }
}
