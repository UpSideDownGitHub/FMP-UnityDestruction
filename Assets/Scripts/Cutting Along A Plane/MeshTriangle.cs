using System.Collections.Generic;
using UnityEngine;

public class MeshTriangle
{
    public List<Vector3> verts = new();
    public List<Vector3> norms = new();
    public List<Vector2> uvs = new();
    public int submeshIndex;
    
    public MeshTriangle(Vector3[] _verts, Vector3[] _norms, Vector2[] _uvs, int _submeshIndex)
    {
        Clear();

        verts.AddRange(_verts);
        norms.AddRange(_norms);
        uvs.AddRange(_uvs);
        submeshIndex = _submeshIndex;
    }

    public void Clear()
    {
        verts.Clear();
        norms.Clear();
        uvs.Clear();
        submeshIndex = 0;
    }
}