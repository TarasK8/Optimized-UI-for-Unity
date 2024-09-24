using System;
using System.Collections.Generic;
using System.Linq;
using TarasK8.UI.Animations.Tweening;
using UnityEngine;

namespace TarasK8.UI.Animations
{
    [AddComponentMenu("Optimized UI/Animation/State Machine")]
    public class StateMachine : MonoBehaviour
    {
        [SerializeField] private bool _fullyComplateTransition = false;
        [SerializeField, StateSelector(hasNone:true)] private int _defaultState = -1;
        [SerializeReference] private List<Transition> _transitions = new();

        public int CurrentState { get; private set; } = -1;

        private void Start()
        {
            if (_defaultState < 0)
                return;

            SetStateInstantly(_defaultState);
        }

        public void SetState(int stateIndex)
        {
            if (CurrentState == stateIndex)
                return;

            CurrentState = stateIndex;

            foreach (var transition in _transitions)
            {
                transition.SetState(stateIndex);
                if (transition.IsStarted && _fullyComplateTransition)
                    transition.Process(1f);
                transition.Reset();
                TweenManager.StartTween(transition);
            }
        }

        public void SetState(string stateName)
        {
            SetState(StateNameToIndex(stateName));
        }

        public void SetStateInstantly(int stateIndex)
        {
            foreach (var transition in _transitions)
            {
                transition.SetState(stateIndex);
                if (transition.IsStarted && _fullyComplateTransition)
                    transition.Process(1f);
                transition.Reset();
                transition.Start();
                transition.Process(1f);
            }
        }

        public void AddState(string stateName)
        {
            if (ContainsStateName(stateName))
            {
                Debug.LogError($"Name '{stateName}' already exists!");
                return;
            }
            foreach (var transition in _transitions)
                transition.AddState(stateName);
        }

        public void RemoveState(int index)
        {
            foreach (var transition in _transitions)
                transition.RemoveState(index);
        }

        public void RenameState(int index, string newName)
        {
            if (ContainsStateName(newName))
            {
                Debug.LogError($"Name '{newName}' already exists!");
                return;
            }
            foreach (var transition in _transitions)
                transition.RenameState(index, newName);
        }

        public void AddTransition(Type type)
        {
            if (type.IsSubclassOf(typeof(Transition)) == false && type.IsAbstract == false) return;

            Transition transition = (Transition)Activator.CreateInstance(type);
            if (_transitions.Count > 0)
            {
                foreach (var state in _transitions[0].GetStates())
                {
                    transition.AddState(state.Name);
                }
            }
            _transitions.Add(transition);
        }

        public void RemoveTransition(int index)
        {
            _transitions.RemoveAt(index);
        }

        public string[] GetAllStateNames()
        {
            if (_transitions.Count > 0)
                return _transitions[0].GetAllStateNames();
            else
                return new string[0];
        }

        public bool ContainsStateName(string name)
        {
            return GetAllStateNames().Contains(name);
        }

        
        public int StateNameToIndex(string stateName)
        {
            if(_transitions[0].TryFindState(stateName, out int stateIndex))
            {
                return stateIndex;
            }
            else
            {
                throw new ArgumentException($"State '{stateName}' not found.");
            }
        }
    }
}