using UnityEngine;

namespace TarasK8.UI.Animations
{
    [System.Serializable]
    public partial class Easing
    {
        [SerializeField] private AnimationCurve _customCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        [SerializeField] private Type _type;

        public float Evaluate(float t) => _type switch
        {
            Type.Linear => t,
            Type.Custom => _customCurve.Evaluate(t),
            Type.InQuad => InQuad(t),
            Type.OutQuad => OutQuad(t),
            Type.InOutQuad => InOutQuad(t),
            Type.InCubic => InCubic(t),
            Type.OutCubic => OutCubic(t),
            Type.InOutCubic => InOutCubic(t),
            Type.InQuart => InQuart(t),
            Type.OutQuart => OutQuart(t),
            Type.InOutQuart => InOutQuart(t),
            Type.InQuint => InQuint(t),
            Type.OutQuint => OutQuint(t),
            Type.InOutQuint => InOutQuint(t),
            Type.InSine => InSine(t),
            Type.OutSine => OutSine(t),
            Type.InOutSine => InOutSine(t),
            Type.InExpo => InExpo(t),
            Type.OutExpo => OutExpo(t),
            Type.InOutExpo => InOutExpo(t),
            Type.InCirc => InCirc(t),
            Type.OutCirc => OutCirc(t),
            Type.InOutCirc => InOutCirc(t),
            Type.InElastic => InElastic(t),
            Type.OutElastic => OutElastic(t),
            Type.InOutElastic => InOutElastic(t),
            Type.InBack => InBack(t),
            Type.OutBack => OutBack(t),
            Type.InOutBack => InOutBack(t),
            Type.InBounce => InBounce(t),
            Type.OutBounce => OutBounce(t),
            Type.InOutBounce => InOutBounce(t),
            _ => t,
        };
        

        // Functions stolen from: https://gist.github.com/Kryzarel/bba64622057f21a1d6d44879f9cd7bd4 xD

        public static float Linear(float t) => t;

        public static float InQuad(float t) => t * t;
        public static float OutQuad(float t) => 1 - InQuad(1 - t);
        public static float InOutQuad(float t)
        {
            if (t < 0.5) return InQuad(t * 2) / 2;
            return 1 - InQuad((1 - t) * 2) / 2;
        }

        public static float InCubic(float t) => t * t * t;
        public static float OutCubic(float t) => 1 - InCubic(1 - t);
        public static float InOutCubic(float t)
        {
            if (t < 0.5) return InCubic(t * 2) / 2;
            return 1 - InCubic((1 - t) * 2) / 2;
        }

        public static float InQuart(float t) => t * t * t * t;
        public static float OutQuart(float t) => 1 - InQuart(1 - t);
        public static float InOutQuart(float t)
        {
            if (t < 0.5) return InQuart(t * 2) / 2;
            return 1 - InQuart((1 - t) * 2) / 2;
        }

        public static float InQuint(float t) => t * t * t * t * t;
        public static float OutQuint(float t) => 1 - InQuint(1 - t);
        public static float InOutQuint(float t)
        {
            if (t < 0.5) return InQuint(t * 2) / 2;
            return 1 - InQuint((1 - t) * 2) / 2;
        }

        //public static float InSine(float t) => (float)-Math.Cos(t * Math.PI / 2);
        public static float InSine(float t) => 1 - Mathf.Cos(t * Mathf.PI / 2);
        public static float OutSine(float t) => Mathf.Sin(t * Mathf.PI / 2);
        public static float InOutSine(float t) => (Mathf.Cos(t * Mathf.PI) - 1) / -2;

        public static float InExpo(float t) => Mathf.Pow(2, 10 * (t - 1));
        public static float OutExpo(float t) => 1 - InExpo(1 - t);
        public static float InOutExpo(float t)
        {
            if (t < 0.5) return InExpo(t * 2) / 2;
            return 1 - InExpo((1 - t) * 2) / 2;
        }

        public static float InCirc(float t) => -(Mathf.Sqrt(1 - t * t) - 1);
        public static float OutCirc(float t) => 1 - InCirc(1 - t);
        public static float InOutCirc(float t)
        {
            if (t < 0.5) return InCirc(t * 2) / 2;
            return 1 - InCirc((1 - t) * 2) / 2;
        }

        public static float InElastic(float t) => 1 - OutElastic(1 - t);
        public static float OutElastic(float t)
        {
            float p = 0.3f;
            return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - p / 4) * (2 * Mathf.PI) / p) + 1;
        }
        public static float InOutElastic(float t)
        {
            if (t < 0.5) return InElastic(t * 2) / 2;
            return 1 - InElastic((1 - t) * 2) / 2;
        }

        public static float InBack(float t)
        {
            float s = 1.70158f;
            return t * t * ((s + 1) * t - s);
        }
        public static float OutBack(float t) => 1 - InBack(1 - t);
        public static float InOutBack(float t)
        {
            if (t < 0.5) return InBack(t * 2) / 2;
            return 1 - InBack((1 - t) * 2) / 2;
        }

        public static float InBounce(float t) => 1 - OutBounce(1 - t);
        public static float OutBounce(float t)
        {
            float div = 2.75f;
            float mult = 7.5625f;

            if (t < 1 / div)
            {
                return mult * t * t;
            }
            else if (t < 2 / div)
            {
                t -= 1.5f / div;
                return mult * t * t + 0.75f;
            }
            else if (t < 2.5 / div)
            {
                t -= 2.25f / div;
                return mult * t * t + 0.9375f;
            }
            else
            {
                t -= 2.625f / div;
                return mult * t * t + 0.984375f;
            }
        }
        public static float InOutBounce(float t)
        {
            if (t < 0.5) return InBounce(t * 2) / 2;
            return 1 - InBounce((1 - t) * 2) / 2;
        }
    }
}