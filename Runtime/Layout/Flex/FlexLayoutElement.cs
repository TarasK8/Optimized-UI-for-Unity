using UnityEngine;

namespace TarasK8.UI.Layout
{
    [AddComponentMenu("Optimized UI/Layout/Flex Layout Element")]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class FlexLayoutElement : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _grow = 1f;
        [SerializeField, Min(0f)] private float _shrink = 0f;
        [SerializeField, Min(0f)] private float _basis = 0f;

        [SerializeField, HideInInspector] private RectTransform _transform;
        [SerializeField, HideInInspector] private RectTransform _containerTransform;
        [SerializeField, HideInInspector] private FlexLayoutGroup _container;

        private DrivenRectTransformTracker _tracker;

        public float Grow
        {
            get { return _grow; }
            set { _grow = value; _container.FlexElements(); }
        }
        public float Shrink
        {
            get { return _shrink; }
            set { _shrink = value; _container.FlexElements(); }
        }
        public float Basis
        {
            get { return _basis; }
            set { _basis = value; _container.FlexElements(); }
        }

        private void OnValidate()
        {
            _transform = transform as RectTransform;
            _containerTransform = transform.parent as RectTransform;
            _container = _containerTransform.GetComponent<FlexLayoutGroup>();
        }

        private void OnEnable()
        {
            _transform = transform as RectTransform;
            UpdateTracker();
        }

        private void OnDisable()
        {
            _tracker.Clear();
        }

        public void FlexElement(
            float totalSize,
            float totalGrow,
            float totalShrink,
            float totalBasis,
            FlexLayoutGroup.Direction direction,
            RectOffset padding,
            ref float nextPosition,
            bool forceSize)
        {
            float size = CalculateFlexSize(_grow, _shrink, _basis, totalSize, totalGrow, totalShrink, totalBasis);
            float position = size / 2f;

            Vector2 containerSize = _containerTransform.sizeDelta;

            _transform.sizeDelta = CalculateSizeDelta(direction, size, padding, containerSize, forceSize);
            _transform.anchoredPosition = CalculatePosition(direction, position, nextPosition, padding, containerSize, forceSize);
            UpdateAnchors();
            nextPosition += size;
        }

        public float CalculateFlexSize(float grow, float shrink, float basis, float totalSize, float totalGrow, float totalShrink, float totalBasis)
        {
            float freeSpace = totalSize - totalBasis;

            if (freeSpace > 0)
            {
                return basis + (grow / totalGrow) * freeSpace;
            }
            else
            {
                return basis + (shrink / totalShrink) * freeSpace;
            }
        }

        public void UpdateTracker()
        {
            if (_container is null) return;

            _tracker.Clear();
            _tracker = new DrivenRectTransformTracker();

            if(_container.ForceSize)
            {
                _tracker.Add(this, _transform,
                    DrivenTransformProperties.SizeDelta |
                    DrivenTransformProperties.AnchoredPosition |
                    DrivenTransformProperties.Anchors);
            }
            else
            {
                if(_container.FlexDirection == FlexLayoutGroup.Direction.Row)
                {
                    _tracker.Add(this, _transform,
                        DrivenTransformProperties.SizeDeltaX |
                        DrivenTransformProperties.AnchoredPositionX |
                        DrivenTransformProperties.AnchorMaxX |
                        DrivenTransformProperties.AnchorMinX);
                }
                else
                {
                    _tracker.Add(this, _transform,
                        DrivenTransformProperties.SizeDeltaY |
                        DrivenTransformProperties.AnchoredPositionY |
                        DrivenTransformProperties.AnchorMaxY |
                        DrivenTransformProperties.AnchorMinY);
                }
            }
        }

        private void UpdateAnchors()
        {
            if (_container is null) return;

            if (_container.ForceSize)
            {
                _transform.anchorMax = Vector2.zero;
                _transform.anchorMin = Vector2.zero;
            }
            else
            {
                Vector2 anchorMax = _transform.anchorMax;
                Vector2 anchorMin = _transform.anchorMin;

                if (_container.FlexDirection == FlexLayoutGroup.Direction.Row)
                {
                    anchorMax.x = 0f;
                    anchorMin.x = 0f;
                }
                else
                {
                    anchorMax.y = 0f;
                    anchorMin.y = 0f;
                }

                _transform.anchorMax = anchorMax;
                _transform.anchorMin = anchorMin;
            }
        }

        private Vector2 CalculateSizeDelta(FlexLayoutGroup.Direction direction, float size, RectOffset padding, Vector2 containerSize, bool forceSize)
        {
            float x, y;
            if (direction == FlexLayoutGroup.Direction.Row)
            {
                x = size;
                if(forceSize)
                    y = containerSize.y - padding.vertical;
                else
                    y = _transform.sizeDelta.y;
            }
            else
            {
                y = size;
                if(forceSize)
                    x = containerSize.x - padding.horizontal;
                else
                    x = _transform.sizeDelta.x;
            }

            return new Vector2(x, y);
        }

        private Vector2 CalculatePosition(FlexLayoutGroup.Direction direction, float position, float nextPosition, RectOffset padding, Vector2 containerSize, bool forceSize)
        {
            float x, y;
            if(direction == FlexLayoutGroup.Direction.Row)
            {
                x = position + nextPosition;
                if (forceSize)
                    y = containerSize.y / 2f - (padding.top / 2f) + (padding.bottom / 2f);
                else
                    y = _transform.anchoredPosition.y;
            }
            else
            {
                y = position + nextPosition;
                if (forceSize)
                    x = containerSize.x / 2f - (padding.left / 2f) + (padding.right / 2f);
                else
                    x = _transform.anchoredPosition.x;
            }

            return new Vector2(x, y);
        }
    }
}