using UnityEngine;
using UnityEngine.UI;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Progress Bar Trail")]
    [RequireComponent(typeof(ProgressBar))]
    public class ProgressBarTrail : MonoBehaviour
    {
        [SerializeField] private ProgressBar _trailBar;

        [Header("Color")]
        [SerializeField] private Image _targetImage;
        [SerializeField] private Color _increaseColor = new(0.2663314f, 0.8962264f, 0.5550476f);
        [SerializeField] private Color _decreaseColor = new(1f, 0.2705882f, 0.2705882f);

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
            UpdateColor(oldValue, newValue);

            _trailBar.Value = newValue;
            _trailBar.Center = _progressBar.Center;
        }

        private void UpdateColor(float oldValue, float newValue)
        {
            if (_targetImage != null)
            {
                if (newValue > oldValue)
                {
                    _targetImage.color = _increaseColor;
                }
                else if (newValue < oldValue)
                {
                    _targetImage.color = _decreaseColor;
                }
            }
        }
    }
}