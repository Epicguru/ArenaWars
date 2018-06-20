using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGen
{
    public static Dictionary<int, Mesh> CachedMeshes = new Dictionary<int, Mesh>();

    public static Mesh Gen(int width, int height, float quadSize, bool useCached = true, bool cache = true)
    {
        int id = width * 1000 + height;
        if (useCached && CachedMeshes.ContainsKey(id))
        {
            return CachedMeshes[id];
        }

        int numTiles = width * height;
        int numTris = numTiles * 2;

        int vsize_x = width + 1;
        int vsize_z = height + 1;
        int numVerts = vsize_x * vsize_z;

        // Generate the mesh data
        Vector3[] vertices = new Vector3[numVerts];
        Vector3[] normals = new Vector3[numVerts];
        Vector2[] uv = new Vector2[numVerts];

        int[] triangles = new int[numTris * 3];

        int x, y;
        for (y = 0; y < vsize_z; y++)
        {
            for (x = 0; x < vsize_x; x++)
            {
                // Vertices...
                vertices[y * vsize_x + x] = new Vector3(x * quadSize, y * quadSize, 0);

                // Normal, facing into the camera.
                normals[y * vsize_x + x] = -Vector3.forward;

                // The UV coordinates.
                uv[y * vsize_x + x] = new Vector2((float)x / width, 1f - (float)y / height);
            }
        }

        for (y = 0; y < width; y++)
        {
            for (x = 0; x < height; x++)
            {
                // Build triangles from the constcuted triangles.
                int quadIndex = y * width + x;
                int triOffset = quadIndex * 6;
                triangles[triOffset + 1] = y * vsize_x + x + 0;
                triangles[triOffset + 2] = y * vsize_x + x + vsize_x + 0;
                triangles[triOffset + 0] = y * vsize_x + x + vsize_x + 1;

                triangles[triOffset + 4] = y * vsize_x + x + 0;
                triangles[triOffset + 5] = y * vsize_x + x + vsize_x + 1;
                triangles[triOffset + 3] = y * vsize_x + x + 1;
            }
        }

        // Create a new Mesh and populate with the data
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;

        if (cache)
        {
            // Cache it!
            CachedMeshes.Add(id, mesh);
        }

        return mesh;
    }

    public static void ClearCache()
    {
        CachedMeshes.Clear();
    }
}