using System;
using System.Collections.Generic;
using TarasK8.UI.Utilites;
using UnityEngine;
using UnityEngine.Serialization;

namespace TarasK8.UI.Layout
{
    [AddComponentMenu("Optimized UI/Layout/Radial Layout Group")]
    [ExecuteAlways]
    public class RadialLayoutGroup : MonoBehaviour
    {
        [SerializeField] private bool _reverse = false;
        [SerializeField, Range(0f, 360f)] private float _length = 360f;
        [SerializeField] private float _offset = 0f;
        [SerializeField] private float _spacing = 0f;
        [SerializeField] private ProgressBar.Direction _spacingOffsetOrigin;
        [SerializeField] private List<RadialLayoutElement> _elements = new(16);

#if UNITY_EDITOR
        [SerializeField, HideInInspector] private bool _editorUpdate = false;
        
        private void OnValidate()
        {
            _editorUpdate = true;
        }
        
        private void Update()
        {
            if(_editorUpdate == false)
                return;

            _editorUpdate = false;
            FlexElements();
        }
#endif

        [ContextMenu("Find Child Elements")]
        public void FindChildElements()
        {
            transform.GetComponentsInNearestChildren(_elements);
            FlexElements();
        }

        [ContextMenu("Flex Elements")]
        public void FlexElements()
        {
            if (_elements.Count == 0)
                return;
            
            float totalGrow = 0f, totalShrink = 0f, totalBasis = 0f;
            int activeElements = 0;
            foreach (var child in _elements)
            {
                if(!child || child.enabled == false || child.gameObject.activeSelf == false) continue;
                totalGrow += child.Grow;
                totalShrink += child.Shrink;
                totalBasis += child.Basis;
                activeElements++;
            }
            
            float totalWidth = _length - _spacing * activeElements + (IsConnectedCircle() ? 0f : _spacing);
            float freeSpace = totalWidth - totalBasis;
            float nextPosition = 0f;
            
            for (int i = 0; i < _elements.Count; i++)
            {
                int index = _reverse ? _elements.Count - i - 1: i;
                var element = _elements[index];
                
                if(!element || element.enabled == false || element.gameObject.activeSelf == false) continue;
                
                element.FlexElement(freeSpace, totalGrow, totalShrink, nextPosition + GetSpacingOffset() + _offset, out float size);
                nextPosition += _spacing + size;
            }
        }

        private float GetSpacingOffset()
        {
            if(IsConnectedCircle() == false)
                return 0f;
            
            return _spacingOffsetOrigin switch
            {
                ProgressBar.Direction.Right => 0f,
                ProgressBar.Direction.Left => _spacing,
                ProgressBar.Direction.Center => _spacing / 2f,
                _ => 0f
            };
        }

        private bool IsConnectedCircle()
        {
            return Mathf.Approximately(_length, 360f);
        }
    }
}
