using UnityEngine;

namespace TarasK8.UI.Animations
{
    public class StateSelectorAttribute : PropertyAttribute
    {
        public readonly bool HasNone;
        public readonly string StateMachineFieldName;

        public StateSelectorAttribute(string stateMachineFieldName = null, bool hasNone = false)
        {
            HasNone = hasNone;
            StateMachineFieldName = stateMachineFieldName;
        }
    }
}