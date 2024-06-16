using System;
using TarasK8.UI.Animations;
using UnityEngine;

namespace TarasK8.UI.Animations.Transitions
{
    [TransitionMenuName("Rect Transform/Rotation")]
    public class Rotation : Transition<Rotation.Data>
    {
        [field: SerializeField] public override float Delay { get; protected set; }
        [field: SerializeField] public override float Duration { get; protected set; }

        private Data _data;

        public override void Process(float t)
        {
            //throw new System.NotImplementedException();
        }

        public override void Start(Data data)
        {
            _data = data;
        }

        [Serializable]
        public class Data : IAnimationData
        {
            [field: SerializeField] public string Name { get; set; }
        }
    }
}