using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace TarasK8.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class TextChangedEvent : MonoBehaviour
    {
        [SerializeField] private UnityEvent<string> _event;

        private TMP_Text _text;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChange);
        }

        private void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChange);
        }

        private void OnTextChange(UnityEngine.Object obj)
        {
            if(obj == _text)
                _event?.Invoke(_text.text);
        }

        public void AddListener(UnityAction<string> call) => _event.AddListener(call);

        public void RemoveListener(UnityAction<string> call) => _event.RemoveListener(call);

        public void RemoveAllListeners() => _event.RemoveAllListeners();

    }
}