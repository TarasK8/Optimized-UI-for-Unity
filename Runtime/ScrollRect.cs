using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Scroll Rect")]
    [SelectionBase]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class ScrollRect : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler, ICanvasElement, ILayoutElement, ILayoutGroup
    {
        public enum ClampindType
        {
            Unrestricted,
            Elastic,
            Clamped,
            Scaling,
        }

        public enum ScrollbarVisibility
        {
            Permanent,
            AutoHide,
            AutoHideAndExpandViewport,
        }

        [Serializable]
        public class ScrollRectEvent : UnityEvent<Vector2> { }

        [SerializeField] private RectTransform _content;
        [SerializeField] private bool _horizontal = true;
        [SerializeField] private bool _vertical = true;
        [SerializeField] private ClampindType _movementType = ClampindType.Elastic;
        [SerializeField] private float _elasticity = 0.1f;
        [SerializeField] private bool _inertia = true;
        [SerializeField] private float _decelerationRate = 0.135f; // Only used when inertia is enabled
        [SerializeField] private float _scrollSensitivity = 1.0f;
        [SerializeField] private RectTransform _viewport;
        [SerializeField] private Scrollbar _horizontalScrollbar;
        [SerializeField] private Scrollbar _verticalScrollbar;
        [SerializeField] private ScrollbarVisibility _horizontalScrollbarVisibility;
        [SerializeField] private ScrollbarVisibility _verticalScrollbarVisibility;
        [SerializeField] private float _horizontalScrollbarSpacing;
        [SerializeField] private float _verticalScrollbarSpacing;
        [SerializeField] private ScrollRectEvent _onValueChanged = new ScrollRectEvent();

        public RectTransform Content { get { return _content; } set { _content = value; } }
        public bool Horizontal { get { return _horizontal; } set { _horizontal = value; } }
        public bool Vertical { get { return _vertical; } set { _vertical = value; } }
        public ClampindType movementType { get { return _movementType; } set { _movementType = value; } }
        public float Elasticity { get { return _elasticity; } set { _elasticity = value; } }
        public bool Inertia { get { return _inertia; } set { _inertia = value; } }
        public float DecelerationRate { get { return _decelerationRate; } set { _decelerationRate = value; } }
        public float ScrollSensitivity { get { return _scrollSensitivity; } set { _scrollSensitivity = value; } }
        public RectTransform Viewport { get { return _viewport; } set { _viewport = value; SetDirtyCaching(); } }
        public Scrollbar HorizontalScrollbar
        {
            get
            {
                return _horizontalScrollbar;
            }
            set
            {
                if (_horizontalScrollbar)
                    _horizontalScrollbar.OnValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
                _horizontalScrollbar = value;
                if (_horizontal && _horizontalScrollbar)
                    _horizontalScrollbar.OnValueChanged.AddListener(SetHorizontalNormalizedPosition);
                SetDirtyCaching();
            }
        }
        public Scrollbar VerticalScrollbar
        {
            get
            {
                return _verticalScrollbar;
            }
            set
            {
                if (_verticalScrollbar)
                    _verticalScrollbar.OnValueChanged.RemoveListener(SetVerticalNormalizedPosition);
                _verticalScrollbar = value;
                if (_vertical && _verticalScrollbar)
                    _verticalScrollbar.OnValueChanged.AddListener(SetVerticalNormalizedPosition);
                SetDirtyCaching();
            }
        }
        public ScrollbarVisibility HorizontalScrollbarVisibility { get { return _horizontalScrollbarVisibility; } set { _horizontalScrollbarVisibility = value; SetDirtyCaching(); } }
        public ScrollbarVisibility VerticalScrollbarVisibility { get { return _verticalScrollbarVisibility; } set { _verticalScrollbarVisibility = value; SetDirtyCaching(); } }
        public float HorizontalScrollbarSpacing { get { return _horizontalScrollbarSpacing; } set { _horizontalScrollbarSpacing = value; SetDirty(); } }
        public float VerticalScrollbarSpacing { get { return _verticalScrollbarSpacing; } set { _verticalScrollbarSpacing = value; SetDirty(); } }
        public ScrollRectEvent OnValueChanged { get { return _onValueChanged; } set { _onValueChanged = value; } }
        public Vector2 Velocity { get { return _velocity; } set { _velocity = value; } }

        // The offset from handle position to mouse down position
        private Vector2 _pointerStartLocalCursor = Vector2.zero;
        protected Vector2 _contentStartPosition = Vector2.zero;
        private RectTransform _viewRect;
        protected RectTransform viewRect
        {
            get
            {
                if (_viewRect == null)
                    _viewRect = _viewport;
                if (_viewRect == null)
                    _viewRect = (RectTransform)transform;
                return _viewRect;
            }
        }
        protected Bounds _contentBounds;
        private Bounds _viewBounds;
        private Vector2 _velocity;
        private bool _dragging;
        private bool _scrolling;
        private Vector2 _prevPosition = Vector2.zero;
        private Bounds _prevContentBounds;
        private Bounds _prevViewBounds;
        private bool _hSliderExpand;
        private bool _vSliderExpand;
        private float _hSliderHeight;
        private float _vSliderWidth;
        [NonSerialized] private bool _hasRebuiltLayout = false;
        [NonSerialized] private RectTransform _rect;
        private RectTransform _horizontalScrollbarRect;
        private RectTransform _verticalScrollbarRect;
        private RectTransform _rectTransform
        {
            get
            {
                if (_rect == null)
                    _rect = GetComponent<RectTransform>();
                return _rect;
            }
        }
        // field is never assigned warning
#pragma warning disable 649
        private DrivenRectTransformTracker _tracker;
#pragma warning restore 649

        public virtual void Rebuild(CanvasUpdate executing)
        {
            if (executing == CanvasUpdate.Prelayout)
            {
                UpdateCachedData();
            }

            if (executing == CanvasUpdate.PostLayout)
            {
                UpdateBounds();
                UpdateScrollbars(Vector2.zero);
                UpdatePrevData();

                _hasRebuiltLayout = true;
            }
        }

        public virtual void LayoutComplete()
        { }

        public virtual void GraphicUpdateComplete()
        { }

        void UpdateCachedData()
        {
            Transform transform = this.transform;
            _horizontalScrollbarRect = _horizontalScrollbar == null ? null : _horizontalScrollbar.transform as RectTransform;
            _verticalScrollbarRect = _verticalScrollbar == null ? null : _verticalScrollbar.transform as RectTransform;

            // These are true if either the elements are children, or they don't exist at all.
            bool viewIsChild = (viewRect.parent == transform);
            bool hScrollbarIsChild = (!_horizontalScrollbarRect || _horizontalScrollbarRect.parent == transform);
            bool vScrollbarIsChild = (!_verticalScrollbarRect || _verticalScrollbarRect.parent == transform);
            bool allAreChildren = (viewIsChild && hScrollbarIsChild && vScrollbarIsChild);

            _hSliderExpand = allAreChildren && _horizontalScrollbarRect && HorizontalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
            _vSliderExpand = allAreChildren && _verticalScrollbarRect && VerticalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
            _hSliderHeight = (_horizontalScrollbarRect == null ? 0 : _horizontalScrollbarRect.rect.height);
            _vSliderWidth = (_verticalScrollbarRect == null ? 0 : _verticalScrollbarRect.rect.width);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (_horizontal && _horizontalScrollbar)
                _horizontalScrollbar.OnValueChanged.AddListener(SetHorizontalNormalizedPosition);
            if (_vertical && _verticalScrollbar)
                _verticalScrollbar.OnValueChanged.AddListener(SetVerticalNormalizedPosition);

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            SetDirty();
        }

        protected override void OnDisable()
        {
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            if (_horizontalScrollbar)
                _horizontalScrollbar.OnValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
            if (_verticalScrollbar)
                _verticalScrollbar.OnValueChanged.RemoveListener(SetVerticalNormalizedPosition);

            _dragging = false;
            _scrolling = false;
            _hasRebuiltLayout = false;
            _tracker.Clear();
            _velocity = Vector2.zero;
            LayoutRebuilder.MarkLayoutForRebuild(_rectTransform);
            base.OnDisable();
        }

        public override bool IsActive()
        {
            return base.IsActive() && _content != null;
        }

        private void EnsureLayoutHasRebuilt()
        {
            if (!_hasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
                Canvas.ForceUpdateCanvases();
        }

        public virtual void StopMovement()
        {
            _velocity = Vector2.zero;
        }

        public virtual void OnScroll(PointerEventData data)
        {
            if (!IsActive())
                return;

            EnsureLayoutHasRebuilt();
            UpdateBounds();

            Vector2 delta = data.scrollDelta;
            // Down is positive for scroll events, while in UI system up is positive.
            delta.y *= -1;
            if (Vertical && !Horizontal)
            {
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    delta.y = delta.x;
                delta.x = 0;
            }
            if (Horizontal && !Vertical)
            {
                if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                    delta.x = delta.y;
                delta.y = 0;
            }

            if (data.IsScrolling())
                _scrolling = true;

            Vector2 position = _content.anchoredPosition;
            position += delta * _scrollSensitivity;
            if (_movementType == ClampindType.Clamped)
                position += CalculateOffset(position - _content.anchoredPosition);

            SetContentAnchoredPosition(position);
            UpdateBounds();
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _velocity = Vector2.zero;
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            UpdateBounds();

            _pointerStartLocalCursor = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out _pointerStartLocalCursor);
            _contentStartPosition = _content.anchoredPosition;
            _dragging = true;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _dragging = false;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!_dragging)
                return;

            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out localCursor))
                return;

            UpdateBounds();

            var pointerDelta = localCursor - _pointerStartLocalCursor;
            Vector2 position = _contentStartPosition + pointerDelta;

            // Offset to get content into place in the view.
            Vector2 offset = CalculateOffset(position - _content.anchoredPosition);
            position += offset;
            if (_movementType == ClampindType.Elastic)
            {
                if (offset.x != 0)
                    position.x = position.x - RubberDelta(offset.x, _viewBounds.size.x);
                if (offset.y != 0)
                    position.y = position.y - RubberDelta(offset.y, _viewBounds.size.y);
            }

            SetContentAnchoredPosition(position);
        }

        protected virtual void SetContentAnchoredPosition(Vector2 position)
        {
            if (!_horizontal)
                position.x = _content.anchoredPosition.x;
            if (!_vertical)
                position.y = _content.anchoredPosition.y;

            if (position != _content.anchoredPosition)
            {
                _content.anchoredPosition = position;
                UpdateBounds();
            }
        }

        protected virtual void LateUpdate()
        {
            if (_content == null)
                return;

            EnsureLayoutHasRebuilt();
            UpdateBounds();
            float deltaTime = Time.unscaledDeltaTime;
            Vector2 offset = CalculateOffset(Vector2.zero);

            // Skip processing if deltaTime is invalid (0 or less) as it will cause inaccurate velocity calculations and a divide by zero error.
            if (deltaTime > 0.0f)
            {
                if (!_dragging && (offset != Vector2.zero || _velocity != Vector2.zero))
                {
                    Vector2 position = _content.anchoredPosition;
                    for (int axis = 0; axis < 2; axis++)
                    {
                        // Apply spring physics if movement is elastic and content has an offset from the view.
                        if (_movementType == ClampindType.Elastic && offset[axis] != 0)
                        {
                            float speed = _velocity[axis];
                            float smoothTime = _elasticity;
                            if (_scrolling)
                                smoothTime *= 3.0f;
                            position[axis] = Mathf.SmoothDamp(_content.anchoredPosition[axis], _content.anchoredPosition[axis] + offset[axis], ref speed, smoothTime, Mathf.Infinity, deltaTime);
                            if (Mathf.Abs(speed) < 1)
                                speed = 0;
                            _velocity[axis] = speed;
                        }
                        // Else move content according to velocity with deceleration applied.
                        else if (_inertia)
                        {
                            _velocity[axis] *= Mathf.Pow(_decelerationRate, deltaTime);
                            if (Mathf.Abs(_velocity[axis]) < 1)
                                _velocity[axis] = 0;
                            position[axis] += _velocity[axis] * deltaTime;
                        }
                        // If we have neither elaticity or friction, there shouldn't be any velocity.
                        else
                        {
                            _velocity[axis] = 0;
                        }
                    }

                    if (_movementType == ClampindType.Clamped)
                    {
                        offset = CalculateOffset(position - _content.anchoredPosition);
                        position += offset;
                    }

                    SetContentAnchoredPosition(position);
                }

                if (_dragging && _inertia)
                {
                    Vector3 newVelocity = (_content.anchoredPosition - _prevPosition) / deltaTime;
                    _velocity = Vector3.Lerp(_velocity, newVelocity, deltaTime * 10);
                }
            }

            if (_viewBounds != _prevViewBounds || _contentBounds != _prevContentBounds || _content.anchoredPosition != _prevPosition)
            {
                UpdateScrollbars(offset);
                UISystemProfilerApi.AddMarker("ScrollRect.value", this);
                _onValueChanged.Invoke(normalizedPosition);
                UpdatePrevData();
            }
            UpdateScrollbarVisibility();
            _scrolling = false;
        }

        protected void UpdatePrevData()
        {
            if (_content == null)
                _prevPosition = Vector2.zero;
            else
                _prevPosition = _content.anchoredPosition;
            _prevViewBounds = _viewBounds;
            _prevContentBounds = _contentBounds;
        }

        private void UpdateScrollbars(Vector2 offset)
        {
            if (_horizontalScrollbar)
            {
                if (_contentBounds.size.x > 0)
                    _horizontalScrollbar.Size = Mathf.Clamp01((_viewBounds.size.x - Mathf.Abs(offset.x)) / _contentBounds.size.x);
                else
                    _horizontalScrollbar.Size = 1;

                _horizontalScrollbar.Value = horizontalNormalizedPosition;
            }

            if (_verticalScrollbar)
            {
                if (_contentBounds.size.y > 0)
                    _verticalScrollbar.Size = Mathf.Clamp01((_viewBounds.size.y - Mathf.Abs(offset.y)) / _contentBounds.size.y);
                else
                    _verticalScrollbar.Size = 1;

                _verticalScrollbar.Value = verticalNormalizedPosition;
            }
        }

        public Vector2 normalizedPosition
        {
            get
            {
                return new Vector2(horizontalNormalizedPosition, verticalNormalizedPosition);
            }
            set
            {
                SetNormalizedPosition(value.x, 0);
                SetNormalizedPosition(value.y, 1);
            }
        }

        public float horizontalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                if ((_contentBounds.size.x <= _viewBounds.size.x) || Mathf.Approximately(_contentBounds.size.x, _viewBounds.size.x))
                    return (_viewBounds.min.x > _contentBounds.min.x) ? 1 : 0;
                return (_viewBounds.min.x - _contentBounds.min.x) / (_contentBounds.size.x - _viewBounds.size.x);
            }
            set
            {
                SetNormalizedPosition(value, 0);
            }
        }

        public float verticalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                if ((_contentBounds.size.y <= _viewBounds.size.y) || Mathf.Approximately(_contentBounds.size.y, _viewBounds.size.y))
                    return (_viewBounds.min.y > _contentBounds.min.y) ? 1 : 0;

                return (_viewBounds.min.y - _contentBounds.min.y) / (_contentBounds.size.y - _viewBounds.size.y);
            }
            set
            {
                SetNormalizedPosition(value, 1);
            }
        }

        private void SetHorizontalNormalizedPosition(float value) { SetNormalizedPosition(value, 0); }
        private void SetVerticalNormalizedPosition(float value) { SetNormalizedPosition(value, 1); }

        protected virtual void SetNormalizedPosition(float value, int axis)
        {
            EnsureLayoutHasRebuilt();
            UpdateBounds();
            // How much the content is larger than the view.
            float hiddenLength = _contentBounds.size[axis] - _viewBounds.size[axis];
            // Where the position of the lower left corner of the content bounds should be, in the space of the view.
            float contentBoundsMinPosition = _viewBounds.min[axis] - value * hiddenLength;
            // The new content localPosition, in the space of the view.
            float newAnchoredPosition = _content.anchoredPosition[axis] + contentBoundsMinPosition - _contentBounds.min[axis];

            Vector3 anchoredPosition = _content.anchoredPosition;
            if (Mathf.Abs(anchoredPosition[axis] - newAnchoredPosition) > 0.01f)
            {
                anchoredPosition[axis] = newAnchoredPosition;
                _content.anchoredPosition = anchoredPosition;
                _velocity[axis] = 0;
                UpdateBounds();
            }
        }

        private static float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        private bool hScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return _contentBounds.size.x > _viewBounds.size.x + 0.01f;
                return true;
            }
        }
        private bool vScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return _contentBounds.size.y > _viewBounds.size.y + 0.01f;
                return true;
            }
        }

        /// <summary>Called by the layout system.</summary>
        public virtual void CalculateLayoutInputHorizontal() { }

        /// <summary>Called by the layout system.</summary>
        public virtual void CalculateLayoutInputVertical() { }

        /// <summary>Called by the layout system.</summary>
        public virtual float minWidth { get { return -1; } }

        /// <summary>Called by the layout system.</summary>
        public virtual float preferredWidth { get { return -1; } }

        /// <summary>Called by the layout system.</summary>
        public virtual float flexibleWidth { get { return -1; } }

        /// <summary>Called by the layout system.</summary>
        public virtual float minHeight { get { return -1; } }

        /// <summary>Called by the layout system.</summary>
        public virtual float preferredHeight { get { return -1; } }

        /// <summary>Called by the layout system.</summary>
        public virtual float flexibleHeight { get { return -1; } }

        /// <summary>Called by the layout system.</summary>
        public virtual int layoutPriority { get { return -1; } }

        /// <summary>Called by the layout system.</summary>
        public virtual void SetLayoutHorizontal()
        {
            _tracker.Clear();
            UpdateCachedData();

            if (_hSliderExpand || _vSliderExpand)
            {
                _tracker.Add(this, viewRect,
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.SizeDelta |
                    DrivenTransformProperties.AnchoredPosition);

                // Make view full size to see if content fits.
                viewRect.anchorMin = Vector2.zero;
                viewRect.anchorMax = Vector2.one;
                viewRect.sizeDelta = Vector2.zero;
                viewRect.anchoredPosition = Vector2.zero;

                // Recalculate content layout with this size to see if it fits when there are no scrollbars.
                LayoutRebuilder.ForceRebuildLayoutImmediate(Content);
                _viewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                _contentBounds = GetBounds();
            }

            // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
            if (_vSliderExpand && vScrollingNeeded)
            {
                viewRect.sizeDelta = new Vector2(-(_vSliderWidth + _verticalScrollbarSpacing), viewRect.sizeDelta.y);

                // Recalculate content layout with this size to see if it fits vertically
                // when there is a vertical scrollbar (which may reflowed the content to make it taller).
                LayoutRebuilder.ForceRebuildLayoutImmediate(Content);
                _viewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                _contentBounds = GetBounds();
            }

            // If it doesn't fit horizontally, enable horizontal scrollbar and shrink view vertically to make room for it.
            if (_hSliderExpand && hScrollingNeeded)
            {
                viewRect.sizeDelta = new Vector2(viewRect.sizeDelta.x, -(_hSliderHeight + _horizontalScrollbarSpacing));
                _viewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                _contentBounds = GetBounds();
            }

            // If the vertical slider didn't kick in the first time, and the horizontal one did,
            // we need to check again if the vertical slider now needs to kick in.
            // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
            if (_vSliderExpand && vScrollingNeeded && viewRect.sizeDelta.x == 0 && viewRect.sizeDelta.y < 0)
            {
                viewRect.sizeDelta = new Vector2(-(_vSliderWidth + _verticalScrollbarSpacing), viewRect.sizeDelta.y);
            }
        }

        /// <summary>Called by the layout system.</summary>
        public virtual void SetLayoutVertical()
        {
            UpdateScrollbarLayout();
            _viewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            _contentBounds = GetBounds();
        }

        void UpdateScrollbarVisibility()
        {
            UpdateOneScrollbarVisibility(vScrollingNeeded, _vertical, _verticalScrollbarVisibility, _verticalScrollbar);
            UpdateOneScrollbarVisibility(hScrollingNeeded, _horizontal, _horizontalScrollbarVisibility, _horizontalScrollbar);
        }

        private static void UpdateOneScrollbarVisibility(bool xScrollingNeeded, bool xAxisEnabled, ScrollbarVisibility scrollbarVisibility, Scrollbar scrollbar)
        {
            if (scrollbar)
            {
                if (scrollbarVisibility == ScrollbarVisibility.Permanent)
                {
                    if (scrollbar.gameObject.activeSelf != xAxisEnabled)
                        scrollbar.gameObject.SetActive(xAxisEnabled);
                }
                else
                {
                    if (scrollbar.gameObject.activeSelf != xScrollingNeeded)
                        scrollbar.gameObject.SetActive(xScrollingNeeded);
                }
            }
        }

        private void UpdateScrollbarLayout()
        {
            if (_vSliderExpand && _horizontalScrollbar)
            {
                _tracker.Add(this, _horizontalScrollbarRect,
                    DrivenTransformProperties.AnchorMinX |
                    DrivenTransformProperties.AnchorMaxX |
                    DrivenTransformProperties.SizeDeltaX |
                    DrivenTransformProperties.AnchoredPositionX);
                _horizontalScrollbarRect.anchorMin = new Vector2(0, _horizontalScrollbarRect.anchorMin.y);
                _horizontalScrollbarRect.anchorMax = new Vector2(1, _horizontalScrollbarRect.anchorMax.y);
                _horizontalScrollbarRect.anchoredPosition = new Vector2(0, _horizontalScrollbarRect.anchoredPosition.y);
                if (vScrollingNeeded)
                    _horizontalScrollbarRect.sizeDelta = new Vector2(-(_vSliderWidth + _verticalScrollbarSpacing), _horizontalScrollbarRect.sizeDelta.y);
                else
                    _horizontalScrollbarRect.sizeDelta = new Vector2(0, _horizontalScrollbarRect.sizeDelta.y);
            }

            if (_hSliderExpand && _verticalScrollbar)
            {
                _tracker.Add(this, _verticalScrollbarRect,
                    DrivenTransformProperties.AnchorMinY |
                    DrivenTransformProperties.AnchorMaxY |
                    DrivenTransformProperties.SizeDeltaY |
                    DrivenTransformProperties.AnchoredPositionY);
                _verticalScrollbarRect.anchorMin = new Vector2(_verticalScrollbarRect.anchorMin.x, 0);
                _verticalScrollbarRect.anchorMax = new Vector2(_verticalScrollbarRect.anchorMax.x, 1);
                _verticalScrollbarRect.anchoredPosition = new Vector2(_verticalScrollbarRect.anchoredPosition.x, 0);
                if (hScrollingNeeded)
                    _verticalScrollbarRect.sizeDelta = new Vector2(_verticalScrollbarRect.sizeDelta.x, -(_hSliderHeight + _horizontalScrollbarSpacing));
                else
                    _verticalScrollbarRect.sizeDelta = new Vector2(_verticalScrollbarRect.sizeDelta.x, 0);
            }
        }

        /// <summary> Calculate the bounds the ScrollRect should be using. </summary>
        protected void UpdateBounds()
        {
            _viewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            _contentBounds = GetBounds();

            if (_content == null)
                return;

            Vector3 contentSize = _contentBounds.size;
            Vector3 contentPos = _contentBounds.center;
            var contentPivot = _content.pivot;
            AdjustBounds(ref _viewBounds, ref contentPivot, ref contentSize, ref contentPos);
            _contentBounds.size = contentSize;
            _contentBounds.center = contentPos;

            if (movementType == ClampindType.Clamped)
            {
                // Adjust content so that content bounds bottom (right side) is never higher (to the left) than the view bounds bottom (right side).
                // top (left side) is never lower (to the right) than the view bounds top (left side).
                // All this can happen if content has shrunk.
                // This works because content size is at least as big as view size (because of the call to InternalUpdateBounds above).
                Vector2 delta = Vector2.zero;
                if (_viewBounds.max.x > _contentBounds.max.x)
                {
                    delta.x = Math.Min(_viewBounds.min.x - _contentBounds.min.x, _viewBounds.max.x - _contentBounds.max.x);
                }
                else if (_viewBounds.min.x < _contentBounds.min.x)
                {
                    delta.x = Math.Max(_viewBounds.min.x - _contentBounds.min.x, _viewBounds.max.x - _contentBounds.max.x);
                }

                if (_viewBounds.min.y < _contentBounds.min.y)
                {
                    delta.y = Math.Max(_viewBounds.min.y - _contentBounds.min.y, _viewBounds.max.y - _contentBounds.max.y);
                }
                else if (_viewBounds.max.y > _contentBounds.max.y)
                {
                    delta.y = Math.Min(_viewBounds.min.y - _contentBounds.min.y, _viewBounds.max.y - _contentBounds.max.y);
                }
                if (delta.sqrMagnitude > float.Epsilon)
                {
                    contentPos = _content.anchoredPosition + delta;
                    if (!_horizontal)
                        contentPos.x = _content.anchoredPosition.x;
                    if (!_vertical)
                        contentPos.y = _content.anchoredPosition.y;
                    AdjustBounds(ref _viewBounds, ref contentPivot, ref contentSize, ref contentPos);
                }
            }
        }

        internal static void AdjustBounds(ref Bounds viewBounds, ref Vector2 contentPivot, ref Vector3 contentSize, ref Vector3 contentPos)
        {
            // Make sure content bounds are at least as large as view by adding padding if not.
            // One might think at first that if the content is smaller than the view, scrolling should be allowed.
            // However, that's not how scroll views normally work.
            // Scrolling is *only* possible when content is *larger* than view.
            // We use the pivot of the content rect to decide in which directions the content bounds should be expanded.
            // E.g. if pivot is at top, bounds are expanded downwards.
            // This also works nicely when ContentSizeFitter is used on the content.
            Vector3 excess = viewBounds.size - contentSize;
            if (excess.x > 0)
            {
                contentPos.x -= excess.x * (contentPivot.x - 0.5f);
                contentSize.x = viewBounds.size.x;
            }
            if (excess.y > 0)
            {
                contentPos.y -= excess.y * (contentPivot.y - 0.5f);
                contentSize.y = viewBounds.size.y;
            }
        }

        private readonly Vector3[] m_Corners = new Vector3[4];
        private Bounds GetBounds()
        {
            if (_content == null)
                return new Bounds();
            _content.GetWorldCorners(m_Corners);
            var viewWorldToLocalMatrix = viewRect.worldToLocalMatrix;
            return InternalGetBounds(m_Corners, ref viewWorldToLocalMatrix);
        }

        internal static Bounds InternalGetBounds(Vector3[] corners, ref Matrix4x4 viewWorldToLocalMatrix)
        {
            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int j = 0; j < 4; j++)
            {
                Vector3 v = viewWorldToLocalMatrix.MultiplyPoint3x4(corners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            var bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
            return bounds;
        }

        private Vector2 CalculateOffset(Vector2 delta)
        {
            return InternalCalculateOffset(ref _viewBounds, ref _contentBounds, _horizontal, _vertical, _movementType, ref delta);
        }

        internal static Vector2 InternalCalculateOffset(ref Bounds viewBounds, ref Bounds contentBounds, bool horizontal, bool vertical, ClampindType movementType, ref Vector2 delta)
        {
            Vector2 offset = Vector2.zero;
            if (movementType == ClampindType.Unrestricted)
                return offset;

            Vector2 min = contentBounds.min;
            Vector2 max = contentBounds.max;

            // min/max offset extracted to check if approximately 0 and avoid recalculating layout every frame (case 1010178)

            if (horizontal)
            {
                min.x += delta.x;
                max.x += delta.x;

                float maxOffset = viewBounds.max.x - max.x;
                float minOffset = viewBounds.min.x - min.x;

                if (minOffset < -0.001f)
                    offset.x = minOffset;
                else if (maxOffset > 0.001f)
                    offset.x = maxOffset;
            }

            if (vertical)
            {
                min.y += delta.y;
                max.y += delta.y;

                float maxOffset = viewBounds.max.y - max.y;
                float minOffset = viewBounds.min.y - min.y;

                if (maxOffset > 0.001f)
                    offset.y = maxOffset;
                else if (minOffset < -0.001f)
                    offset.y = minOffset;
            }

            return offset;
        }

        /// <summary> Override to alter or add to the code that keeps the appearance of the scroll rect synced with its data. </summary>
        protected void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(_rectTransform);
        }

        /// <summary> Override to alter or add to the code that caches data to avoid repeated heavy operations. </summary>
        protected void SetDirtyCaching()
        {
            if (!IsActive())
                return;

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            LayoutRebuilder.MarkLayoutForRebuild(_rectTransform);

            _viewRect = null;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirtyCaching();
        }

#endif
    }
}