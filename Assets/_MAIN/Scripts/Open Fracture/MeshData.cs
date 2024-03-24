using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshData
{
    public List<MeshVertex> vertices;
    public List<MeshVertex> cutvertices;
    public List<int>[] triangles;
    public List<EdgeConstraint> constraints;
    public int[] indexMap;
    public Bounds bounds;

    public int triangleCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < this.triangles.Length; i++)
            {
                count += this.triangles[i].Count;
            }
            return count;
        }
    }

    public int vertexCount
    {
        get
        {
            return this.vertices.Count + this.cutvertices.Count;
        }
    }

    public MeshData(int vertexCount, int triangleCount)
    {
        this.vertices = new List<MeshVertex>(vertexCount);
        this.cutvertices = new List<MeshVertex>(vertexCount / 10);

        // Store triangles for each submesh separately
        this.triangles = new List<int>[] {
            new List<int>(triangleCount),
            new List<int>(triangleCount / 10)
        };

        this.constraints = new List<EdgeConstraint>();
        this.indexMap = new int[vertexCount];
    }

    public MeshData(Mesh mesh)
    {
        var positions = mesh.vertices;
        var normals = mesh.normals;
        var uv = mesh.uv;

        this.vertices = new List<MeshVertex>(mesh.vertexCount);
        this.cutvertices = new List<MeshVertex>(mesh.vertexCount / 10);
        this.constraints = new List<EdgeConstraint>();
        this.indexMap = new int[positions.Length];

        for (int i = 0; i < positions.Length; i++)
        {
            this.vertices.Add(new MeshVertex(positions[i], normals[i], uv[i]));
        }

        this.triangles = new List<int>[2];
        this.triangles[0] = new List<int>(mesh.GetTriangles(0));

        if (mesh.subMeshCount >= 2)
        {
            this.triangles[1] = new List<int>(mesh.GetTriangles(1));
        }
        else
        {
            this.triangles[1] = new List<int>(mesh.triangles.Length / 10);
        }

        this.Calculatebounds();
    }

    public void AddCutFaceVertex(Vector3 position, Vector3 normal, Vector2 uv)
    {
        var vertex = new MeshVertex(position, normal, uv);
        this.vertices.Add(vertex);
        this.cutvertices.Add(vertex);
    }

    public void AddMappedVertex(MeshVertex vertex, int sourceIndex)
    {
        this.vertices.Add(vertex);
        this.indexMap[sourceIndex] = this.vertices.Count - 1;
    }

    public void AddTriangle(int v1, int v2, int v3, int subMesh)
    {
        this.triangles[subMesh].Add(v1);
        this.triangles[subMesh].Add(v2);
        this.triangles[subMesh].Add(v3);
    }

    public void AddMappedTriangle(int v1, int v2, int v3, int subMesh)
    {
        this.triangles[subMesh].Add(indexMap[v1]);
        this.triangles[subMesh].Add(indexMap[v2]);
        this.triangles[subMesh].Add(indexMap[v3]);
    }

    public void WeldCutFacevertices()
    {
        List<MeshVertex> weldedVerts = new List<MeshVertex>(cutvertices.Count);
        int[] indexMap = new int[cutvertices.Count];
        int k = 0;

        for (int i = 0; i < cutvertices.Count; i++)
        {
            bool duplicate = false;
            for (int j = 0; j < weldedVerts.Count; j++)
            {
                if (cutvertices[i].position == weldedVerts[j].position)
                {
                    indexMap[i] = j;
                    duplicate = true;
                    break;
                }
            }

            if (!duplicate)
            {
                weldedVerts.Add(cutvertices[i]);
                indexMap[i] = k;
                k++;
            }
        }

        for (int i = 0; i < constraints.Count; i++)
        {
            var edge = constraints[i];
            edge.v1 = indexMap[edge.v1];
            edge.v2 = indexMap[edge.v2];
        }

        weldedVerts.TrimExcess();

        this.cutvertices = new List<MeshVertex>(weldedVerts);
    }

    public int[] Gettriangles(int subMeshIndex)
    {
        return this.triangles[subMeshIndex].ToArray();
    }

    public void Calculatebounds()
    {
        float vertexCount = (float)vertices.Count;
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        foreach (MeshVertex vertex in vertices)
        {
            if (vertex.position.x < min.x) min.x = vertex.position.x;
            if (vertex.position.y < min.y) min.y = vertex.position.y;
            if (vertex.position.z < min.z) min.z = vertex.position.z;
            if (vertex.position.x > max.x) max.x = vertex.position.x;
            if (vertex.position.y > max.y) max.y = vertex.position.y;
            if (vertex.position.z > max.z) max.z = vertex.position.z;
        }

        this.bounds = new Bounds((max + min) / 2f, max - min);
    }

    public Mesh ToMesh()
    {
        Mesh mesh = new Mesh();
        var layout = new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
        };
        mesh.SetIndexBufferParams(triangleCount, IndexFormat.UInt32);
        mesh.SetVertexBufferParams(vertexCount, layout);
        mesh.SetVertexBufferData(vertices, 0, 0, vertices.Count);
        mesh.SetVertexBufferData(cutvertices, 0, vertices.Count, cutvertices.Count);

        mesh.subMeshCount = triangles.Length;
        int indexStart = 0;
        for (int i = 0; i < triangles.Length; i++)
        {
            var subMeshIndexBuffer = triangles[i];
            mesh.SetIndexBufferData(subMeshIndexBuffer, 0, indexStart, subMeshIndexBuffer.Count);
            mesh.SetSubMesh(i, new SubMeshDescriptor(indexStart, subMeshIndexBuffer.Count));
            indexStart += subMeshIndexBuffer.Count;
        }

        mesh.RecalculateBounds();
        return mesh;
    }
}