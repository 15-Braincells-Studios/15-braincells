using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project.Scripts.Core
{
    [Serializable]
    public class LocalizationEntry
    {
        public string group;
        public string key;
        public string value;
    }
    [Serializable] public class LocalizationData { public LocalizationEntry[] entries; }

    public static class Localization
    {
        private const string FallbackLanguage = "English";
        
        private const string ForcedLanguage = "English";

        private static Dictionary<string, string> _table = new();
        private static Dictionary<string, string> _fallback = new();

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
            if (_table.TryGetValue(key, out var value))
                return value;

            if (_fallback.TryGetValue(key, out value))
                return value;

            return "[" + key + "]";
        }

        private static Dictionary<string, string> LoadTable(string language)
        {
            TextAsset file = Resources.Load<TextAsset>(language);

            if (file == null)
            {
                Debug.LogError($"No localization file for '{language}'");
                return null;
            }

            var data = JsonUtility.FromJson<LocalizationData>(file.text);

            return data.entries.ToDictionary(
                entry => entry.key,
                entry => entry.value
            );
        }
    }
}