using UnityEngine;
using UnityEngine.UI;

namespace TarasK8.UI.Animations.Transitions
{
    [System.Serializable]
    [TransitionMenuName("Canvas Group/Fade")]
    public class CanvasGroupFade : AnimatedProperty<CanvasGroupFade.Data>
    {
        [SerializeField] private float _duration = 0.2f;
        [SerializeField] private float _delay = 0f;
        [SerializeField] public Easing _easing;
        [SerializeField] private CanvasGroup _targetGroup;

        private Data _data;
        private float _currentAlpha;

        public override float Delay { get => _delay; protected set => _delay = value; }
        public override float Duration { get => _duration; protected set => _duration = value; }

        public override void Start(Data data)
        {
            _data = data;
            _currentAlpha = _targetGroup.alpha;
        }

        public override void Process(float t)
        {
            float lerp = _easing.Evaluate(t);
            _targetGroup.alpha = Mathf.LerpUnclamped(_currentAlpha, _data.Alpha, lerp);
        }

        [System.Serializable]
        public class Data : IAnimationData
        {
            [field: SerializeField] public string Name { get; set; }
            [SerializeField, Range(0f, 1f)] public float Alpha = 0f;
        }
    }
}