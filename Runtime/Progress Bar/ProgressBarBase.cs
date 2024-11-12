using System;
using UnityEngine;

namespace TarasK8.UI
{
    public abstract class ProgressBarBase : MonoBehaviour
    {
        [SerializeField] private float _minValue = 0f;
        [SerializeField] private float _maxValue = 1f;
        [SerializeField] private float _value = 1f;

        public event Action<float, float> OnValueChanged;
        
        public float Value
        {
            get => _value;
            set => SetValue(value);
        }
        
        public float Length => MaxValue - MinValue;

        public virtual float MinValue
        {
            get => _minValue;
            set
            {
                _minValue = value;
                SetValue(Value);
            }
        }

        public virtual float MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value;
                UpdateValue();
            }
        }

        private void OnValidate()
        {
            if (_minValue > _maxValue)
            {
                _minValue = _maxValue - 0.01f;
            }
        }
        
        public void AddValue(float value)
        {
            Value += value;
        }

        public void UpdateValue()
        {
            SetValue(Value);
        }

        public float CalculatePosition(float value)
        {
            return Mathf.InverseLerp(MinValue, MaxValue, value);
        }

        private void SetValue(float value)
        {
            value = Mathf.Clamp(value, MinValue, MaxValue);
            
            if(ShouldFilterSameValues() && Mathf.Approximately(value, _value))
                return;
            
            float oldValue = _value;
            _value = value;
            OnSetValue(oldValue, value);
            OnValueChanged?.Invoke(oldValue, value);
        }

        protected abstract void OnSetValue(float oldValue, float newValue);

        protected virtual bool ShouldFilterSameValues()
        {
            return false;
        }
    }
}