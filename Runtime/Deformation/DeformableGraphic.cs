using System.Collections.Generic;
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
        [SerializeField, Range(1f, 30f)] private float _widthResolution = 5.0f;
        [SerializeField, Range(1f, 30f)] private float _heightResolution = 5.0f;
        [SerializeField] private List<BaseDeformer> _deformers; // Expect an implementation of BaseDeformer

        private bool _isUpdateRequired = true;
        protected bool IsUpdateRequired => _isUpdateRequired;
        protected readonly List<UIVertex> CachedVertices = new();
        //private readonly List<UIVertex> _cachedQuads = new();
        private static readonly ProfilerMarker _tessellationMarker = new("Deformable Graphic.Mesh Tessellation");
        private static readonly ProfilerMarker _tessellateQuadMarker = new("Mesh Tessellation.Tessellate Quad");
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
                
                if (_subdivide)
                    TessellateGraphic(CachedVertices);
                
                DeformVertices(CachedVertices);
                
                _isUpdateRequired = false;
            }
            
            vh.Clear();
            vh.AddUIVertexTriangleStream(CachedVertices);
        }

        private void DeformVertices(List<UIVertex> verts)
        {
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
            _tessellationMarker.Begin();
            
            int originalQuadsCount = verts.Count / 6;
            int tessellatedCount = 0;
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
                tessellatedCount += quad.TessellationCount;
            }
            NativeArray<UIVertex> quads = new NativeArray<UIVertex>(tessellatedCount * 4, Allocator.TempJob);

            _tessellateQuadMarker.Begin();
            var job = new TessellationJob()
            {
                OriginalQuads = originalQuads,
                Quads = quads,
            };
            JobHandle handle = job.Schedule();
            handle.Complete();
            originalQuads.Dispose();
            _tessellateQuadMarker.End();

            // process new quads and turn them into triangles
            verts.Clear();
            for (int i = 0; i < tessellatedCount * 4; i += 4)
            {
                verts.Add(quads[i]);
                verts.Add(quads[i + 1]);
                verts.Add(quads[i + 2]);
                verts.Add(quads[i + 2]);
                verts.Add(quads[i + 3]);
                verts.Add(quads[i]);
            }
            quads.Dispose();
            _tessellationMarker.End();
        }

        [BurstCompile]
        private struct TessellationJob : IJob
        {
            [ReadOnly] public NativeArray<OriginalQuad> OriginalQuads;
            public NativeArray<UIVertex> Quads;
            
            public void Execute()
            {
                int tessellated = 0;
                for (int i = 0; i < OriginalQuads.Length; i++)
                {
                    TessellateQuad(Quads, OriginalQuads[i], tessellated);
                    tessellated += OriginalQuads[i].TessellationCount * 4;
                }
            }
            
            private static void TessellateQuad(NativeArray<UIVertex> quads, OriginalQuad quad, int startIndex)
            {
                int quadIdx = 0;
            
                for (int x = 0; x < quad.WidthQuadEdgeNum; x++)
                {
                    for (int y = 0; y < quad.HeightQuadEdgeNum; y++, quadIdx += 4)
                    {
                        float xRatio = (float)x / quad.WidthQuadEdgeNum;
                        float yRatio = (float)y / quad.HeightQuadEdgeNum;
                        float xPlusOneRatio = (float)(x + 1) / quad.WidthQuadEdgeNum;
                        float yPlusOneRatio = (float)(y + 1) / quad.HeightQuadEdgeNum;
                    
                        var index = startIndex + quadIdx;
                    
                        quads[index] = quad.VertexBerp(xRatio, yRatio);
                        quads[index + 1] = quad.VertexBerp(xRatio, yPlusOneRatio);
                        quads[index + 2] = quad.VertexBerp(xPlusOneRatio, yPlusOneRatio);
                        quads[index + 3] = quad.VertexBerp(xPlusOneRatio, yRatio);
                
                    }
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

            public OriginalQuad(UIVertex bottomLeft, UIVertex topLeft, UIVertex topRight, UIVertex bottomRight, Vector2 size)
            {
                BottomLeft = bottomLeft;
                TopLeft = topLeft;
                TopRight = topRight;
                BottomRight = bottomRight;
                HeightQuadEdgeNum = Mathf.Max(1, Mathf.CeilToInt((TopLeft.position - BottomLeft.position).magnitude / size.y));
                WidthQuadEdgeNum = Mathf.Max(1, Mathf.CeilToInt((TopRight.position - TopLeft.position).magnitude / size.x));
                TessellationCount = HeightQuadEdgeNum * WidthQuadEdgeNum;
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