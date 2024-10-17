using System;
using UnityEngine;
using UnityEngine.UI;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Progress Bar Trail")]
    [RequireComponent(typeof(ProgressBar))]
    public class ProgressBarTrail : MonoBehaviour
    {
        [SerializeField] private Color _increaseColor;
        [SerializeField] private Color _decreaseColor;
        [SerializeField] private Image _image;
        [SerializeField] private ProgressBar _trailBar;

        private ProgressBar _progressBar;

        private void Awake()
        {
            _progressBar = GetComponent<ProgressBar>();
        }

        private void OnEnable()
        {
            _progressBar.OnValueChanged += ProgressBar_OnValueChanged;
        }

        private void OnDisable()
        {
            _progressBar.OnValueChanged -= ProgressBar_OnValueChanged;
        }

        private void ProgressBar_OnValueChanged(float oldValue, float newValue)
        {
            if(newValue > oldValue)
            {
                _image.color = _increaseColor;
            }
            else if(newValue < oldValue)
            {
                _image.color = _decreaseColor;
            }

            _trailBar.Value = newValue;
            _trailBar.Center = _progressBar.Center;
        }
    }
}