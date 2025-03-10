using System.Collections.Generic;
using System.Text;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
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
        [SerializeField, Range(0.001f, 30f)] private float _widthResolution = 5.0f;
        [SerializeField, Range(0.001f, 30f)] private float _heightResolution = 5.0f;
        [SerializeField] private List<BaseDeformer> _deformers; // Expect an implementation of BaseDeformer

        private bool _isUpdateRequired = true;
        protected bool IsUpdateRequired => _isUpdateRequired;
        protected readonly List<UIVertex> CachedVertices = new();
        //private readonly List<UIVertex> _cachedQuads = new();
        private static readonly ProfilerMarker _tessellationMarker = new("Deformable Graphic.Mesh Tessellation");
        private static readonly ProfilerMarker _tessellateQuadsMarker = new("Mesh Tessellation.Tessellate Quads");
        private static readonly ProfilerMarker _applyingNewVerticesMarker = new("Deformable Graphic.Applying new vertices");
        private static readonly ProfilerMarker _deformationMarker = new("Deformable Graphic.Deformation");
        private static readonly ProfilerMarker _verticesAddingMarker = new("Deformable Graphic.Vertices Adding");
        public RectTransform RectTrans => graphic.rectTransform;
        
        protected override void Awake()
        {
            base.Awake();
            _isUpdateRequired = true;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            Rebuild();
        }
        public void Rebuild()
        {
            _isUpdateRequired = true;
            graphic.SetVerticesDirty();
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
                
                if (_subdivide)
                    TessellateGraphic(CachedVertices);
                
                // DeformVertices(CachedVertices);
                
                _isUpdateRequired = false;
            }
            
            vh.Clear();
            vh.AddUIVertexTriangleStream(CachedVertices);
        }

        private void DeformVertices(List<UIVertex> verts)
        {
            _deformationMarker.Begin();

            if (_deformers == null || _deformers.Count == 0)
            {
                _deformationMarker.End();
                return;
            }
            
            var rect = RectTrans.rect;
            var pivot = RectTrans.pivot;

            foreach (var deformer in _deformers)
            {
                if (deformer == null || !deformer.enabled)
                    continue;
                
                deformer.BeginDeform();
                
                for (int index = 0; index < verts.Count; index++)
                {
                    var uiVertex = verts[index];
                    var vertPos = uiVertex.position;
                    
                    // finding the horizontal ratio position (0.0 - 1.0) of a vertex
                    float horRatio = (vertPos.x + rect.width * pivot.x) / rect.width;
                    float verRatio = (vertPos.y + rect.height * pivot.y) / rect.height;
                    
                    vertPos = deformer.DeformPoint(horRatio, verRatio);

                    uiVertex.position = vertPos;
                    verts[index] = uiVertex;
                }
            }

            _deformationMarker.End();
        }

        private void TessellateGraphic(List<UIVertex> verts)
        {
            _tessellationMarker.Begin();
            
            int originalQuadsCount = verts.Count / 6;
            int newVerticesCount = 0;
            int tesselatedCountOld = 0;
            Vector2 quadSize = new Vector2(100f / _widthResolution, 100f / _heightResolution);
            NativeArray<OriginalQuad> originalQuads = new NativeArray<OriginalQuad>(originalQuadsCount, Allocator.TempJob);
            for (int v = 0; v < verts.Count; v += 6)
            {
                var bl = verts[v]; // bottom left
                var tl = verts[v + 1]; // top left
                var tr = verts[v + 2]; // top right
                // verts[3] is redundant, top right
                var br = verts[v + 4]; // bottom right
                // verts[5] is redundant, bottom left
                
                var quad = new OriginalQuad(bl, tl, tr, br, quadSize);
                originalQuads[v / 6] = quad;
                newVerticesCount += quad.NewVerticesCount;
                tesselatedCountOld += quad.TessellationCount;
            }

            // var savedRatio = (float)newVerticesCount / (tesselatedCountOld * 4);
            // var saved = Mathf.Abs(savedRatio - 1f) * 100f;
            // Debug.Log($"Operations Count: New: {newVerticesCount};\tOld: {tesselatedCountOld * 4}; Saved: {Mathf.RoundToInt(saved)}%");
            
            NativeArray<UIVertex> quads = new NativeArray<UIVertex>(newVerticesCount, Allocator.TempJob);

            _tessellateQuadsMarker.Begin();
            var job = new TessellationJob
            {
                OriginalQuads = originalQuads,
                Quads = quads,
            };
            JobHandle handle = job.Schedule();
            handle.Complete();
            _tessellateQuadsMarker.End();
            
            _deformationMarker.Begin();
            if (_deformers == null || _deformers.Count == 0)
            {
                _deformationMarker.End();
                return;
            }
            
            var rect = RectTrans.rect;
            var pivot = RectTrans.pivot;

            foreach (var deformer in _deformers)
            {
                if (deformer == null || !deformer.enabled)
                    continue;
                
                for (int index = 0; index < quads.Length; index++)
                {
                    var uiVertex = quads[index];
                    var vertPos = uiVertex.position;
                    
                    // finding the horizontal ratio position (0.0 - 1.0) of a vertex
                    float horRatio = (vertPos.x + rect.width * pivot.x) / rect.width;
                    float verRatio = (vertPos.y + rect.height * pivot.y) / rect.height;
                    
                    vertPos = deformer.DeformPoint(horRatio, verRatio);

                    uiVertex.position = vertPos;
                    quads[index] = uiVertex;
                }
            }
            _deformationMarker.End();

            // process new quads and turn them into triangles
            verts.Clear();
            _applyingNewVerticesMarker.Begin();
            var processedVerticesCount = 0;
            for (int i = 0; i < originalQuads.Length; i++)
            {
                var quad = originalQuads[i];
                var widthVerts = quad.WidthQuadEdgeNum + 1;
                for (int x = 0; x < quad.WidthQuadEdgeNum; x++)
                {
                    for (int y = 0; y < quad.HeightQuadEdgeNum; y++)
                    {
                        int downLeftIndex = processedVerticesCount + widthVerts * y + x;
                        int downRightIndex = downLeftIndex + 1;
                        int upLeftIndex = processedVerticesCount + widthVerts * (y + 1) + x;
                        int upRightIndex = upLeftIndex + 1;
                        
                        verts.Add(quads[downLeftIndex]);
                        verts.Add(quads[upLeftIndex]);
                        verts.Add(quads[upRightIndex]);
                        verts.Add(quads[downLeftIndex]);
                        verts.Add(quads[upRightIndex]);
                        verts.Add(quads[downRightIndex]);
                    }
                }
                processedVerticesCount += quad.NewVerticesCount;
            }
            _applyingNewVerticesMarker.End();
            originalQuads.Dispose();
            quads.Dispose();
            _tessellationMarker.End();
        }
        
        private static void TessellateQuad(NativeArray<UIVertex> quads, OriginalQuad quad, int startIndex)
        {
            for (int x = 0; x < quad.WidthQuadEdgeNum + 1; x++)
            {
                for (int y = 0; y < quad.HeightQuadEdgeNum + 1; y++)
                {
                    float xRatio = (float)x / quad.WidthQuadEdgeNum;
                    float yRatio = (float)y / quad.HeightQuadEdgeNum;
                    int index = startIndex + (quad.WidthQuadEdgeNum + 1) * y + x;
                    quads[index] = quad.VertexBerp(xRatio, yRatio);
                }
            }
        }
        
        [BurstCompile]
        private struct TessellationJob : IJob
        {
            [ReadOnly] public NativeArray<OriginalQuad> OriginalQuads;
            public NativeArray<UIVertex> Quads;
            
            public void Execute()
            {
                int processedVertices = 0;
                for (int i = 0; i < OriginalQuads.Length; i++)
                {
                    TessellateQuad(Quads, OriginalQuads[i], processedVertices);
                    processedVertices += OriginalQuads[i].NewVerticesCount;
                }
            }
        }
        
        public struct OriginalQuad
        {
            public UIVertex BottomLeft;
            public UIVertex TopLeft;
            public UIVertex TopRight;
            public UIVertex BottomRight;
            public int WidthQuadEdgeNum;
            public int HeightQuadEdgeNum;
            public int TessellationCount;
            public int NewVerticesCount;

            public OriginalQuad(UIVertex bottomLeft, UIVertex topLeft, UIVertex topRight, UIVertex bottomRight, Vector2 size)
            {
                BottomLeft = bottomLeft;
                TopLeft = topLeft;
                TopRight = topRight;
                BottomRight = bottomRight;
                HeightQuadEdgeNum = Mathf.Max(1, Mathf.CeilToInt((TopLeft.position - BottomLeft.position).magnitude / size.y));
                WidthQuadEdgeNum = Mathf.Max(1, Mathf.CeilToInt((TopRight.position - TopLeft.position).magnitude / size.x));
                TessellationCount = HeightQuadEdgeNum * WidthQuadEdgeNum;
                NewVerticesCount = (HeightQuadEdgeNum + 1) * (WidthQuadEdgeNum + 1);
            }

            public UIVertex VertexBerp(float xTime, float yTime)
            {
                var topX = VertexLerp(TopLeft, TopRight, xTime);
                var bottomX = VertexLerp(BottomLeft, BottomRight, xTime);
                return VertexLerp(bottomX, topX, yTime);
            }
            
            private UIVertex VertexLerp(UIVertex a, UIVertex b, float time)
            {
                var tmpUIVertex = new UIVertex
                {
                    position = Vector3.LerpUnclamped(a.position, b.position, time),
                    normal = Vector3.LerpUnclamped(a.normal, b.normal, time),
                    tangent = Vector3.LerpUnclamped(a.tangent, b.tangent, time),
                    uv0 = Vector2.LerpUnclamped(a.uv0, b.uv0, time),
                    uv1 = Vector2.LerpUnclamped(a.uv1, b.uv1, time),
                    color = Color32.LerpUnclamped(a.color, b.color, time)
                };
                return tmpUIVertex;
            }
        }
    }
}