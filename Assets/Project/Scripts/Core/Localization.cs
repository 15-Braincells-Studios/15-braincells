using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project.Scripts.Core
{
    [Serializable] public class LocalizationEntry { public string group; public string key; public string value; public bool isClue; }
    [Serializable] public class LocalizationData { public LocalizationEntry[] entries; }

    public static class Localization
    {
        private const string FallbackLanguage = "English";
        
        private const string ForcedLanguage = "English";

        private static Dictionary<string, string> _table = new();
        private static Dictionary<string, string> _fallback = new();
        
        // Stores which dialogue entries are important clues
        private static Dictionary<string, bool> _clueTable = new();
        private static Dictionary<string, bool> _fallbackClues = new();
        
        private static LocalizationEntry[] _entries = Array.Empty<LocalizationEntry>();
        private static LocalizationEntry[] _fallbackEntries = Array.Empty<LocalizationEntry>();

        public static string CurrentLanguage { get; private set; } = FallbackLanguage;
        public static event Action OnLanguageChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        // lets player choose language
        // private static void Init()
        // {
        //     _fallback = LoadTable(FallbackLanguage) ?? new();
        //     Load(PlayerPrefs.GetString("language", FallbackLanguage));
        // }
        
        // to manually force language
        private static void Init()
        {
            _fallback = LoadTable(FallbackLanguage) ?? new();

            string language = string.IsNullOrEmpty(ForcedLanguage)
                ? PlayerPrefs.GetString("language", FallbackLanguage)
                : ForcedLanguage;

            Load(language);
        }

        public static void Load(string language)
        {
            var table = LoadTable(language);
            if (table == null) return;

            _table = table;
            CurrentLanguage = language;
            PlayerPrefs.SetString("language", language);
            OnLanguageChanged?.Invoke();
        }

        public static bool Has(string key) =>
            _table.ContainsKey(key) || _fallback.ContainsKey(key);

        public static string Get(string key)
        {
            if (_table.TryGetValue(key, out var value)) return value;
            if (_fallback.TryGetValue(key, out value)) return value;
            return $"[{key}]";
        }
        // Checks if a dialogue entry is marked as an important clue
        public static bool IsClue(string key)
        {
            if (_clueTable.TryGetValue(key, out bool isClue))
                return isClue;
            if (_fallbackClues.TryGetValue(key, out isClue))
                return isClue;
            return false;
        }
        // Gets the group name for a dialogue entry
        public static string GetGroup(string key)
        {
            foreach (var entry in _entries)
            {
                if (entry.key == key)
                    return entry.group;
            }
            foreach (var entry in _fallbackEntries)
            {
                if (entry.key == key)
                    return entry.group;
            }
            return "";
        }

        private static Dictionary<string, string> LoadTable(string language)
        {
            var file = Resources.Load<TextAsset>(language);
            if (file == null)
            {
                Debug.LogError($"No localization file for '{language}'");
                return null;
            }

            var data = JsonUtility.FromJson<LocalizationData>(file.text);
            if (language == FallbackLanguage)
            {
                _fallbackClues = new Dictionary<string, bool>();
                _fallbackEntries = data.entries;

                foreach (var entry in data.entries)
                    _fallbackClues[entry.key] = entry.isClue;
            }
            else
            {
                _clueTable = new Dictionary<string, bool>();
                _entries = data.entries;

                foreach (var entry in data.entries)
                    _clueTable[entry.key] = entry.isClue;
            }
            return data.entries.ToDictionary(e => e.key, e => e.value);
        }
    }
}