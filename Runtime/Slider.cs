using System;
using TarasK8.UI.Animations;
using TarasK8.UI.Utilites;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Slider")]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class Slider : Selectable, IDragHandler, IInitializePotentialDragHandler
    {
        // Handle
        public const string HANDLE_RECT_FIELD = nameof(_handleRect);
        public const string FILL_RECT_FIELD = nameof(_fillRect);
        public const string DIRECTION_FIELD = nameof(_direction);
        public const string MAGNET_TO_CURSOR_FIELD = nameof(_magnetToCursor);
        // Value
        public const string MIN_VALUE_FIELD = nameof(_minValue);
        public const string MAX_VALUE_FIELD = nameof(_maxValue);
        public const string STEP_FIELD = nameof(_step);
        public const string CURVE_FIELD = nameof(_curve);
        public const string VALUE_FIELD = nameof(_value);
        // Display
        public const string TEXT_FIELD = nameof(_text);
        public const string FORMAT_FIELD = nameof(_format);
        // Events
        public const string ON_VALUE_CHANGED_FIELD = nameof(_onValueChanged);

        [Header("Handle")]
        [SerializeField] private RectTransform _handleRect;
        [SerializeField] private RectTransform _fillRect;
        [SerializeField] private Direction _direction = Direction.LeftToRight;
        [SerializeField] private bool _magnetToCursor = true;
        [Header("Value")]
        [SerializeField] private float _minValue = 0;
        [SerializeField] private float _maxValue = 1;
        [SerializeField, Min(0f)] private float _step;
        [SerializeField] private Easing _curve;
        [SerializeField] protected float _value;
        [Header("Display")]
        [SerializeField] private TMP_Text _text;
        [SerializeField] private string _format = "Value: {0:N}";
        [Space]
        [SerializeField] private SliderEvent _onValueChanged = new SliderEvent();

        private Image _fillImage;
        private Transform _fillTransform;
        private RectTransform _fillContainerRect;
        private Transform _handleTransform;
        private RectTransform _handleContainerRect;
        private Vector2 _offset = Vector2.zero;
        private DrivenRectTransformTracker _tracker;
        private bool _delayedUpdateVisuals = false;
        private float _stepSize => Mathf.Approximately(ValueStep, 0f) == false ? ValueStep : (MaxValue - MinValue) * 0.1f;
        private bool _reverseValue => _direction == Direction.RightToLeft || _direction == Direction.TopToBottom;
        private Axis _axis => (_direction == Direction.LeftToRight || _direction == Direction.RightToLeft) ? Axis.Horizontal : Axis.Vertical;

        public RectTransform FillRect
        {
            get
            {
                return _fillRect;
            }
            set
            {
                if (SetPropertyUtility.SetClass(ref _fillRect, value))
                {
                    UpdateCachedReferences();
                    UpdateVisuals();
                }
            }
        }
        public RectTransform HandleRect
        {
            get
            {
                return _handleRect;
            }
            set
            {
                if (SetPropertyUtility.SetClass(ref _handleRect, value))
                {
                    UpdateCachedReferences();
                    UpdateVisuals();
                }
            }
        }
        public Direction HandleDirection
        {
            get
            {
                return _direction;
            }
            set
            {
                if (SetPropertyUtility.SetStruct(ref _direction, value))
                    UpdateVisuals();
            }
        }
        public float MinValue
        {
            get 
            { 
                return _minValue; 
            }
            set 
            {
                if (SetPropertyUtility.SetStruct(ref _minValue, value))
                { 
                    Set(_value); 
                    UpdateVisuals();
                }
            }
        }
        public float MaxValue
        {
            get
            {
                return _maxValue;
            }
            set
            {
                if (SetPropertyUtility.SetStruct(ref _maxValue, value))
                {
                    Set(_value);
                    UpdateVisuals();
                }
            }
        }
        public float NormalizedValue
        {
            get
            {
                if (Mathf.Approximately(MinValue, MaxValue))
                    return 0;
                return Mathf.InverseLerp(MinValue, MaxValue, _value);
            }
            set
            {
                this.Value = Mathf.Lerp(MinValue, MaxValue, value);
            }
        }
        public virtual float Value
        {
            get => _value;
            set => Set(value);
        }
        public float CurvedValue => Mathf.Lerp(MinValue, MaxValue, _curve.Evaluate(NormalizedValue));
        public float ValueStep
        {
            get
            {
                return _step;
            }
            set
            {
                _step = value;
            }
        }
        public SliderEvent OnValueChanged
        {
            get => _onValueChanged;
            set => _onValueChanged = value;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (Mathf.Approximately(ValueStep, 0f) == false)
            {
                _minValue = Snapping.Snap(_minValue, ValueStep);
                _maxValue = Snapping.Snap(_maxValue, ValueStep);
            }

            if (IsActive())
            {
                UpdateCachedReferences();
                _delayedUpdateVisuals = true;
            }
        }

        protected virtual void Update()
        {
            if (_delayedUpdateVisuals)
            {
                _delayedUpdateVisuals = false;
                Set(_value, false);
                UpdateVisuals();
            }
        }
#endif

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateCachedReferences();
            Set(_value, false);
            // Update rects since they need to be initialized correctly.
            UpdateVisuals();
        }

        protected override void OnDisable()
        {
            _tracker.Clear();
            base.OnDisable();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            //This can be invoked before OnEnabled is called. So we shouldn't be accessing other objects, before OnEnable is called.
            if (!IsActive())
                return;

            UpdateVisuals();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            // Has value changed? Various elements of the slider have the old normalisedValue assigned, we can use this to perform a comparison.
            // We also need to ensure the value stays within min/max.
            _value = ClampValue(_value);
            float oldNormalizedValue = NormalizedValue;
            if (_fillContainerRect != null)
            {
                if (_fillImage != null && _fillImage.type == Image.Type.Filled)
                    oldNormalizedValue = _fillImage.fillAmount;
                else
                    oldNormalizedValue = (_reverseValue ? 1 - _fillRect.anchorMin[(int)_axis] : _fillRect.anchorMax[(int)_axis]);
            }
            else if (_handleContainerRect != null)
                oldNormalizedValue = (_reverseValue ? 1 - _handleRect.anchorMin[(int)_axis] : _handleRect.anchorMin[(int)_axis]);

            UpdateVisuals();

            if (oldNormalizedValue != NormalizedValue)
            {
                UISystemProfilerApi.AddMarker("Slider.value", this);
                OnValueChanged.Invoke(CurvedValue);
            }
            // UUM-34170 Apparently, some properties on slider such as IsInteractable and Normalcolor Animation is broken.
            // We need to call base here to render the animation on Scene
            base.OnDidApplyAnimationProperties();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            base.OnPointerDown(eventData);

            _offset = Vector2.zero;

            if (_magnetToCursor == false)
            {
                _offset = eventData.pointerPressRaycast.screenPosition - (Vector2)_handleRect.position;
            }
            else if (_handleContainerRect is not null && RectTransformUtility.RectangleContainsScreenPoint(_handleRect, eventData.pointerPressRaycast.screenPosition, eventData.enterEventCamera))
            {
                Vector2 localMousePos;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_handleRect, eventData.pointerPressRaycast.screenPosition, eventData.pressEventCamera, out localMousePos))
                    _offset = localMousePos;
            }
            else
            {
                // Outside the slider handle - jump to this point instead
                UpdateDrag(eventData, eventData.pressEventCamera);
            }
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;
            UpdateDrag(eventData, eventData.pressEventCamera);
        }

        public override void OnMove(AxisEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
            {
                base.OnMove(eventData);
                return;
            }

            switch (eventData.moveDir)
            {
                case MoveDirection.Left:
                    if (_axis == Axis.Horizontal && FindSelectableOnLeft() == null)
                        Set(_reverseValue ? Value + _stepSize : Value - _stepSize);
                    else
                        base.OnMove(eventData);
                    break;
                case MoveDirection.Right:
                    if (_axis == Axis.Horizontal && FindSelectableOnRight() == null)
                        Set(_reverseValue ? Value - _stepSize : Value + _stepSize);
                    else
                        base.OnMove(eventData);
                    break;
                case MoveDirection.Up:
                    if (_axis == Axis.Vertical && FindSelectableOnUp() == null)
                        Set(_reverseValue ? Value - _stepSize : Value + _stepSize);
                    else
                        base.OnMove(eventData);
                    break;
                case MoveDirection.Down:
                    if (_axis == Axis.Vertical && FindSelectableOnDown() == null)
                        Set(_reverseValue ? Value + _stepSize : Value - _stepSize);
                    else
                        base.OnMove(eventData);
                    break;
            }
        }

        public void SetDirection(Direction direction, bool includeRectLayouts)
        {
            Axis oldAxis = _axis;
            bool oldReverse = _reverseValue;
            this.HandleDirection = direction;

            if (!includeRectLayouts)
                return;

            if (_axis != oldAxis)
                RectTransformUtility.FlipLayoutAxes(transform as RectTransform, true, true);

            if (_reverseValue != oldReverse)
                RectTransformUtility.FlipLayoutOnAxis(transform as RectTransform, (int)_axis, true, true);
        }

        public override UnityEngine.UI.Selectable FindSelectableOnLeft()
        {
            if (navigation.mode == Navigation.Mode.Automatic && _axis == Axis.Horizontal)
                return null;
            return base.FindSelectableOnLeft();
        }

        public override UnityEngine.UI.Selectable FindSelectableOnRight()
        {
            if (navigation.mode == Navigation.Mode.Automatic && _axis == Axis.Horizontal)
                return null;
            return base.FindSelectableOnRight();
        }

        public override UnityEngine.UI.Selectable FindSelectableOnUp()
        {
            if (navigation.mode == Navigation.Mode.Automatic && _axis == Axis.Vertical)
                return null;
            return base.FindSelectableOnUp();
        }

        public override UnityEngine.UI.Selectable FindSelectableOnDown()
        {
            if (navigation.mode == Navigation.Mode.Automatic && _axis == Axis.Vertical)
                return null;
            return base.FindSelectableOnDown();
        }

        private void UpdateVisuals()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UpdateCachedReferences();
#endif

            _tracker.Clear();

            float normalizedValue = NormalizedValue;

            if (_fillContainerRect != null)
            {
                _tracker.Add(this, _fillRect, DrivenTransformProperties.Anchors);
                Vector2 anchorMin = Vector2.zero;
                Vector2 anchorMax = Vector2.one;

                if (_fillImage != null && _fillImage.type == Image.Type.Filled)
                {
                    _fillImage.fillAmount = normalizedValue;
                }
                else
                {
                    if (_reverseValue)
                        anchorMin[(int)_axis] = 1 - normalizedValue;
                    else
                        anchorMax[(int)_axis] = normalizedValue;
                }

                _fillRect.anchorMin = anchorMin;
                _fillRect.anchorMax = anchorMax;
            }

            if (_handleContainerRect != null)
            {
                _tracker.Add(this, _handleRect, DrivenTransformProperties.Anchors);
                Vector2 anchorMin = Vector2.zero;
                Vector2 anchorMax = Vector2.one;
                anchorMin[(int)_axis] = anchorMax[(int)_axis] = (_reverseValue ? (1 - normalizedValue) : normalizedValue);
                _handleRect.anchorMin = anchorMin;
                _handleRect.anchorMax = anchorMax;
            }

            if(_text is not null)
            {
                _text.text = string.IsNullOrWhiteSpace(_format) ? CurvedValue.ToString() : string.Format(_format, CurvedValue);
            }
        }

        private void UpdateDrag(PointerEventData eventData, Camera cam)
        {
            RectTransform clickRect = _handleContainerRect ?? _fillContainerRect;
            if (clickRect != null && clickRect.rect.size[(int)_axis] > 0)
            {
                Vector2 position = Vector2.zero;
                if (! MultipleDisplayUtilities.GetRelativeMousePositionForDrag(eventData, ref position))
                    return;

                Vector2 localCursor;
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(clickRect, position, cam, out localCursor))
                    return;
                localCursor -= clickRect.rect.position;

                float val = Mathf.Clamp01((localCursor - _offset)[(int)_axis] / clickRect.rect.size[(int)_axis]);
                NormalizedValue = (_reverseValue ? 1f - val : val);
            }
        }

        private void UpdateCachedReferences()
        {
            if (_fillRect && _fillRect != (RectTransform)transform)
            {
                _fillTransform = _fillRect.transform;
                _fillImage = _fillRect.GetComponent<Image>();
                if (_fillTransform.parent != null)
                    _fillContainerRect = _fillTransform.parent.GetComponent<RectTransform>();
            }
            else
            {
                _fillRect = null;
                _fillContainerRect = null;
                _fillImage = null;
            }

            if (_handleRect && _handleRect != (RectTransform)transform)
            {
                _handleTransform = _handleRect.transform;
                if (_handleTransform.parent != null)
                    _handleContainerRect = _handleTransform.parent.GetComponent<RectTransform>();
            }
            else
            {
                _handleRect = null;
                _handleContainerRect = null;
            }
        }

        private float ClampValue(float input)
        {
            float newValue = Mathf.Clamp(input, MinValue, MaxValue);
            if (Mathf.Approximately(ValueStep, 0f) == false)
                newValue = Snapping.Snap(newValue, ValueStep);
            return newValue;
        }

        private bool MayDrag(PointerEventData eventData)
        {
            return IsActive() && IsInteractable() && eventData.button == PointerEventData.InputButton.Left;
        }

        protected virtual void Set(float input, bool sendCallback = true)
        {
            // Clamp the input
            float newValue = ClampValue(input);

            // If the stepped value doesn't match the last one, it's time to update
            if (_value == newValue)
                return;

            _value = newValue;
            UpdateVisuals();
            if (sendCallback)
            {
                UISystemProfilerApi.AddMarker("Slider.value", this);
                OnValueChanged.Invoke(CurvedValue);
            }
        }

        [Serializable]
        public class SliderEvent : UnityEvent<float>
        {
            
        }

        public enum Direction
        {
            LeftToRight,
            RightToLeft,
            BottomToTop,
            TopToBottom,
        }

        private enum Axis
        {
            Horizontal = 0,
            Vertical = 1
        }
    }
}