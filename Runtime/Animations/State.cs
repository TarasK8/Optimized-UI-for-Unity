using System;
using System.Collections.Generic;
using TarasK8.UI.Animations.AnimatedProperties;
using UnityEngine;
using UnityEngine.Serialization;

namespace TarasK8.UI.Animations
{
    [Serializable]
    public class State
    {
        public const string NameFieldName = "<Name>k__BackingField";
        [field: SerializeField] public string Name { get; private set; }
        [SerializeReference] private List<IAnimationData> _dataList;

        public State(string name)
        {
            Rename(name);
        }

        public void AddAnimationData(IAnimationData animationData)
        {
            _dataList.Add(animationData);
        }

        public IAnimationData GetAnimationData(int index)
        {
            return _dataList[index];
        }

        public void Rename(string newName)
        {
            if(string.IsNullOrEmpty(newName))
                throw new ArgumentNullException(nameof(newName));
            
            Name = newName;
        }
    }
}
