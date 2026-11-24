using System.Collections.Generic;
using System.Linq;

namespace FifteenBraincells.MurderGenerator.Data
{
    /// <summary>
    /// All validated content the generator works with. Build one with <see cref="DatabaseBuilder"/>
    /// and share it: it is never changed after building.
    /// </summary>
    public sealed class GeneratorDatabase
    {
        private readonly Dictionary<string, Character> _charactersById;
        private readonly Dictionary<string, Room> _roomsById;
        private readonly Dictionary<string, Method> _methodsById;
        private readonly Dictionary<(Method, ClueGroup), IReadOnlyList<MethodClue>> _cluesByMethodAndGroup;
        private readonly Dictionary<Method, IReadOnlyList<VictimPlan>> _plansByMethod;

        internal GeneratorDatabase(
            GeneratorSettings settings,
            IReadOnlyList<Character> characters,
            IReadOnlyList<Room> rooms,
            IReadOnlyList<Method> methods,
            IReadOnlyList<ClueGroupRule> clueGroups,
            IReadOnlyList<MethodClue> methodClues,
            IReadOnlyList<Motive> motives,
            IReadOnlyList<Alibi> alibis,
            IReadOnlyList<Contradiction> contradictions,
            IReadOnlyList<RedHerring> redHerrings,
            IReadOnlyList<VictimPlan> victimPlans,
            IReadOnlyList<IReadOnlyList<string>> closableRooms,
            IReadOnlyCollection<string> closedRoomIds = null)
        {
            ClosableRooms = closableRooms;
            ClosedRoomIds = closedRoomIds ?? new string[0];
            Settings = settings;
            Characters = characters;
            Rooms = rooms;
            Methods = methods;
            ClueGroups = clueGroups;
            MethodClues = methodClues;
            Motives = motives;
            Alibis = alibis;
            Contradictions = contradictions;
            RedHerrings = redHerrings;
            VictimPlans = victimPlans;
            _plansByMethod = victimPlans.GroupBy(p => p.Method).ToDictionary(g => g.Key, g => (IReadOnlyList<VictimPlan>)g.ToList());

            _charactersById = characters.ToDictionary(c => c.Id);
            _roomsById = rooms.ToDictionary(r => r.Id);
            _methodsById = methods.ToDictionary(m => m.Id);
            _cluesByMethodAndGroup = methodClues
                .GroupBy(c => (c.Method, c.Group))
                .ToDictionary(g => g.Key, g => (IReadOnlyList<MethodClue>)g.ToList());
        }

        public GeneratorSettings Settings { get; }
        public IReadOnlyList<Character> Characters { get; }
        public IReadOnlyList<Room> Rooms { get; }
        public IReadOnlyList<Method> Methods { get; }
        public IReadOnlyList<ClueGroupRule> ClueGroups { get; }
        public IReadOnlyList<MethodClue> MethodClues { get; }
        public IReadOnlyList<Motive> Motives { get; }
        public IReadOnlyList<Alibi> Alibis { get; }
        public IReadOnlyList<Contradiction> Contradictions { get; }
        public IReadOnlyList<RedHerring> RedHerrings { get; }
        public IReadOnlyList<VictimPlan> VictimPlans { get; }

        /// <summary>
        /// The rooms that may be closed for a level. Rooms in one entry are always closed together (the two
        /// bathrooms). See <see cref="WithClosedRooms"/>.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<string>> ClosableRooms { get; }

        /// <summary>The rooms that are closed in this database. Empty for the full database.</summary>
        public IReadOnlyCollection<string> ClosedRoomIds { get; }

        public bool IsClosed(string roomId) => ClosedRoomFilter.IsRoomClosed(ClosedRoomIds.ToList(), roomId);

        /// <summary>Whether an alibi line can be used: it does not lie in or talk about a closed room.</summary>
        public bool IsOpen(AlibiVariant variant) =>
            ClosedRoomIds.Count == 0
            || (!IsClosed(variant.Line.Room.RoomId) && !variant.MentionedRooms.Any(ClosedRoomIds.Contains));

        /// <summary>
        /// A copy of this database with some rooms closed. A closed room is left out of the game completely:
        /// no method, alibi, plan, clue, motive, red herring or contradiction takes place there or talks about it.
        /// Each generated level gets a note on the door of every closed room. Rooms that belong together
        /// (both bathrooms) are closed together. Build one <see cref="Generation.LevelGenerator"/> per choice of rooms.
        /// </summary>
        /// <param name="roomIds">Ids from <see cref="ClosableRooms"/>, e.g. "armory" or "royal_bathroom".</param>
        public GeneratorDatabase WithClosedRooms(IEnumerable<string> roomIds) => ClosedRoomFilter.Apply(this, roomIds);

        /// <summary>The plans a victim (or decoy) can have with this method. Empty for Fixed methods.</summary>
        public IReadOnlyList<VictimPlan> GetPlans(Method method) =>
            _plansByMethod.TryGetValue(method, out var plans) ? plans : new VictimPlan[0];

        public Character GetCharacter(string id) => _charactersById[id];

        public Room GetRoom(string id) => _roomsById[id];

        public Method GetMethod(string id) => _methodsById[id];

        public bool TryGetMethod(string id, out Method method) => _methodsById.TryGetValue(id, out method);

        public IReadOnlyList<MethodClue> GetMethodClues(Method method, ClueGroup group) =>
            _cluesByMethodAndGroup.TryGetValue((method, group), out var clues) ? clues : new MethodClue[0];
    }
}
