using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TarasK8.UI.Layout
{
    [AddComponentMenu("Optimized UI/Layout/Radial Layout Element")]
    [RequireComponent(typeof(RadialBarSegment))]
    [ExecuteAlways]
    public class RadialLayoutElement : MonoBehaviour
    {
        [SerializeField, SerializeReference] private IFlexElement _flexElement;
        [SerializeField, Min(0f)] private float _grow = 1f;
        [SerializeField, Min(0f)] private float _shrink = 0f;
        [SerializeField, Min(0f)] private float _basis = 0f;
        [SerializeField] private bool _autoUpdate = true;
        
        [SerializeField, HideInInspector] private RadialBarSegment _radialSegment;
        [SerializeField, HideInInspector] private RadialLayoutGroup _container;
        
        public RadialBarSegment AttachedBarSegment => _radialSegment;
        public RadialLayoutGroup AttachedContainer => _container;
        
        public float Grow
        {
            get { return _grow; }
            set { _grow = value; if(_autoUpdate) UpdateContainer(); }
        }
        public float Shrink
        {
            get { return _shrink; }
            set { _shrink = value; if(_autoUpdate) UpdateContainer(); }
        }
        public float Basis
        {
            get { return _basis; }
            set { _basis = value; if(_autoUpdate) UpdateContainer(); }
        }

        public bool AutoUpdate
        {
            get { return _autoUpdate; }
            set
            {
                if(_autoUpdate != value && value == true)
                    UpdateContainer();
                _autoUpdate = value;
            }
        }

#if UNITY_EDITOR
        [SerializeField, HideInInspector] private bool _editorUpdate = false;
        private void OnValidate()
        {
            UpdateRequirements();
            
            _editorUpdate = true;
        }

        private void Update()
        {
            if(_editorUpdate == false)
                return;

            _editorUpdate = false;
            UpdateContainer();
        }
#endif

        private void Awake()
        {
            UpdateRequirements();
        }

        private void OnEnable()
        {
            UpdateContainer();
        }

        private void OnDisable()
        {
            UpdateContainer();
        }

        public void FlexElement(float freeSpace, float totalGrow, float totalShrink, float nextPosition, out float size)
        {
            size = FlexBoxUtility.CalculateElementSize(_grow, _shrink, _basis, totalGrow, totalShrink, freeSpace);

            _radialSegment.SetStartEndAngles(nextPosition, nextPosition + size);
        }

        public void FlexElement2(float position, float size)
        {
            _radialSegment.SetStartEndAngles(position, position + size);
        }

        public void UpdateContainer()
        {
            if(_container)
                _container.FlexElements();
        }

        private void UpdateRequirements()
        {
            if(!_radialSegment)
                _radialSegment = GetComponent<RadialBarSegment>();
            if (!_container && transform.parent)
                _container = transform.parent.GetComponent<RadialLayoutGroup>();
        }
    }
}
