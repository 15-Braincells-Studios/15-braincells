using System.Collections.Generic;

namespace FifteenBraincells.MurderGenerator.Generation
{
    /// <summary>
    /// Seeded random numbers (SplitMix64). Used instead of System.Random so that a seed gives exactly the
    /// same level on every platform and .NET version, which matters for saving and sharing levels.
    /// </summary>
    public sealed class Rng
    {
        private ulong _state;

        public Rng(int seed) => _state = unchecked((ulong)seed * 0x9E3779B97F4A7C15UL + 0x632BE59BD9B4E019UL);

        /// <summary>A number from 0 (inclusive) to <paramref name="maxExclusive"/> (exclusive).</summary>
        public int Next(int maxExclusive) => (int)((NextUInt() * (ulong)maxExclusive) >> 32);

        /// <summary>A number from <paramref name="min"/> to <paramref name="max"/>, both inclusive.</summary>
        public int Range(int min, int max) => max <= min ? min : min + Next(max - min + 1);

        /// <summary>True with the given probability (0 to 1).</summary>
        public bool Chance(float probability) => NextDouble() < probability;

        public T Pick<T>(IReadOnlyList<T> items) => items[Next(items.Count)];

        /// <summary>A shuffled copy of the items (Fisher-Yates).</summary>
        public List<T> Shuffled<T>(IEnumerable<T> items)
        {
            var list = new List<T>(items);
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }

            return list;
        }

        private double NextDouble() => (NextULong() >> 11) * (1.0 / (1UL << 53));

        private uint NextUInt() => (uint)(NextULong() >> 32);

        private ulong NextULong()
        {
            unchecked
            {
                var z = _state += 0x9E3779B97F4A7C15UL;
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                return z ^ (z >> 31);
            }
        }
    }
}
