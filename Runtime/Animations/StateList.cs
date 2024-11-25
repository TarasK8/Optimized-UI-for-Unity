using System;
using System.Collections.Generic;
using System.Linq;
using TarasK8.UI.Animations.AnimatedProperties;
using UnityEngine;

namespace TarasK8.UI.Animations
{
    [Serializable]
    public class StateList
    {
        [SerializeField] private List<State> _states = new(8);
        
        public int Count => _states.Count;
        
        public State this[int index] => _states[index];
        
        public IEnumerable<string> GetAllNames()
        {
            return _states.Select(s => s.Name);
        }

        public void AddAnimationData(int stateIndex, IAnimationData animationData)
        {
            _states[stateIndex].AddAnimationData(animationData);
        }

        public bool ContainsName(string stateName)
        {
            return _states.Any(s => s.Name == stateName);
        }

        public IAnimationData GetAnimationData(int stateIndex, int animationDataIndex)
        {
            return _states[stateIndex].GetAnimationData(animationDataIndex);
        }

        public int NameToIndex(string stateName)
        {
            var index = _states.FindIndex(x => x.Name == stateName);
            
            if(index == -1)
                throw new ArgumentException($"State '{stateName}' not found.");
            
            return index;
        }

        public bool TryAddState(string name)
        {
            State state = new State(name);
            return TryAddState(state);
        }
        
        public bool TryAddState(State state)
        {
            if (ContainsName(state.Name))
            {
                //Debug.LogError($"State '{state.Name}' already exists.");
                return false;
            }
            _states.Add(state);
            return true;
        }

        public void Remove(int index)
        {
            _states.RemoveAt(index);
        }

        public void Rename(int index, string newName)
        {
            if (ContainsName(newName))
                throw new ArgumentException($"State '{newName}' already exists.");
            
            _states[index].Rename(newName);
        }
    }
}
