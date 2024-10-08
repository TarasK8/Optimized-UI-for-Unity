using System;
using UnityEngine;

namespace TarasK8.UI.Animations.Transitions
{
    [Serializable]
    [TransitionMenuName("Transform/Rotation Z")]
    public class RotationZ : Transition<RotationZ.Data>
    {
        [field: SerializeField] public override float Duration { get; protected set; }
        [field: SerializeField] public override float Delay { get; protected set; }
        [SerializeField] private Easing _easing;
        [SerializeField] private RectTransform _targetTransform;

        private Data _data;
        private float _current;

        public override void Start(Data data)
        {
            _data = data;
            _current = _targetTransform.rotation.eulerAngles.z;
        }

        public override void Process(float t)
        {
            float lerp = _easing.Evaluate(t);
            Vector3 currentRotation = _targetTransform.rotation.eulerAngles;
            float z = Mathf.LerpAngle(_current, _data.Rotation, lerp);
            _targetTransform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, z);
        }

        [Serializable]
        public class Data : IAnimationData
        {
            [field: SerializeField] public string Name { get; set; }
            [SerializeField] public float Rotation = 0f;
        }
    }
}