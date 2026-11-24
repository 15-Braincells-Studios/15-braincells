using System;
using System.Collections.Generic;

namespace FifteenBraincells.MurderGenerator.Data
{
    /// <summary>Thrown when generator_data.json contains mistakes. <see cref="Errors"/> lists all of them.</summary>
    public sealed class GeneratorDataException : Exception
    {
        public GeneratorDataException(IReadOnlyList<string> errors)
            : base($"Generator data has {errors.Count} problem(s):\n- " + string.Join("\n- ", errors))
        {
            Errors = errors;
        }

        public IReadOnlyList<string> Errors { get; }
    }
}
