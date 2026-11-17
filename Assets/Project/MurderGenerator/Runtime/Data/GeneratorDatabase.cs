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
            IReadOnlyList<RedHerring> redHerrings)
        {
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

        public Character GetCharacter(string id) => _charactersById[id];

        public Room GetRoom(string id) => _roomsById[id];

        public Method GetMethod(string id) => _methodsById[id];

        public bool TryGetMethod(string id, out Method method) => _methodsById.TryGetValue(id, out method);

        public IReadOnlyList<MethodClue> GetMethodClues(Method method, ClueGroup group) =>
            _cluesByMethodAndGroup.TryGetValue((method, group), out var clues) ? clues : new MethodClue[0];
    }
}
