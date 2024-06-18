using UnityEngine;

namespace TarasK8.UI.Animations.Tweening
{
    [System.Serializable]
    public abstract class Tween
    {
        public abstract float Delay { get; protected set; }
        public abstract float Duration { get; protected set; }
        public float ElapsedTime { get; private set; }
        public bool IsCompleted { get; private set; }
        public bool IsStarted { get; private set; }
        public float Progress => (ElapsedTime - Delay) / Duration;

        public void Update(float deltaTime)
        {
            if (IsCompleted) return;

            ElapsedTime += deltaTime;

            if (ElapsedTime > Delay && IsStarted == false)
            {
                IsStarted = true;
                Start();
            }
            if (ElapsedTime < Delay)
            {
                return;
            }
            if (ElapsedTime - Delay >= Duration)
            {
                if(Mathf.Approximately(Duration, 0f))
                {
                    Process(1f);
                }
                Complate();
            }
            else
            {
                Process(Progress);
            }
        }

        public void Complate()
        {
            IsCompleted = true;
        }

        public virtual void Reset()
        {
            IsCompleted = false;
            IsStarted = false;
            ElapsedTime = 0f;
        }

        public abstract void Start();

        public abstract void Process(float t);

    }
}