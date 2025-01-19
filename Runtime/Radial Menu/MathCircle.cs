using System;
using UnityEngine;

namespace TarasK8.UI
{
    public static class MathCircle
    {
        public const float Circle = 360f;
        
        public static float AngleToPosition(float angle)
        {
            float result = RepeatAngle(angle) / Circle;
            return result;
        }

        public static float PositionToAngle(float value)
        {
            return value * Circle;
        }

        public static float RepeatAngle(float angle)
        {
            angle = (float)Math.Round(angle, 3); // To avoid float precision troubles
            var result = Mathf.Repeat(angle, Circle);
            if(Mathf.Approximately(result, Circle)) return 0f;
            return result;
        }
    }
}
