using UnityEngine;
using System.Collections;

namespace Kvant {

public partial class Spray
{
    //
    // Bulk mesh class
    //
    // Duplicate and combine the given meshes to a single mesh.
    // It duplicate the meshes as many as possible, but it will be limited by
    // the number of vertices (<64k) and copies (<4k).
    //
    [System.Serializable]
    class BulkMesh
    {
        // Single combined mesh.
        Mesh _mesh;

        public Mesh mesh { get { return _mesh; } }

        public BulkMesh(int maxParticles, Mesh[] shapes, int bufferWidth, int bufferHeight)
        {
            BuildInternal(maxParticles, shapes, bufferWidth, bufferHeight);
        }

        public void Rebuild(int maxParticles, Mesh[] shapes, int bufferWidth, int bufferHeight)
        {
            DestroyImmediate(_mesh);
            BuildInternal(maxParticles, shapes, bufferWidth, bufferHeight);
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
        void BuildInternal(int maxParticles, Mesh[] shapes, int bufferWidth, int bufferHeight)
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

            // If there is nothing, make a null mesh.
            if (vc_shapes == 0) {
                _mesh = new Mesh();
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
            _mesh = new Mesh();

            _mesh.vertices = va;
            _mesh.normals = na;
            _mesh.uv = ta;

            _mesh.SetIndices(ia, MeshTopology.Triangles, 0);
            _mesh.Optimize();

            // This only for temporary use. Don't save.
            _mesh.hideFlags = HideFlags.DontSave;

            // Avoid being culled.
            _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100);
        }

        #endregion
    }
}

} // namespace Kvant
