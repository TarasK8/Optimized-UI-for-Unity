using UnityEngine;
using UnityEngine.UI;

namespace TarasK8.UI.Animations.Transitions
{
    [System.Serializable]
    [TransitionMenuName("Image/Color Tint")]
    public class ColorTint : Transition<ColorTint.Data>
    {
        [SerializeField] private float _duration = 0.2f;
        [SerializeField] private float _delay = 0f;
        [SerializeField] private bool _useAlpha = false;
        [SerializeField] private Graphic _targetGraphic;

        public override float Delay { get => _delay; protected set => _delay = value; }
        public override float Duration { get => _duration; protected set => _duration = value; }

        public override void Start(Data data)
        {
            _targetGraphic.CrossFadeColor(data.Color, Duration, true, _useAlpha);
        }

        public override void Process(float t) {}

        [System.Serializable]
        public class Data : IAnimationData
        {
            [field: SerializeField] public string Name { get; set; }
            [SerializeField] public UnityEngine.Color Color = UnityEngine.Color.white;
        }
    }
}