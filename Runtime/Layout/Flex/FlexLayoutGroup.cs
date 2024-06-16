using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TarasK8.UI.Layout
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class FlexLayoutGroup : MonoBehaviour
    {
        [SerializeField] private Direction _direction = Direction.Row;
        [SerializeField] private bool _reverse = false;
        [SerializeField] private bool _forceSize = false;
        [SerializeField] private float _spacing = 0f;
        [SerializeField] private RectOffset _padding;

        [SerializeField, HideInInspector] private RectTransform _transform;
        [SerializeField, HideInInspector] private FlexLayoutElement[] _elements;

        public bool ForceSize => _forceSize;
        public Direction FlexDirection => _direction;

        private void OnValidate()
        {
            _transform = transform as RectTransform;
            FindChilds();
            foreach (var element in _elements)
            {
                element.UpdateTracker();
            }
        }

        private void OnTransformChildrenChanged()
        {
            FindChilds();
            FlexElements();
        }

        private void OnRectTransformDimensionsChange()
        {
            FlexElements();
        }

        [ContextMenu("Flex Elements")]
        public void FlexElements()
        {
            float padding = _direction == Direction.Row ? _padding.horizontal : _padding.vertical;
            float widthAxis = _direction == Direction.Row ? _transform.rect.width : _transform.rect.height;
            float totalWidth = widthAxis - _spacing * (_elements.Length - 1) - padding;
            float totalGrow = 0f, totalShrink = 0f, totalBasis = 0f;
            foreach (var child in _elements)
            {
                totalGrow += child.Grow;
                totalShrink += child.Shrink;
                totalBasis += child.Basis;
            }

            float nextPosition = 0f;
            nextPosition += _direction == Direction.Row ? _padding.left : _padding.top;
            for (int i = 0; i < _elements.Length; i++)
            {
                int index = _reverse ? _elements.Length - i - 1: i;
                _elements[index].FlexElement(totalWidth, totalGrow, totalShrink, totalBasis, _direction, _padding, ref nextPosition, _forceSize);
                nextPosition += _spacing;
            }
        }

        private void FindChilds()
        {
            _elements = GetComponentsInChilden().ToArray();
        }

        private IEnumerable<FlexLayoutElement> GetComponentsInChilden()
        {
            foreach (Transform item in transform)
            {
                if (item.TryGetComponent<FlexLayoutElement>(out var component))
                {
                    yield return component;
                }
            }
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Application.isPlaying) return;

            FlexElements();
        }
#endif

        public enum Direction
        {
            Row,
            Column
        }
    }
}