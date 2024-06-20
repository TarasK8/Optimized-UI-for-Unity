using log4net.Util;
using System;
using System.Collections;
using TarasK8.UI;
using TarasK8.UI.Utilites;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Scrollbar")]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class Scrollbar : Selectable, IBeginDragHandler, IDragHandler, IInitializePotentialDragHandler, ICanvasElement
    {
        [Header("Handle")]
        [SerializeField] private RectTransform _handleRect;
        [SerializeField] private Direction _direction = Direction.LeftToRight;
        [SerializeField, Range(0f, 1f)] private float _size = 0.2f;
        [Header("Value")]
        [SerializeField, Range(0f, 1f)] private float _value;
        [SerializeField, Range(0, 11)] private int _numberOfSteps = 0;
        [Space(6)]
        [SerializeField] private ScrollEvent _onValueChanged = new ScrollEvent();

        private RectTransform m_ContainerRect;
        private Vector2 m_Offset = Vector2.zero;
        private DrivenRectTransformTracker m_Tracker;
        private Coroutine m_PointerDownRepeat;
        private bool isPointerDownAndNotDragging = false;
        private bool m_DelayedUpdateVisuals = false;
        private float stepSize => (_numberOfSteps > 1) ? 1f / (_numberOfSteps - 1) : 0.1f;

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
        public float Value
        {
            get
            {
                float val = _value;
                if (_numberOfSteps > 1)
                    val = Mathf.Round(val * (_numberOfSteps - 1)) / (_numberOfSteps - 1);
                return val;
            }
            set
            {
                Set(value);
            }
        }
        public float Size
        {
            get
            {
                return _size;
            }
            set
            {
                if (SetPropertyUtility.SetStruct(ref _size, Mathf.Clamp01(value)))
                    UpdateVisuals();
            }
        }
        public int NumberOfSteps
        {
            get
            {
                return _numberOfSteps;
            }
            set
            {
                if (SetPropertyUtility.SetStruct(ref _numberOfSteps, value))
                {
                    Set(_value);
                    UpdateVisuals();
                }
            }
        }
        public ScrollEvent OnValueChanged
        {
            get => _onValueChanged;
            set => _onValueChanged = value;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            _size = Mathf.Clamp01(_size);

            //This can be invoked before OnEnabled is called. So we shouldn't be accessing other objects, before OnEnable is called.
            if (IsActive())
            {
                UpdateCachedReferences();
                Set(_value, false);
                // Update rects (in next update) since other things might affect them even if value didn't change.
                m_DelayedUpdateVisuals = true;
            }

            if (!UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this) && !Application.isPlaying)
                CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
        }

        protected virtual void Update()
        {
            if (m_DelayedUpdateVisuals)
            {
                m_DelayedUpdateVisuals = false;
                UpdateVisuals();
            }
        }
#endif

        public virtual void Rebuild(CanvasUpdate executing)
        {
#if UNITY_EDITOR
            if (executing == CanvasUpdate.Prelayout)
                OnValueChanged.Invoke(Value);
#endif
        }

        /// <summary>
        /// See ICanvasElement.LayoutComplete.
        /// </summary>
        public virtual void LayoutComplete()
        { }

        /// <summary>
        /// See ICanvasElement.GraphicUpdateComplete.
        /// </summary>
        public virtual void GraphicUpdateComplete()
        { }

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
            m_Tracker.Clear();
            base.OnDisable();
        }

        void UpdateCachedReferences()
        {
            if (_handleRect && _handleRect.parent != null)
                m_ContainerRect = _handleRect.parent.GetComponent<RectTransform>();
            else
                m_ContainerRect = null;
        }

        void Set(float input, bool sendCallback = true)
        {
            float currentValue = _value;

            // bugfix (case 802330) clamp01 input in callee before calling this function, this allows inertia from dragging content to go past extremities without being clamped
            _value = input;

            // If the stepped value doesn't match the last one, it's time to update
            if (currentValue == Value)
                return;

            if (sendCallback)
            {
                UpdateVisuals();
                UISystemProfilerApi.AddMarker("Scrollbar.value", this);
                _onValueChanged.Invoke(Value);
            }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            //This can be invoked before OnEnabled is called. So we shouldn't be accessing other objects, before OnEnable is called.
            if (!IsActive())
                return;

            UpdateVisuals();
        }

        Axis axis { get { return (_direction == Direction.LeftToRight || _direction == Direction.RightToLeft) ? Axis.Horizontal : Axis.Vertical; } }
        bool reverseValue { get { return _direction == Direction.RightToLeft || _direction == Direction.TopToBottom; } }

        // Force-update the scroll bar. Useful if you've changed the properties and want it to update visually.
        private void UpdateVisuals()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UpdateCachedReferences();
#endif
            m_Tracker.Clear();

            if (m_ContainerRect != null)
            {
                m_Tracker.Add(this, _handleRect, DrivenTransformProperties.Anchors);
                Vector2 anchorMin = Vector2.zero;
                Vector2 anchorMax = Vector2.one;

                float movement = Mathf.Clamp01(Value) * (1 - Size);
                if (reverseValue)
                {
                    anchorMin[(int)axis] = 1 - movement - Size;
                    anchorMax[(int)axis] = 1 - movement;
                }
                else
                {
                    anchorMin[(int)axis] = movement;
                    anchorMax[(int)axis] = movement + Size;
                }

                _handleRect.anchorMin = anchorMin;
                _handleRect.anchorMax = anchorMax;
            }
        }

        // Update the scroll bar's position based on the mouse.
        void UpdateDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (m_ContainerRect == null)
                return;

            Vector2 position = Vector2.zero;
            if (!MultipleDisplayUtilities.GetRelativeMousePositionForDrag(eventData, ref position))
                return;

            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ContainerRect, position, eventData.pressEventCamera, out localCursor))
                return;

            Vector2 handleCenterRelativeToContainerCorner = localCursor - m_Offset - m_ContainerRect.rect.position;
            Vector2 handleCorner = handleCenterRelativeToContainerCorner - (_handleRect.rect.size - _handleRect.sizeDelta) * 0.5f;

            float parentSize = axis == 0 ? m_ContainerRect.rect.width : m_ContainerRect.rect.height;
            float remainingSize = parentSize * (1 - Size);
            if (remainingSize <= 0)
                return;

            DoUpdateDrag(handleCorner, remainingSize);
        }

        //this function is testable, it is found using reflection in ScrollbarClamp test
        private void DoUpdateDrag(Vector2 handleCorner, float remainingSize)
        {
            switch (_direction)
            {
                case Direction.LeftToRight:
                    Set(Mathf.Clamp01(handleCorner.x / remainingSize));
                    break;
                case Direction.RightToLeft:
                    Set(Mathf.Clamp01(1f - (handleCorner.x / remainingSize)));
                    break;
                case Direction.BottomToTop:
                    Set(Mathf.Clamp01(handleCorner.y / remainingSize));
                    break;
                case Direction.TopToBottom:
                    Set(Mathf.Clamp01(1f - (handleCorner.y / remainingSize)));
                    break;
            }
        }

        private bool MayDrag(PointerEventData eventData)
        {
            return IsActive() && IsInteractable() && eventData.button == PointerEventData.InputButton.Left;
        }

        /// <summary>
        /// Handling for when the scrollbar value is begin being dragged.
        /// </summary>
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            isPointerDownAndNotDragging = false;

            if (!MayDrag(eventData))
                return;

            if (m_ContainerRect == null)
                return;

            m_Offset = Vector2.zero;
            if (RectTransformUtility.RectangleContainsScreenPoint(_handleRect, eventData.pointerPressRaycast.screenPosition, eventData.enterEventCamera))
            {
                Vector2 localMousePos;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_handleRect, eventData.pointerPressRaycast.screenPosition, eventData.pressEventCamera, out localMousePos))
                    m_Offset = localMousePos - _handleRect.rect.center;
            }
        }

        /// <summary>
        /// Handling for when the scrollbar value is dragged.
        /// </summary>
        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            if (m_ContainerRect != null)
                UpdateDrag(eventData);
        }

        /// <summary>
        /// Event triggered when pointer is pressed down on the scrollbar.
        /// </summary>
        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            base.OnPointerDown(eventData);
            isPointerDownAndNotDragging = true;
            m_PointerDownRepeat = StartCoroutine(ClickRepeat(eventData.pointerPressRaycast.screenPosition, eventData.enterEventCamera));
        }

        protected IEnumerator ClickRepeat(PointerEventData eventData)
        {
            return ClickRepeat(eventData.pointerPressRaycast.screenPosition, eventData.enterEventCamera);
        }

        /// <summary>
        /// Coroutine function for handling continual press during Scrollbar.OnPointerDown.
        /// </summary>
        protected IEnumerator ClickRepeat(Vector2 screenPosition, Camera camera)
        {
            var wait = new WaitForEndOfFrame();
            while (isPointerDownAndNotDragging)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(_handleRect, screenPosition, camera))
                {
                    Vector2 localMousePos;
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_handleRect, screenPosition, camera, out localMousePos))
                    {
                        var axisCoordinate = axis == 0 ? localMousePos.x : localMousePos.y;

                        // modifying value depending on direction, fixes (case 925824)

                        float change = axisCoordinate < 0 ? Size : -Size;
                        Value += reverseValue ? change : -change;
                        Value = Mathf.Clamp01(Value);
                        // Only keep 4 decimals of precision
                        Value = Mathf.Round(Value * 10000f) / 10000f;
                    }
                }
                yield return wait;
            }
            StopCoroutine(m_PointerDownRepeat);
        }

        /// <summary>
        /// Event triggered when pointer is released after pressing on the scrollbar.
        /// </summary>
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            isPointerDownAndNotDragging = false;
        }

        /// <summary>
        /// Handling for movement events.
        /// </summary>
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
                    if (axis == Axis.Horizontal && FindSelectableOnLeft() == null)
                        Set(Mathf.Clamp01(reverseValue ? Value + stepSize : Value - stepSize));
                    else
                        base.OnMove(eventData);
                    break;
                case MoveDirection.Right:
                    if (axis == Axis.Horizontal && FindSelectableOnRight() == null)
                        Set(Mathf.Clamp01(reverseValue ? Value - stepSize : Value + stepSize));
                    else
                        base.OnMove(eventData);
                    break;
                case MoveDirection.Up:
                    if (axis == Axis.Vertical && FindSelectableOnUp() == null)
                        Set(Mathf.Clamp01(reverseValue ? Value - stepSize : Value + stepSize));
                    else
                        base.OnMove(eventData);
                    break;
                case MoveDirection.Down:
                    if (axis == Axis.Vertical && FindSelectableOnDown() == null)
                        Set(Mathf.Clamp01(reverseValue ? Value + stepSize : Value - stepSize));
                    else
                        base.OnMove(eventData);
                    break;
            }
        }

        /// <summary>
        /// Prevents selection if we we move on the Horizontal axis. See Selectable.FindSelectableOnLeft.
        /// </summary>
        public override UnityEngine.UI.Selectable FindSelectableOnLeft()
        {
            if (navigation.mode == Navigation.Mode.Automatic && axis == Axis.Horizontal)
                return null;
            return base.FindSelectableOnLeft();
        }

        /// <summary>
        /// Prevents selection if we we move on the Horizontal axis.  See Selectable.FindSelectableOnRight.
        /// </summary>
        public override UnityEngine.UI.Selectable FindSelectableOnRight()
        {
            if (navigation.mode == Navigation.Mode.Automatic && axis == Axis.Horizontal)
                return null;
            return base.FindSelectableOnRight();
        }

        /// <summary>
        /// Prevents selection if we we move on the Vertical axis. See Selectable.FindSelectableOnUp.
        /// </summary>
        public override UnityEngine.UI.Selectable FindSelectableOnUp()
        {
            if (navigation.mode == Navigation.Mode.Automatic && axis == Axis.Vertical)
                return null;
            return base.FindSelectableOnUp();
        }

        /// <summary>
        /// Prevents selection if we we move on the Vertical axis. See Selectable.FindSelectableOnDown.
        /// </summary>
        public override UnityEngine.UI.Selectable FindSelectableOnDown()
        {
            if (navigation.mode == Navigation.Mode.Automatic && axis == Axis.Vertical)
                return null;
            return base.FindSelectableOnDown();
        }

        /// <summary>
        /// See: IInitializePotentialDragHandler.OnInitializePotentialDrag
        /// </summary>
        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

        /// <summary>
        /// Set the direction of the scrollbar, optionally setting the layout as well.
        /// </summary>
        /// <param name="direction">The direction of the scrollbar.</param>
        /// <param name="includeRectLayouts">Should the layout be flipped together with the direction?</param>
        public void SetDirection(Direction direction, bool includeRectLayouts)
        {
            Axis oldAxis = axis;
            bool oldReverse = reverseValue;
            this.HandleDirection = direction;

            if (!includeRectLayouts)
                return;

            if (axis != oldAxis)
                RectTransformUtility.FlipLayoutAxes(transform as RectTransform, true, true);

            if (reverseValue != oldReverse)
                RectTransformUtility.FlipLayoutOnAxis(transform as RectTransform, (int)axis, true, true);
        }

        [Serializable]
        public class ScrollEvent : UnityEvent<float>
        {

        }

        public enum Direction
        {
            LeftToRight,
            RightToLeft,
            BottomToTop,
            TopToBottom,
        }

        enum Axis
        {
            Horizontal = 0,
            Vertical = 1
        }
    }
}