using System;

namespace TarasK8.UI.Animations.Tweening
{
    public class WaitTween : Tween
    {
        public override float Delay { get; protected set; }
        public override float Duration { get; protected set; }

        private readonly Action _action;

        public WaitTween(Action action, float delay)
        {
            Delay = delay;
            _action = action;
        }

        public override void Start()
        {
            _action?.Invoke();
        }

        public override void Process(float t) { }
    }
}