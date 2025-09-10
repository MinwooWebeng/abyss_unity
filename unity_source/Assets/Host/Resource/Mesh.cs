using Dummiesman;
using System.Runtime.InteropServices;
using UnityEngine;

#nullable enable
namespace Host
{
    class Mesh : StaticResource
    {
        public UnityEngine.Mesh? UnityMesh;
        public Mesh(string file_name) : base(file_name) {
        }
        private bool _is_inited = false;
        public override void Init()
        {
            UnityMesh = new();
            _is_inited = true;
        }
        public override void UpdateMMFRead()
        {
            if (!_is_inited)
                throw new System.Exception("mesh not initialized");

            if (CurrentSize != Size)
                throw new System.Exception("mesh file not loaded at once");

            var stream = _mmf.CreateViewStream(Marshal.SizeOf<StaticResourceHeader>(), Size);
            var result_gameobject = new OBJLoader().Load(stream);
            OverwriteMesh(result_gameobject.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh, UnityMesh!);
            UnityEngine.Object.Destroy(result_gameobject);
        }
        private static void OverwriteMesh(UnityEngine.Mesh src, UnityEngine.Mesh dst)
        {
            dst.Clear(keepVertexLayout: false);
            dst.indexFormat = src.indexFormat;
            dst.subMeshCount = src.subMeshCount;

            // Vert data
            dst.SetVertices(src.vertices);
            if (src.normals != null && src.normals.Length == src.vertexCount) dst.SetNormals(src.normals);
            if (src.tangents != null && src.tangents.Length == src.vertexCount) dst.SetTangents(src.tangents);
            if (src.colors != null && src.colors.Length == src.vertexCount) dst.SetColors(src.colors);
            if (src.uv != null && src.uv.Length == src.vertexCount) dst.SetUVs(0, src.uv);
            if (src.uv2 != null && src.uv2.Length == src.vertexCount) dst.SetUVs(1, src.uv2);
            if (src.uv3 != null && src.uv3.Length == src.vertexCount) dst.SetUVs(2, src.uv3);
            if (src.uv4 != null && src.uv4.Length == src.vertexCount) dst.SetUVs(3, src.uv4);

            // Indices per submesh
            for (int i = 0; i < src.subMeshCount; i++)
            {
                var desc = src.GetSubMesh(i);
                var indices = src.GetIndices(i, applyBaseVertex: false);
                dst.SetIndices(indices, desc.topology, i, calculateBounds: false);
            }

            // Skinning (if present)
            if (src.bindposes != null && src.bindposes.Length > 0) dst.bindposes = src.bindposes;
            if (src.boneWeights != null && src.boneWeights.Length == src.vertexCount) dst.boneWeights = src.boneWeights;

            // Bounds
            dst.bounds = src.bounds;
        }
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}