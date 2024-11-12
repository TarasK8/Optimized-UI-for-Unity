using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Progress Bar/Multi Progress Bar")]
    [ExecuteInEditMode]
    public class MultiProgressBar : ProgressBarBase
    {
        [SerializeField] private bool _ignoreLenght = true;
        [SerializeField] private bool _reversed = false;
        [SerializeField] private List<ProgressBarBase> _fillBars = new();

        public bool IgnoreLenght
        {
            get => _ignoreLenght;
            set
            {
                _ignoreLenght = value;
                base.UpdateValue();
            }
        }

        public bool Reversed
        {
            get => _reversed;
            set
            {
                _reversed = value;
                base.UpdateValue();
            }
        }

        private void OnValidate()
        {
            if (_fillBars.Remove(this))
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
            if(Application.isPlaying == false && _fillBars != null)
                base.UpdateValue();
        }
#endif

        protected override void OnSetValue(float oldValue, float newValue)
        {
            if (_fillBars == null || _fillBars.Count <= 0)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"MultiProgressBar does not contain any Fill Bar", this);
#endif
                return;
            }

            float position = CalculatePosition(newValue);
            float allLength = _fillBars.Sum(GetLength);
            
            float length = 0f;

            for (int i = 0; i < _fillBars.Count; i++)
            {
                int index = _reversed ? -i + _fillBars.Count - 1 : i;
                var bar = _fillBars[index];
                
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
        public void SortListByX()
        {
            _fillBars.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
        }
        
        [ContextMenu("Sort Bars List By Position Y")]
        public void SortListByY()
        {
            _fillBars.Sort((a, b) => a.transform.position.y.CompareTo(b.transform.position.y));
        }
        
        [ContextMenu("Reverse Bars List")]
        public void ReverseList()
        {
            _fillBars.Reverse();
        }
#endif
    }
}