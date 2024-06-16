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
        public const string FULLY_COMPLATE_FIELD_NAME = nameof(_fullyComplateTransition);
        public const string TRANSITIONS_FIELD_NAME = nameof(_transitions);
        [SerializeField] private bool _fullyComplateTransition = false;
        [SerializeReference] private List<Transition> _transitions = new();

        private int _currentState = 0;


        public void SetState(int stateIndex)
        {
            if (_currentState == stateIndex) return;
            _currentState = stateIndex;

            foreach (var transition in _transitions)
            {
                transition.SetState(stateIndex);
                if (transition.IsStarted && _fullyComplateTransition)
                    transition.Process(1f);
                transition.Reset();
                TweenManager.StartTween(transition);
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

        /*
        private bool TryFindStateIndex(string stateName, out int stateIndex)
        {
            for (int i = 0; i < _states.Length; i++)
            {
                if (_states[i].Id == stateName)
                {
                    stateIndex = i;
                    return true;
                }
            }
            stateIndex = -1;
            return false;
        }
        */
    }
}