using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;

namespace TarasK8.UI.Deformation
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Graphic))]
    [DisallowMultipleComponent]
    [AddComponentMenu("Optimized UI/Deformation/Deformable Graphic")]
    public class DeformableGraphic : BaseMeshEffect
    {
        [SerializeField] private bool _liveUpdate = false;
        [SerializeField] private bool _subdivide = true;
        [SerializeField, Range(1f, 30f)] private float _widthResolution = 5.0f;
        [SerializeField, Range(1f, 30f)] private float _heightResolution = 5.0f;
        [SerializeField] private List<BaseDeformer> _deformers; // Expect an implementation of BaseDeformer

        private bool _isUpdateRequired = true;
        protected bool IsUpdateRequired => _isUpdateRequired;
        protected readonly List<UIVertex> CachedVertices = new();
        private readonly List<UIVertex> _cachedQuads = new();
        private static readonly ProfilerMarker _tessellationMarker = new("Deformable Graphic.Mesh Tessellation");
        private static readonly ProfilerMarker _deformationMarker = new("Deformable Graphic.Deformation");
        public RectTransform RectTrans => graphic.rectTransform;
        
        protected override void Awake()
        {
            base.Awake();
            _isUpdateRequired = true;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            ForceRebuild();
        }

        public void Rebuild()
        {
            _isUpdateRequired = true;
        }

        public void ForceRebuild()
        {
            Rebuild();
            if (graphic.enabled)
            {
                graphic.SetVerticesDirty();
            }
        }

        public override void ModifyMesh(Mesh mesh)
        {
            if (!IsActive()) return;

            using var vh = new VertexHelper(mesh);
            ModifyMesh(vh);
            vh.FillMesh(mesh);
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
                return;
            
            if (_isUpdateRequired || _liveUpdate)
            {
                CachedVertices.Clear();
                vh.GetUIVertexStream(CachedVertices);
                ModifyVertices(CachedVertices);
                _isUpdateRequired = false;
            }
            
            vh.Clear();
            vh.AddUIVertexTriangleStream(CachedVertices);
        }

        private void ModifyVertices(List<UIVertex> verts)
        {
            if (!IsActive())
                return;

            //Debug.Log("Modify Vertices");
            if (_subdivide)
            {
                _tessellationMarker.Begin();
                TessellateGraphic(verts);
                _tessellationMarker.End();
            }

            _deformationMarker.Begin();
            var rect = RectTrans.rect;
            var pivot = RectTrans.pivot;

            for (int index = 0; index < verts.Count; index++)
            {
                var uiVertex = verts[index];
                
                if (_deformers != null && _deformers.Count > 0)
                {
                    // finding the horizontal ratio position (0.0 - 1.0) of a vertex
                    var vertPos = uiVertex.position;

                    foreach (var deformer in _deformers)
                    {
                        float horRatio = (vertPos.x + rect.width * pivot.x) / rect.width;
                        float verRatio = (vertPos.y + rect.height * pivot.y) / rect.height;
                        
                        if(deformer != null && deformer.enabled)
                            vertPos = deformer.DeformPoint(horRatio, verRatio);
                    }

                    uiVertex.position = vertPos;
                }

                verts[index] = uiVertex;
            }
            _deformationMarker.End();
        }
        
        private void TessellateGraphic(List<UIVertex> verts)
        {
            for (int v = 0; v < verts.Count; v += 6)
            {
                _cachedQuads.Add(verts[v]); // bottom left
                _cachedQuads.Add(verts[v + 1]); // top left
                _cachedQuads.Add(verts[v + 2]); // top right
                // verts[3] is redundant, top right
                _cachedQuads.Add(verts[v + 4]); // bottom right
                // verts[5] is redundant, bottom left
            }

            int originalQuadNumbers = _cachedQuads.Count / 4;
            for (int q = 0; q < originalQuadNumbers; q++)
            {
                TessellateQuad(_cachedQuads, q * 4);
            }

            // remove original quads
            _cachedQuads.RemoveRange(0, originalQuadNumbers * 4);

            verts.Clear();

            // process new quads and turn them into triangles
            for (int q = 0; q < _cachedQuads.Count; q += 4)
            {
                verts.Add(_cachedQuads[q]);
                verts.Add(_cachedQuads[q + 1]);
                verts.Add(_cachedQuads[q + 2]);
                verts.Add(_cachedQuads[q + 2]);
                verts.Add(_cachedQuads[q + 3]);
                verts.Add(_cachedQuads[q]);
            }

            _cachedQuads.Clear();
        }
        
        private void TessellateQuad(List<UIVertex> quads, int index)
        {
            UIVertex vBottomLeft = quads[index];
            UIVertex vTopLeft = quads[index + 1];
            UIVertex vTopRight = quads[index + 2];
            UIVertex vBottomRight = quads[index + 3];

            Vector2 quadSize = new Vector2(100f / _widthResolution, 100f / _heightResolution);

            int heightQuadEdgeNum = Mathf.Max(1, Mathf.CeilToInt((vTopLeft.position - vBottomLeft.position).magnitude / quadSize.y));
            int widthQuadEdgeNum = Mathf.Max(1, Mathf.CeilToInt((vTopRight.position - vTopLeft.position).magnitude / quadSize.x));

            int quadIdx = 0;
            
            int count = widthQuadEdgeNum * heightQuadEdgeNum;

            for (int i = 0; i < count; i++)
            {
                int x = i / heightQuadEdgeNum;
                int y = i % heightQuadEdgeNum;
                
                quads.Add(new UIVertex());
                quads.Add(new UIVertex());
                quads.Add(new UIVertex());
                quads.Add(new UIVertex());
                
                float xRatio = (float)x / widthQuadEdgeNum;
                float yRatio = (float)y / heightQuadEdgeNum;
                float xPlusOneRatio = (float)(x + 1) / widthQuadEdgeNum;
                float yPlusOneRatio = (float)(y + 1) / heightQuadEdgeNum;
                
                quads[quads.Count - 4] = VertexBerp(vBottomLeft, vTopLeft, vTopRight, vBottomRight, xRatio, yRatio);
                quads[quads.Count - 3] = VertexBerp(vBottomLeft, vTopLeft, vTopRight, vBottomRight, xRatio, yPlusOneRatio);
                quads[quads.Count - 2] = VertexBerp(vBottomLeft, vTopLeft, vTopRight, vBottomRight, xPlusOneRatio, yPlusOneRatio);
                quads[quads.Count - 1] = VertexBerp(vBottomLeft, vTopLeft, vTopRight, vBottomRight, xPlusOneRatio, yRatio);
            }
        }
        
        private static UIVertex VertexBerp(UIVertex vBottomLeft, UIVertex vTopLeft, UIVertex vTopRight, UIVertex vBottomRight, float xTime, float yTime)
        {
            var topX = VertexLerp(vTopLeft, vTopRight, xTime);
            var bottomX = VertexLerp(vBottomLeft, vBottomRight, xTime);
            return VertexLerp(bottomX, topX, yTime);
        }
        
        private static UIVertex VertexLerp(UIVertex a, UIVertex b, float time)
        {
            var tmpUIVertex = new UIVertex
            {
                position = Vector3.LerpUnclamped(a.position, b.position, time),
                normal = Vector3.LerpUnclamped(a.normal, b.normal, time),
                tangent = Vector3.LerpUnclamped(a.tangent, b.tangent, time),
                uv0 = Vector2.LerpUnclamped(a.uv0, b.uv0, time),
                uv1 = Vector2.LerpUnclamped(a.uv1, b.uv1, time),
                color = Color.LerpUnclamped(a.color, b.color, time)
            };
            return tmpUIVertex;
        }
    }
}