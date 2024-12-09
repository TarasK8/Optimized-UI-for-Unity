using System;
using System.Collections.Generic;
using TarasK8.UI.Animations.AnimatedProperties;
using TarasK8.UI.Animations.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace TarasK8.UI.Animations
{
    [AddComponentMenu("Optimized UI/Animation/State Machine")]
    public class StateMachine : MonoBehaviour
    {
        [SerializeField] private bool _ignoreTimeScale = true;
        [SerializeField] private bool _fullyCompleteTransition = false;
        [SerializeField, StateSelector(hasNone:true)] private int _defaultState = -1;
        [SerializeReference] private List<AnimatedProperty> _animatedProperties = new();
        [SerializeField] private StateList _states = new();
        
        public StateList States => _states;

        public int CurrentState { get; private set; } = -1;

        private void Start()
        {
            if (_defaultState < 0)
                return;

            SetStateInstantly(_defaultState);
        }

        public void SetState(int stateIndex)
        {
            SetState(stateIndex, instantly: false);
        }

        public void SetState(string stateName)
        {
            int index = States.NameToIndex(stateName);
            SetState(index);
        }

        public void SetStateInstantly(int stateIndex)
        {
            SetState(stateIndex, instantly: true);
        }
        
        public void SetStateInstantly(string stateName)
        {
            int index = States.NameToIndex(stateName);
            SetStateInstantly(index);
        }
        
        public void AddAnimatedProperty(AnimatedProperty animatedProperty)
        {
            for (int i = 0; i < _states.Count; i++)
            {
                var data = animatedProperty.CreateNewAnimationData();
                _states.AddAnimationData(i, data);
            }
            _animatedProperties.Add(animatedProperty);
        }

        public void AddState(string stateName)
        {
            var state = new State(stateName);
            foreach (var animatedProperty in _animatedProperties)
            {
                var data = animatedProperty.CreateNewAnimationData();
                state.AddAnimationData(data);
            }
            _states.TryAddState(state);
        }

        public void RemoveAnimatedProperty(int index)
        {
            for (int i = 0; i < _states.Count; i++)
            {
                _states.RemoveAnimationData(i, index);
            }
            _animatedProperties.RemoveAt(index);
        }
        
        private void SetState(int stateIndex, bool instantly)
        {
            if (CurrentState == stateIndex && instantly == false)
                return;

            CurrentState = stateIndex;

            for (int i = 0; i < _animatedProperties.Count; i++)
            {
                var animatedProperty = _animatedProperties[i];
                if(animatedProperty.Enabled == false)
                    continue;
                animatedProperty.SetAnimationData(_states.GetAnimationData(stateIndex, i));
                if (animatedProperty.IsStarted && _fullyCompleteTransition)
                    animatedProperty.Process(1f);
                animatedProperty.Reset();
                TweenManager.StartTween(animatedProperty, instantly, _ignoreTimeScale);
            }
        }
    }
}