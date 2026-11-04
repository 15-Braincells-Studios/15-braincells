using TMPro;
using UnityEngine;

namespace Project.Scripts.Core
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string key = "ui_interact";

        private TMP_Text _text;

        private void Awake() => _text = GetComponent<TMP_Text>();

        private void OnEnable()
        {
            Refresh();
            Localization.OnLanguageChanged += Refresh;
        }

        private void OnDisable()
        {
            Localization.OnLanguageChanged -= Refresh;
        }

        private void Refresh() => _text.text = Localization.Get(key);
    }
}