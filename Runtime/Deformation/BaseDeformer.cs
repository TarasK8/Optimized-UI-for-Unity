using System;
using UnityEngine;

namespace TarasK8.UI.Deformation
{
    public abstract class BaseDeformer : MonoBehaviour
    {
        [SerializeField, HideInInspector] private DeformableGraphic _deformableGraphic;
        
        public DeformableGraphic AttachedDeformableGraphic => _deformableGraphic;

        protected virtual void Awake()
        {
            UpdateReferences();
        }

        protected virtual void OnValidate()
        {
            UpdateReferences();
            
            if(_deformableGraphic != null)
                _deformableGraphic.Rebuild();
        }

        public abstract void BeginDeform();
        public abstract Vector2 DeformPoint(float xTime, float yTime);

        private void UpdateReferences()
        {
            if(_deformableGraphic == null)
                _deformableGraphic = GetComponent<DeformableGraphic>();
        }
    }
}
