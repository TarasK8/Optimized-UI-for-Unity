using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Progress Bar/Multi Progress Bar")]
    [ExecuteInEditMode]
    public class MultiProgressBar : ProgressBarBase
    {
        [Header("Visual")]
        [SerializeField] private bool _ignoreLenght = true;
        [SerializeField] private bool _reversed = false;
        [SerializeField] private List<ProgressBarBase> _bars;

        private void OnValidate()
        {
            if (_bars.Remove(this))
            {
                Debug.LogWarning("MultiProgressBar removed from list");
            }
        }
        
        private void OnEnable()
        {
            OnSetValue(0f, Value);
        }
        
#if UNITY_EDITOR
        private void Update()
        {
            if(Application.isPlaying == false && _bars != null)
                OnSetValue(0f, Value);
        }
#endif

        protected override void OnSetValue(float oldValue, float newValue)
        {
            float position = CalculatePosition(newValue);
            float allLength = _bars.Sum(GetLength);
            
            float length = 0f;

            for (int i = 0; i < _bars.Count; i++)
            {
                int index = _reversed ? -i + _bars.Count - 1 : i;
                var bar = _bars[index];
                
                var globalPos = position * allLength;
                bar.Value = GetValue(globalPos - length, bar);
                
                length += GetLength(bar);
            }
        }

        private float GetLength(ProgressBarBase bar)
        {
            return _ignoreLenght ? 1f : bar.Length;
        }

        private float GetValue(float rawValue, ProgressBarBase bar)
        {
            if(_ignoreLenght)
                return rawValue * bar.Length;
            
            return rawValue;
        }

#if UNITY_EDITOR
        
        [ContextMenu("Sort Bars List By Position X")]
        private void SortListByX()
        {
            _bars.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
        }
        
        [ContextMenu("Sort Bars List By Position Y")]
        private void SortListByY()
        {
            _bars.Sort((a, b) => a.transform.position.y.CompareTo(b.transform.position.y));
        }
        
        [ContextMenu("Reverse Bars List")]
        private void ReverseList()
        {
            _bars.Reverse();
        }
        
        #endif
    }
}