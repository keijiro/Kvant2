using UnityEngine;
using System.Collections;

namespace Kvant {

public partial class Spray
{
    //
    // Bulk mesh storage class
    //
    // Makes a given number of copies of the shapes,
    // and combine them into the minimum number of meshes.
    //
    [System.Serializable]
    class BulkMesh
    {
        // Combined meshes.
        Mesh[] _segments = new Mesh[0];

        #region Public Properties And Methods

        public Mesh[] segments { get { return _segments; } }

        public BulkMesh(Mesh[] shapes, int duplicate, Texture buffer)
        {
            BuildInternal(shapes, duplicate, buffer);
        }

        public void Rebuild(Mesh[] shapes, int duplicate, Texture buffer)
        {
            Release();
            BuildInternal(shapes, duplicate, buffer);
        }

        public void Release()
        {
            foreach (var m in _segments) DestroyImmediate(m);
            _segments = new Mesh[0];
        }

        #endregion

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
        void BuildInternal(Mesh[] shapes, int duplicate, Texture buffer)
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
            if (vc_shapes == 0) return;

            var shape_set_per_segment = 65000 / vc_shapes;
            var segment_count = duplicate / set_per_mesh + 1;
                // Create vertex arrays.
                var vc = vc_shapes * shape_set_per_segment;
                var ic = ic_shapes * shape_set_per_segment;

                var va = new Vector3[vc];
                var na = new Vector3[vc];
                var ta = new Vector2[vc];
                var ia = new int[ic];

                for (int va_i = 0, ia_i = 0, e_i = 0; va_i < vc; e_i++)
                {
                    var s = cache[e_i % shapes.Length];

                    s.CopyVerticesTo(va, va_i);
                    s.CopyNormalsTo(na, va_i);
                    s.CopyIndicesTo(ia, ia_i, va_i);

                    var uv = new Vector2(
                            (float)(e_i % buffer.width) / buffer.width,
                            (float)(e_i / buffer.width) / buffer.height
                            );

                    for (var i = 0; i < s.VertexCount; i++) ta[va_i + i] = uv;

                    va_i += s.VertexCount;
                    ia_i += s.IndexCount;
                }








            _segments = new Mesh[segment_count];

            for (var segment_i = 0; segment_i < segment_count; segment_i++)
            {
                // Create vertex arrays.
                var vc = vc_shapes * shape_set_per_segment;
                var ic = ic_shapes * shape_set_per_segment;

                var va = new Vector3[vc];
                var na = new Vector3[vc];
                var ta = new Vector2[vc];
                var ia = new int[ic];

                for (int va_i = 0, ia_i = 0, e_i = 0; va_i < vc; e_i++)
                {
                    var s = cache[e_i % shapes.Length];

                    s.CopyVerticesTo(va, va_i);
                    s.CopyNormalsTo(na, va_i);
                    s.CopyIndicesTo(ia, ia_i, va_i);

                    var uv = new Vector2(
                            (float)(e_i % buffer.width) / buffer.width,
                            (float)(e_i / buffer.width) / buffer.height
                            );

                    for (var i = 0; i < s.VertexCount; i++) ta[va_i + i] = uv;

                    va_i += s.VertexCount;
                    ia_i += s.IndexCount;
                }
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

            _segments = new Mesh[1] { mesh };
        }

        #endregion
    }
}

} // namespace Kvant
