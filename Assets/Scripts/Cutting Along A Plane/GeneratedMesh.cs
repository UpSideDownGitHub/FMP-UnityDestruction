using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GeneratedMesh
{
    public List<Vector3> verts = new();
    public List<Vector3> norms = new();
    public List<Vector2> uvs = new();
    public List<List<int>> submeshIndexes = new();

    public void AddTriangle(MeshTriangle _triangle)
    {
        int currentVerticeCount = verts.Count;

        verts.AddRange(_triangle.verts);
        norms.AddRange(_triangle.norms);
        uvs.AddRange(_triangle.uvs);

        if (submeshIndexes.Count < _triangle.submeshIndex + 1)
        {
            for (int i = submeshIndexes.Count; i < _triangle.submeshIndex + 1; i++)
            {
                submeshIndexes.Add(new List<int>());
            }
        }

        for (int i = 0; i < 3; i++)
        {
            submeshIndexes[_triangle.submeshIndex].Add(currentVerticeCount + i);
        }
    }

    public void AddTriangle(Vector3[] _verts, Vector3[] _norms, Vector2[] _uvs, int _submeshIndex)
    {
        int currentVerticeCount = verts.Count;

        verts.AddRange(_verts);
        norms.AddRange(_norms);
        uvs.AddRange(_uvs);

        if (submeshIndexes.Count < _submeshIndex + 1)
        {
            for (int i = submeshIndexes.Count; i < _submeshIndex + 1; i++)
            {
                submeshIndexes.Add(new List<int>());
            }
        }

        for (int i = 0; i < 3; i++)
        {
            submeshIndexes[_submeshIndex].Add(currentVerticeCount + i);
        }
    }

    public Mesh GetGenerateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetNormals(norms);
        mesh.SetUVs(0, uvs);
        mesh.SetUVs(1, uvs);
        mesh.subMeshCount = submeshIndexes.Count;
        for (int i = 0; i < submeshIndexes.Count; i++)
        {
            mesh.SetTriangles(submeshIndexes[i], i);
        }
        return mesh;
    }
}