using System;
using System.Collections.Generic;
using System.Linq;
using TarasK8.UI.Animations.Tweening;
using UnityEngine;

namespace TarasK8.UI.Animations.AnimatedProperties
{
    [Serializable]
    public abstract class AnimatedProperty : Tween
    {
        [SerializeField] private bool _enabled = true;
        
        public bool Enabled { get => _enabled; set => _enabled = value; }
        
        public abstract void SetAnimationData(IAnimationData data);
        
        public abstract IAnimationData CreateNewAnimationData();
    }

    [Serializable]
    public abstract class AnimatedProperty<T> : AnimatedProperty
        where T : IAnimationData, new()
    {
        private T _data;

        public override void SetAnimationData(IAnimationData data)
        {
            _data = (T)data;
        }

        public override IAnimationData CreateNewAnimationData()
        {
            return new T();
        }

        public override void Start()
        {
            Start(_data);
        }

        public abstract void Start(T data);

        private IAnimationData AnimationDataConverter(T data)
        {
            return data;
        }
    }

    public interface IAnimationData
    { }
}