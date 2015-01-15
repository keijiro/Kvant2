using UnityEngine;
using System.Collections;

namespace Kvant {

public partial class Spray
{
    [System.Serializable]
    class BulkMesh
    {
        Mesh[] _meshes;

        public Mesh[] meshes { get { return _meshes; } }

        public BulkMesh(int maxParticles, Mesh[] shapes, int bufferWidth, int bufferHeight)
        {
            BuildMeshes(maxParticles, shapes, bufferWidth, bufferHeight);
        }

        public void Rebuild(int maxParticles, Mesh[] shapes, int bufferWidth, int bufferHeight)
        {
            foreach (var m in _meshes) DestroyImmediate(m);
            BuildMeshes(maxParticles, shapes, bufferWidth, bufferHeight);
        }

        #region Private Methods

        // Cache structure to store shape information.
        struct ShapeCacheData
        {
            Vector3[] vertices;
            Vector3[] normals;
            int[] indices;

            public ShapeCacheData(Mesh mesh)
            {
                if (mesh)
                {
                    vertices = mesh.vertices;
                    normals = mesh.normals;
                    indices = mesh.GetIndices(0);
                }
                else
                {
                    vertices = null;
                    normals = null;
                    indices = null;
                }
            }

            public int VertexCount { get { return vertices.Length; } }
            public int IndexCount { get { return indices.Length; } }

            public void CopyVerticesTo(Vector3[] destination, int position)
            {
                System.Array.Copy(vertices, 0, destination, position, vertices.Length);
            }

            public void CopyNormalsTo(Vector3[] destination, int position)
            {
                System.Array.Copy(normals, 0, destination, position, normals.Length);
            }

            public void CopyIndicesTo(int[] destination, int position, int offset)
            {
                for (var i = 0; i < indices.Length; i++)
                    destination[position + i] = offset + indices[i];
            }
        }

        // Mesh builder functoin.
        void BuildMeshes(int maxParticles, Mesh[] shapes, int bufferWidth, int bufferHeight)
        {
            // Store the meshes into the shape cache.
            var cache = new ShapeCacheData[shapes.Length];
            for (var i = 0; i < shapes.Length; i++)
                cache[i] = new ShapeCacheData(shapes[i]);

            // Count the number of vertices and indices in the shape cache.
            var vc_shapes = 0;
            var ic_shapes = 0;
            foreach (var s in cache) {
                vc_shapes += s.VertexCount;
                ic_shapes += s.IndexCount;
            }

            // If there is nothing, make a null array.
            if (vc_shapes == 0) {
                _meshes = new Mesh[0];
                return;
            }

            // Create vertex arrays.
            var rep = maxParticles / shapes.Length + 1;

            var vc = vc_shapes * rep;
            var ic = ic_shapes * rep;

            var va = new Vector3[vc];
            var na = new Vector3[vc];
            var ta = new Vector2[vc];
            var ia = new int[ic];

            for (int va_i = 0, ia_i = 0, e_i = 0; va_i < vc; e_i++)
            {
                var s = cache[e_i % shapes.Length];

                var uv = new Vector2(
                    (float)(e_i % bufferWidth) / bufferWidth,
                    (float)(e_i / bufferWidth) / bufferHeight
                );

                s.CopyVerticesTo(va, va_i);
                s.CopyNormalsTo(na, va_i);
                s.CopyIndicesTo(ia, ia_i, va_i);

                for (var i = 0; i < s.VertexCount; i++)
                    ta[va_i + i] = uv;

                va_i += s.VertexCount;
                ia_i += s.IndexCount;
            }

            // Create a mesh object.
            var mesh = new Mesh();

            mesh.vertices = va;
            mesh.normals = na;
            mesh.uv = ta;

            mesh.SetIndices(ia, MeshTopology.Triangles, 0);
            mesh.Optimize();

            // This only for temporary use. Don't save.
            mesh.hideFlags = HideFlags.DontSave;

            // Avoid being culled.
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100);

            _meshes = new Mesh[1] { mesh };
        }

        #endregion
    }
}

} // namespace Kvant
