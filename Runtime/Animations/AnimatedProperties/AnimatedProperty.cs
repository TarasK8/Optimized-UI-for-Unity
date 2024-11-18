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
        public const string STATES_FIELD_NAME = "_states";

        public abstract void SetState(int index);

        public abstract void AddState(string stateName);

        public abstract void RemoveState(int index);

        public abstract void RenameState(int index, string newName);

        public abstract List<IAnimationData> GetStates();

        public abstract string[] GetAllStateNames();

        public abstract bool TryFindState(string stateName, out int index);
    }

    [Serializable]
    public abstract class AnimatedProperty<T> : AnimatedProperty
        where T : IAnimationData, new()
    {
        [SerializeReference] private List<T> _states = new(2);

        private int _currentStateIndex = 0;

        public override void SetState(int stateIndex)
        {
            if (stateIndex >= 0 || stateIndex < _states.Count)
                _currentStateIndex = stateIndex;
            else
                throw new ArgumentOutOfRangeException(nameof(stateIndex));
        }

        public override void AddState(string stateName)
        {
            T newState = new()
            {
                Name = stateName
            };
            _states.Add(newState);
        }

        public override void RemoveState(int stateIndex)
        {
            _states.RemoveAt(stateIndex);
        }

        public override void RenameState(int index, string newName)
        {
            _states[index].Name = newName;
        }

        public override List<IAnimationData> GetStates()
        {
            return _states.ConvertAll(new Converter<T, IAnimationData>(AnimationDataConverter));
        }

        public override string[] GetAllStateNames()
        {
            return _states.Select(x => x.Name).ToArray();
        }

        public override bool TryFindState(string stateName, out int index)
        {
            for (int i = 0; i < _states.Count; i++)
            {
                if (_states[i].Name == stateName)
                {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }

        public override void Start()
        {
            Start(_states[_currentStateIndex]);
        }

        public abstract void Start(T data);

        private IAnimationData AnimationDataConverter(T data)
        {
            return data;
        }
    }

    public interface IAnimationData
    {
        public const string NAME_FIELD_NAME = "<Name>k__BackingField";
        public string Name { get; set; }
    }
}