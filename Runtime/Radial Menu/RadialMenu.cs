using System;
using System.Collections.Generic;
using UnityEngine;

namespace TarasK8.UI
{
    [ExecuteInEditMode]
    public class RadialMenu : MonoBehaviour
    {
        [SerializeField] private List<RadialBarSegment> _radialElements;

        [SerializeField] private float _inputAngle;
        
        public IReadOnlyList<RadialBarSegment> AttachedElements => _radialElements;

        public RadialBarSegment GetElementAtDirection(Vector2 direction)
        {
            float angle = Vector2.Angle(Vector2.up, direction);
            return GetElementAtAngle(angle);
        }

        public RadialBarSegment GetElementAtAngle(float angle)
        {
            for (var i = 0; i < _radialElements.Count; i++)
            {
                var segment = _radialElements[i];
                if (angle > segment.GlobalStartAngle &&
                    angle < segment.GlobalEndAngle)
                {
                    return segment;
                }
            }
            return null;
        }
    }
}
