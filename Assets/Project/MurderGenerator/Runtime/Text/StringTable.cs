using System;
using System.Collections.Generic;
using FifteenBraincells.MurderGenerator.Data.Dto;

namespace FifteenBraincells.MurderGenerator.Text
{
    /// <summary>Looks up text by key. One table per language.</summary>
    public interface IStringTable
    {
        bool Contains(string key);

        /// <exception cref="KeyNotFoundException">The key does not exist.</exception>
        string Get(string key);
    }

    /// <summary>A string table read from a strings_&lt;language&gt;.json file.</summary>
    public sealed class StringTable : IStringTable
    {
        private readonly Dictionary<string, string> _values;

        public StringTable(StringTableDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            _values = new Dictionary<string, string>(dto.entries.Count);
            foreach (var entry in dto.entries)
            {
                if (_values.ContainsKey(entry.key))
                {
                    throw new ArgumentException($"Duplicate string key '{entry.key}'.", nameof(dto));
                }

                _values.Add(entry.key, entry.value);
            }
        }

        public int Count => _values.Count;

        public bool Contains(string key) => _values.ContainsKey(key);

        public string Get(string key) =>
            _values.TryGetValue(key, out var value)
                ? value
                : throw new KeyNotFoundException($"String key '{key}' does not exist.");
    }
}
