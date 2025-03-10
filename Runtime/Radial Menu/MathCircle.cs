using System;
using UnityEngine;

namespace TarasK8.UI
{
    public static class MathCircle
    {
        public const float Circle = 360f;
        
        public static float AngleToRatio(float angle)
        {
            float result = RepeatAngle(angle) / Circle;
            return result;
        }

        public static float RatioToAngle(float value)
        {
            return value * Circle;
        }

        public static float RepeatAngle(float angle)
        {
            if(Mathf.Approximately(angle, 0f)) return 0f;
            var remainder = Mathf.Floor(angle / Circle) * Circle;
            var result = angle - remainder;
            if(Mathf.Approximately(result, 0f)) return Circle;
            return result;
        }
    }
}
