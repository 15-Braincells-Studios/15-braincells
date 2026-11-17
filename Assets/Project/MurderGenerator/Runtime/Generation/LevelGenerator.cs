using System;
using FifteenBraincells.MurderGenerator.Data;
using FifteenBraincells.MurderGenerator.Output;

namespace FifteenBraincells.MurderGenerator.Generation
{
    /// <summary>Thrown when no valid level could be made within <see cref="GeneratorSettings.MaxAttempts"/>.</summary>
    public sealed class LevelGenerationException : Exception
    {
        public LevelGenerationException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Builds levels from a <see cref="GeneratorDatabase"/>. The same seed (and method) always gives the
    /// same level. The class keeps no state between calls, so one instance can be shared.
    /// </summary>
    /// <example>
    /// <code>
    /// var generator = new LevelGenerator(database);
    /// Level level = generator.Generate(seed: 2611);
    /// </code>
    /// </example>
    public sealed class LevelGenerator
    {
        private readonly GeneratorDatabase _database;

        public LevelGenerator(GeneratorDatabase database) =>
            _database = database ?? throw new ArgumentNullException(nameof(database));

        /// <summary>Generates a level.</summary>
        /// <param name="seed">Any number. Equal seeds give equal levels.</param>
        /// <param name="method">Forces a murder method. Null: picked at random.</param>
        /// <exception cref="LevelGenerationException">No valid level was found. Usually means the data is too thin.</exception>
        public Level Generate(int seed, Method method = null)
        {
            if (TryGenerate(seed, out var level, method))
            {
                return level;
            }

            throw new LevelGenerationException(
                $"No valid level for seed {seed}{(method == null ? "" : $" and method '{method.Id}'")} " +
                $"after {_database.Settings.MaxAttempts} attempts.");
        }

        /// <summary>Like <see cref="Generate"/>, but returns false instead of throwing.</summary>
        public bool TryGenerate(int seed, out Level level, Method method = null)
        {
            var rng = new Rng(seed);
            for (var attempt = 1; attempt <= _database.Settings.MaxAttempts; attempt++)
            {
                level = new LevelAttempt(_database, rng, method).TryBuild(seed, attempt);
                if (level != null)
                {
                    return true;
                }
            }

            level = null;
            return false;
        }
    }
}
