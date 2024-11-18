using System;
using System.Collections.Generic;
using System.Linq;
using TarasK8.UI.Animations.AnimatedProperties;
using TarasK8.UI.Animations.Tweening;
using UnityEngine;

namespace TarasK8.UI.Animations
{
    [AddComponentMenu("Optimized UI/Animation/State Machine")]
    public class StateMachine : MonoBehaviour
    {
        [SerializeField] private bool _ignoreTimeScale = true;
        [SerializeField] private bool _fullyComplateTransition = false;
        [SerializeField, StateSelector(hasNone:true)] private int _defaultState = -1;
        [SerializeReference] private List<AnimatedProperty> _animatedProperties = new();

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
            int index = StateNameToIndex(stateName);
            SetState(index, instantly: false);
        }

        public void SetStateInstantly(int stateIndex)
        {
            SetState(stateIndex, instantly: true);
        }
        
        public void SetStateInstantly(string stateName)
        {
            int index = StateNameToIndex(stateName);
            SetState(index, instantly: true);
        }

        public void AddState(string stateName)
        {
            if (ContainsStateName(stateName))
            {
                Debug.LogError($"Name '{stateName}' already exists!");
                return;
            }
            foreach (var transition in _animatedProperties)
                transition.AddState(stateName);
        }

        public void RemoveState(int index)
        {
            foreach (var transition in _animatedProperties)
                transition.RemoveState(index);
        }

        public void RenameState(int index, string newName)
        {
            if (ContainsStateName(newName))
            {
                Debug.LogError($"Name '{newName}' already exists!");
                return;
            }
            foreach (var transition in _animatedProperties)
                transition.RenameState(index, newName);
        }

        public void AddAnimatedProperty(Type type)
        {
            if (type.IsSubclassOf(typeof(AnimatedProperty)) == false && type.IsAbstract == false) return;

            AnimatedProperty animatedProperty = (AnimatedProperty)Activator.CreateInstance(type);
            if (_animatedProperties.Count > 0)
            {
                foreach (var state in _animatedProperties[0].GetStates())
                {
                    animatedProperty.AddState(state.Name);
                }
            }
            _animatedProperties.Add(animatedProperty);
        }

        public void RemoveAnimatedProperty(int index)
        {
            _animatedProperties.RemoveAt(index);
        }

        public string[] GetAllStateNames()
        {
            if (_animatedProperties.Count > 0)
                return _animatedProperties[0]?.GetAllStateNames();
            else
                return new string[0];
        }

        public bool ContainsStateName(string name)
        {
            return GetAllStateNames().Contains(name);
        }

        
        public int StateNameToIndex(string stateName)
        {
            if(_animatedProperties[0].TryFindState(stateName, out int stateIndex))
            {
                return stateIndex;
            }
            else
            {
                throw new ArgumentException($"State '{stateName}' not found.");
            }
        }

        private void SetState(int stateIndex, bool instantly)
        {
            if (CurrentState == stateIndex && instantly == false)
                return;

            CurrentState = stateIndex;

            foreach (var animatedProperty in _animatedProperties)
            {
                animatedProperty.SetState(stateIndex);
                if (animatedProperty.IsStarted && _fullyComplateTransition)
                    animatedProperty.Process(1f);
                animatedProperty.Reset();
                TweenManager.StartTween(animatedProperty, instantly: instantly, ignoreTimeScale: _ignoreTimeScale);
            }
        }
    }
}