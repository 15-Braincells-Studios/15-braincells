using TMPro;
using UnityEngine;

namespace Project.Scripts.Core
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string key = "ui_interact";
        [SerializeField] private string actionName; 

        private TMP_Text _text;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            if (_text == null)
                _text = GetComponentInChildren<TMP_Text>(true);
        }
        
        private void OnEnable()
        {
            Refresh();
            Localization.OnLanguageChanged += Refresh;
            InputPrompts.OnDeviceChanged += Refresh;
        }

        private void OnDisable()
        {
            Localization.OnLanguageChanged -= Refresh;
            InputPrompts.OnDeviceChanged -= Refresh;
        }

        // refresh inputs
        private void Refresh()
        {
            if (_text == null)
            {
                return;
            }

            string text = Localization.Get(key);

            if (!string.IsNullOrEmpty(actionName))
                text = text.Replace("{0}", InputPrompts.GetDisplayName(actionName));

            _text.text = text;
        }
    }
}