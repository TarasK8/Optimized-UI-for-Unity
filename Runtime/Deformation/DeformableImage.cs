using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TarasK8.UI.Deformation
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    [AddComponentMenu("UI/Effects/Extensions/Deformable Image")]
    [ExecuteAlways]
    public class DeformableImage : DeformableGraphic
    {
        private readonly Vector2 _cornerPositionRatio = new(0.1f, 0.1f);
        private readonly Vector2 _originalCornerPositionRatio = new(0.1f, 0.1f);

        private Image AttachedImage => (Image)graphic;

        public override void ModifyMesh(VertexHelper vertexHelper)
        {
            //graphic.
            if (!IsActive()) return;

            // Handle additional behavior for specific Image types (e.g., Sliced or Tiled)
            if (IsUpdateRequired && AttachedImage.type == Image.Type.Sliced || AttachedImage.type == Image.Type.Tiled)
            {
                vertexHelper.GetUIVertexStream(CachedVertices);

                //InitializeCornerPositionRatio();
                //ConstrainCornerPositionRatio();

                AdjustVerticesForCornerRatio(CachedVertices);

                vertexHelper.Clear();
                vertexHelper.AddUIVertexTriangleStream(CachedVertices);
            }

            // Apply deformation from the assigned shape handler
            base.ModifyMesh(vertexHelper);
        }

        /*
        [ContextMenu("Reset Corner Position Ratio")]
        public void ResetCornerPositionRatio()
        {
            //_originalCornerPositionRatio = GetOriginalCornerPositionRatio();
            _cornerPositionRatio = _originalCornerPositionRatio;
        }

        private void InitializeCornerPositionRatio()
        {
            if (_originalCornerPositionRatio == -Vector2.one)
                _originalCornerPositionRatio = GetOriginalCornerPositionRatio();
            if (_cornerPositionRatio == -Vector2.one)
                _cornerPositionRatio = _originalCornerPositionRatio;
        }

        private void ConstrainCornerPositionRatio()
        {
            _cornerPositionRatio.x = Mathf.Clamp(_cornerPositionRatio.x, 0, 0.5f);
            _cornerPositionRatio.y = Mathf.Clamp(_cornerPositionRatio.y, 0, 0.5f);
        }

        private Vector2 GetOriginalCornerPositionRatio()
        {
            int referenceVertexIndex = ImageTypeCornerReferenceVertexIndex(AttachedImage.type);
            var referenceVertex = CachedVertices[referenceVertexIndex].position;

            var rect = RectTrans.rect;
            var pivot = RectTrans.pivot;
            var x = (referenceVertex.x + pivot.x * rect.width) / rect.width;
            var y = (referenceVertex.y + pivot.y * rect.height) / rect.height;

            return new Vector2(x, y);
        }
        */

        private void AdjustVerticesForCornerRatio(List<UIVertex> verts)
        {
            for (int i = 0; i < verts.Count; i++)
            {
                var vertex = verts[i];

                // Calculate horizontal and vertical ratios
                float horizontalRatio = (vertex.position.x + RectTrans.rect.width * RectTrans.pivot.x) / RectTrans.rect.width;
                float verticalRatio = (vertex.position.y + RectTrans.rect.height * RectTrans.pivot.y) / RectTrans.rect.height;

                // Adjust ratios based on corner positions
                horizontalRatio = AdjustRatio(horizontalRatio, _originalCornerPositionRatio.x, _cornerPositionRatio.x);
                verticalRatio = AdjustRatio(verticalRatio, _originalCornerPositionRatio.y, _cornerPositionRatio.y);

                // Update vertex position
                vertex.position.x = horizontalRatio * RectTrans.rect.width - RectTrans.rect.width * RectTrans.pivot.x;
                vertex.position.y = verticalRatio * RectTrans.rect.height - RectTrans.rect.height * RectTrans.pivot.y;

                verts[i] = vertex;
            }
        }
        
        private float AdjustRatio(float ratio, float originalCorner, float targetCorner)
        {
            if (ratio < originalCorner)
                return Mathf.Lerp(0, targetCorner, ratio / originalCorner);
            if (ratio > 1 - originalCorner)
                return Mathf.Lerp(1 - targetCorner, 1, (ratio - (1 - originalCorner)) / originalCorner);
            return Mathf.Lerp(targetCorner, 1 - targetCorner, (ratio - originalCorner) / (1 - originalCorner * 2));
        }
        
        private int ImageTypeCornerReferenceVertexIndex(Image.Type imageType) => imageType switch
        {
            Image.Type.Sliced => 2,
            Image.Type.Filled => 0,
            _ => 0
        };
    }
}
