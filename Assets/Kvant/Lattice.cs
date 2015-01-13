using UnityEngine;
using System.Collections;

namespace Kvant {

//
// Lattice Builder
//
// Builds a triangular lattice for GPGPU geometry construction.
//
// It has two submeshes in order to store two types of vertex orders.
//
// 1st submesh: A-B-C
// 2nd submesh: A-C-D
//
// B   C
// .---.---.
//  \ / \ /
//   .---.--
//   A   D
//
// Vertex attribute usage:
//
// POS - not in use
// UV1 - texcoord for position buffer
// UV2 - texcoord for normal vector buffer
//

public class Lattice
{
    static public Mesh Build(int columns, int rows)
    {
        var Nx = columns;
        var Ny = rows + 1;

        var Sx = 1.0f / Nx;
        var Sy = 1.0f / Ny;

        // Texcoord Array for UV1 and UV2.
        var TA1 = new Vector2[Nx * (Ny - 1) * 6];
        var TA2 = new Vector2[Nx * (Ny - 1) * 6];
        var iTA = 0;

        // 1st submesh (A-B-C triangles).
        for (var Iy = 0; Iy < Ny - 1; Iy++)
        {
            for (var Ix = 0; Ix < Nx; Ix++, iTA += 3)
            {
                var Ix2 = Ix + 0.5f * (Iy & 1);
                // UVs for position.
                TA1[iTA + 0] = new Vector2(Sx * (Ix2 + 0.0f), Sy * (Iy + 0));
                TA1[iTA + 1] = new Vector2(Sx * (Ix2 - 0.5f), Sy * (Iy + 1));
                TA1[iTA + 2] = new Vector2(Sx * (Ix2 + 0.5f), Sy * (Iy + 1));
                // UVs for normal vector.
                TA2[iTA] = TA2[iTA + 1] = TA2[iTA + 2] = new Vector2(Sx * Ix2, Sy * Iy);
            }
        }

        // 2nd submesh (A-C-D triangls).
        for (var Iy = 0; Iy < Ny - 1; Iy++)
        {
            for (var Ix = 0; Ix < Nx; Ix++, iTA += 3)
            {
                var Ix2 = Ix + 0.5f * (Iy & 1);
                // UVs for position.
                TA1[iTA + 0] = new Vector2(Sx * (Ix2 + 0.0f), Sy * (Iy + 0));
                TA1[iTA + 1] = new Vector2(Sx * (Ix2 + 0.5f), Sy * (Iy + 1));
                TA1[iTA + 2] = new Vector2(Sx * (Ix2 + 1.0f), Sy * (Iy + 0));
                // UVs for normal vector.
                TA2[iTA] = TA2[iTA + 1] = TA2[iTA + 2] = new Vector2(Sx * Ix2, Sy * Iy);
            }
        }

        // Index arrays for the 1st and 2nd submesh (surfaces).
        var IA1 = new int[Nx * (Ny - 1) * 3];
        var IA2 = new int[Nx * (Ny - 1) * 3];
        for (var iIA = 0; iIA < IA1.Length; iIA++)
        {
            IA1[iIA] = iIA;
            IA2[iIA] = iIA + IA1.Length;
        }

        // Index array for the 3rd submesh (lines).
        var IA3 = new int[Nx * (Ny - 1) * 6];
        var iIA3a = 0;
        var iIA3b = 0;
        for (var Iy = 0; Iy < Ny - 1; Iy++)
        {
            for (var Ix = 0; Ix < Nx; Ix++, iIA3b += 3)
            {
                IA3[iIA3a++] = iIA3b;
                IA3[iIA3a++] = iIA3b + 1;
                IA3[iIA3a++] = iIA3b;
                IA3[iIA3a++] = iIA3b + 2;
                IA3[iIA3a++] = iIA3b;
                IA3[iIA3a++] = iIA3b + IA1.Length + 2;
            }
        }

        // Construct a mesh.
        var mesh = new Mesh();
        mesh.subMeshCount = 3;
        mesh.vertices = new Vector3[TA1.Length];
        mesh.uv = TA1;
        mesh.uv2 = TA2;
        mesh.SetIndices(IA1, MeshTopology.Triangles, 0);
        mesh.SetIndices(IA2, MeshTopology.Triangles, 1);
        mesh.SetIndices(IA3, MeshTopology.Lines, 2);
        mesh.Optimize();

        // Avoid being culled.
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000);

        return mesh;
    }
}

} // namespace Kvant
