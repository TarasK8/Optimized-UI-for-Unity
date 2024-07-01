using UnityEngine;
using UnityEngine.Events;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Button")]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class Button : Selectable
    {
        [Space]
        [SerializeField] public float _doubleClickTheshold = 0.5f;
        [field: SerializeField] public UnityEvent OnClick { get; private set; } = new UnityEvent();
        [field: SerializeField] public UnityEvent OnDoubleClick { get; private set; } = new UnityEvent();

        private float _lastClickTime = Mathf.NegativeInfinity;

        protected override void Click()
        {
            OnClick?.Invoke();

            float difference = Time.time - _lastClickTime;
            if(difference < _doubleClickTheshold)
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