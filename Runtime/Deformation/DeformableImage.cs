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
        [Tooltip("Adjust corner position ratio for Sliced or Tiled Image types")]
        [SerializeField] private Vector2 cornerPosRatio = Vector2.one * -1; // -1 indicates unset
        private Vector2 oriCornerPosRatio = Vector2.one * -1;

        private Image UIImage => (Image)Graphic;

        /*
        protected override void ModifyVertices(List<UIVertex> verts)
        {
            if (!IsActive() || !IsDeformed || Deformer == null) return;

            if (UIImage.type == Image.Type.Filled)
            {
                Debug.LogWarning("Might not work well Radial Filled at the moment!");

            }
            else if (UIImage.type == Image.Type.Sliced || UIImage.type == Image.Type.Tiled)
            {
                // setting the starting cornerRatio
                if (cornerPosRatio == Vector2.one * -1)
                {
                    cornerPosRatio = verts[ImageTypeCornerReferenceVertexIndex(UIImage.type)].position;
                    cornerPosRatio.x = (cornerPosRatio.x + RectTrans.pivot.x * RectTrans.rect.width) / RectTrans.rect.width;
                    cornerPosRatio.y = (cornerPosRatio.y + RectTrans.pivot.y * RectTrans.rect.height) / RectTrans.rect.height;

                    oriCornerPosRatio = cornerPosRatio;

                }

                // constraining the corner ratio 
                if (cornerPosRatio.x < 0)
                {
                    cornerPosRatio.x = 0;
                }
                if (cornerPosRatio.x >= 0.5f)
                {
                    cornerPosRatio.x = 0.5f;
                }
                if (cornerPosRatio.y < 0)
                {
                    cornerPosRatio.y = 0;
                }
                if (cornerPosRatio.y >= 0.5f)
                {
                    cornerPosRatio.y = 0.5f;
                }

                for (int index = 0; index < verts.Count; index++)
                {
                    var uiVertex = verts[index];

                    // finding the horizontal ratio position (0.0 - 1.0) of a vertex
                    float horRatio = (uiVertex.position.x + RectTrans.rect.width * RectTrans.pivot.x) / RectTrans.rect.width;
                    float verRatio = (uiVertex.position.y + RectTrans.rect.height * RectTrans.pivot.y) / RectTrans.rect.height;

                    if (horRatio < oriCornerPosRatio.x)
                    {
                        horRatio = Mathf.Lerp(0, cornerPosRatio.x, horRatio / oriCornerPosRatio.x);
                    }
                    else if (horRatio > 1 - oriCornerPosRatio.x)
                    {
                        horRatio = Mathf.Lerp(1 - cornerPosRatio.x, 1, (horRatio - (1 - oriCornerPosRatio.x)) / oriCornerPosRatio.x);
                    }
                    else
                    {
                        horRatio = Mathf.Lerp(cornerPosRatio.x, 1 - cornerPosRatio.x, (horRatio - oriCornerPosRatio.x) / (1 - oriCornerPosRatio.x * 2));
                    }

                    if (verRatio < oriCornerPosRatio.y)
                    {
                        verRatio = Mathf.Lerp(0, cornerPosRatio.y, verRatio / oriCornerPosRatio.y);
                    }
                    else if (verRatio > 1 - oriCornerPosRatio.y)
                    {
                        verRatio = Mathf.Lerp(1 - cornerPosRatio.y, 1, (verRatio - (1 - oriCornerPosRatio.y)) / oriCornerPosRatio.y);
                    }
                    else
                    {
                        verRatio = Mathf.Lerp(cornerPosRatio.y, 1 - cornerPosRatio.y, (verRatio - oriCornerPosRatio.y) / (1 - oriCornerPosRatio.y * 2));
                    }

                    uiVertex.position.x = horRatio * RectTrans.rect.width - RectTrans.rect.width * RectTrans.pivot.x;
                    uiVertex.position.y = verRatio * RectTrans.rect.height - RectTrans.rect.height * RectTrans.pivot.y;
                    //uiVertex.position.z = pos.z;

                    verts[index] = uiVertex;
                }
            }

            base.ModifyVertices(verts);
        }
        */
        public override void ModifyMesh(VertexHelper vertexHelper)
        {
            if (!IsActive() || !IsDeformed || Deformer == null) return;

            // Handle additional behavior for specific Image types (e.g., Sliced or Tiled)
            if (UIImage.type == Image.Type.Sliced || UIImage.type == Image.Type.Tiled)
            {
                var verts = new List<UIVertex>();
                vertexHelper.GetUIVertexStream(verts);

                if (cornerPosRatio == Vector2.one * -1) InitializeCornerPositionRatio(verts);

                ConstrainCornerPositionRatio();
                AdjustVerticesForCornerRatio(verts);

                vertexHelper.Clear();
                vertexHelper.AddUIVertexTriangleStream(verts);
            }

            // Apply deformation from the assigned shape handler
            base.ModifyMesh(vertexHelper);
        }

        private void InitializeCornerPositionRatio(List<UIVertex> verts)
        {
            int referenceVertexIndex = ImageTypeCornerReferenceVertexIndex(UIImage.type);
            var referenceVertex = verts[referenceVertexIndex].position;

            cornerPosRatio.x = (referenceVertex.x + RectTrans.pivot.x * RectTrans.rect.width) / RectTrans.rect.width;
            cornerPosRatio.y = (referenceVertex.y + RectTrans.pivot.y * RectTrans.rect.height) / RectTrans.rect.height;

            oriCornerPosRatio = cornerPosRatio;
        }


        private void ConstrainCornerPositionRatio()
        {
            cornerPosRatio.x = Mathf.Clamp(cornerPosRatio.x, 0, 0.5f);
            cornerPosRatio.y = Mathf.Clamp(cornerPosRatio.y, 0, 0.5f);
        }

        private void AdjustVerticesForCornerRatio(List<UIVertex> verts)
        {
            for (int i = 0; i < verts.Count; i++)
            {
                var vertex = verts[i];

                // Calculate horizontal and vertical ratios
                float horizontalRatio = (vertex.position.x + RectTrans.rect.width * RectTrans.pivot.x) / RectTrans.rect.width;
                float verticalRatio = (vertex.position.y + RectTrans.rect.height * RectTrans.pivot.y) / RectTrans.rect.height;

                // Adjust ratios based on corner positions
                horizontalRatio = AdjustRatio(horizontalRatio, oriCornerPosRatio.x, cornerPosRatio.x);
                verticalRatio = AdjustRatio(verticalRatio, oriCornerPosRatio.y, cornerPosRatio.y);

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
