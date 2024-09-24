using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Button")]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class Button : Selectable, IPointerClickHandler, ISubmitHandler
    {
        [Space]
        [SerializeField] public float _doubleClickTheshold = 0.5f;
        [field: SerializeField] public UnityEvent OnClick { get; private set; } = new UnityEvent();
        [field: SerializeField] public UnityEvent OnDoubleClick { get; private set; } = new UnityEvent();

        private float _lastClickTime = Mathf.NegativeInfinity;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            Press();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            Press();

            // if we get set disabled during the press
            // don't run the coroutine.
            if (!IsActive() || !IsInteractable())
                return;
        }

        private void Press()
        {
            if (!IsActive() || !IsInteractable())
                return;

            UISystemProfilerApi.AddMarker("Button.onClick", this);

            OnClick?.Invoke();

            float difference = Time.time - _lastClickTime;
            if (difference < _doubleClickTheshold)
            {
                OnDoubleClick?.Invoke();
                _lastClickTime = Mathf.NegativeInfinity;
            }
            else
            {
                _lastClickTime = Time.time;
            }
        }
    }
}