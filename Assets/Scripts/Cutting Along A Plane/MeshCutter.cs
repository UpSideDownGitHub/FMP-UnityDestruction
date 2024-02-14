using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MeshCutter : MonoBehaviour
{
    public bool isBusy;
    public Mesh ogMesh;

    public void Cut(GameObject _ogGameObject, Vector3 contactPoint, Vector3 cutNormal)
    {
        if (isBusy) { return; }

        isBusy = true;

        Plane cutPlane = new Plane(_ogGameObject.transform.InverseTransformDirection(-cutNormal), 
            _ogGameObject.transform.InverseTransformPoint(contactPoint));

        ogMesh = _ogGameObject.GetComponent<MeshFilter>().mesh;

        if (ogMesh == null){ return; }

        List<Vector3> addedVertices = new List<Vector3>();
        GeneratedMesh leftMesh = new GeneratedMesh();
        GeneratedMesh rightMesh = new GeneratedMesh();

        SeperateMeshes(leftMesh, rightMesh, cutPlane, addedVertices);
        FillCut(addedVertices, cutPlane, leftMesh, rightMesh);

        Mesh finishedLeftMesh = leftMesh.GetGenerateMesh();
        Mesh finishedRightMesh = rightMesh.GetGenerateMesh();

        //Getting and destroying all original colliders to prevent having multiple colliders
        //of different kinds on one object
        var originalCols = _ogGameObject.GetComponents<Collider>();
        foreach (var col in originalCols)
            Destroy(col);

        _ogGameObject.GetComponent<MeshFilter>().mesh = finishedLeftMesh;
        var collider = _ogGameObject.AddComponent<MeshCollider>();
        collider.sharedMesh = finishedLeftMesh;
        collider.convex = true;

        Material[] mats = new Material[finishedLeftMesh.subMeshCount];
        for (int i = 0; i < finishedLeftMesh.subMeshCount; i++)
        {
            mats[i] = _ogGameObject.GetComponent<MeshRenderer>().material;
        }
        _ogGameObject.GetComponent<MeshRenderer>().materials = mats;

        GameObject right = new GameObject();
        right.transform.position = _ogGameObject.transform.position + (Vector3.up * .05f);
        right.transform.rotation = _ogGameObject.transform.rotation;
        right.transform.localScale = _ogGameObject.transform.localScale;
        right.AddComponent<MeshRenderer>();

        mats = new Material[finishedRightMesh.subMeshCount];
        for (int i = 0; i < finishedRightMesh.subMeshCount; i++)
        {
            mats[i] = _ogGameObject.GetComponent<MeshRenderer>().material;
        }
        right.GetComponent<MeshRenderer>().materials = mats;
        right.AddComponent<MeshFilter>().mesh = finishedRightMesh;

        right.AddComponent<MeshCollider>().sharedMesh = finishedRightMesh;
        var cols = right.GetComponents<MeshCollider>();
        foreach (var col in cols)
        {
            col.convex = true;
        }

        var rightRigidbody = right.AddComponent<Rigidbody>();
        rightRigidbody.AddRelativeForce(-cutPlane.normal * 250f);

        isBusy = false;
    }

    public void SeperateMeshes(GeneratedMesh leftMesh, GeneratedMesh rightMesh, Plane plane, List<Vector3> addedVerts)
    {
        for (int i = 0; i < ogMesh.subMeshCount; i += 3)
        {
            var submeshIndexes = ogMesh.GetTriangles(i);

            for (int j = 0; j < submeshIndexes.Length; j+=3)
            {
                var triangleIndexA = submeshIndexes[j];
                var triangleIndexB = submeshIndexes[j + 1];
                var triangleIndexC = submeshIndexes[j + 2];

                MeshTriangle currentTriangle = GetTriangle(triangleIndexA, triangleIndexB, triangleIndexC, i);

                //We are now using the plane.getside function to see on which side of the cut our trianle is situated 
                //or if it might be cut through
                bool triangleALeftSide = plane.GetSide(ogMesh.vertices[triangleIndexA]);
                bool triangleBLeftSide = plane.GetSide(ogMesh.vertices[triangleIndexB]);
                bool triangleCLeftSide = plane.GetSide(ogMesh.vertices[triangleIndexC]);

                switch (triangleALeftSide)
                {
                    //All three verts are on the left side of the plane, so they need to be added to the left
                    //mesh
                    case true when triangleBLeftSide && triangleCLeftSide:
                        leftMesh.AddTriangle(currentTriangle);
                        break;
                    //All three verts are on the right side of the mesh.
                    case false when !triangleBLeftSide && !triangleCLeftSide:
                        rightMesh.AddTriangle(currentTriangle);
                        break;
                    default:
                        CutTriangle(plane, currentTriangle, triangleALeftSide, triangleBLeftSide, triangleCLeftSide, leftMesh, rightMesh, addedVerts);
                        break;
                }
            }
        }
    }

    public MeshTriangle GetTriangle(int _triangleIndexA, int _triangleIndexB, int _triangleIndexC, int _submeshIndex)
    {
        Vector3[] vertsToAdd = {
            ogMesh.vertices[_triangleIndexA],
            ogMesh.vertices[_triangleIndexB],
            ogMesh.vertices[_triangleIndexC]
        };
        Vector3[] normsToAdd = {
            ogMesh.normals[_triangleIndexA],
            ogMesh.normals[_triangleIndexB],
            ogMesh.normals[_triangleIndexC]
        };
        Vector2[] uvsToAdd = {
            ogMesh.uv[_triangleIndexA],
            ogMesh.uv[_triangleIndexB],
            ogMesh.uv[_triangleIndexC]
        };

        return new MeshTriangle(vertsToAdd, normsToAdd, uvsToAdd, _submeshIndex);
    }

    public void CutTriangle(Plane plane, MeshTriangle triangle, bool triangleALeftSide, bool triangleBLeftSide, bool triangleCLeftSide,
    GeneratedMesh leftMesh, GeneratedMesh rightMesh, List<Vector3> addedverts)
    {
        List<bool> leftSide = new List<bool>();
        leftSide.Add(triangleALeftSide);
        leftSide.Add(triangleBLeftSide);
        leftSide.Add(triangleCLeftSide);

        MeshTriangle leftMeshTriangle = new MeshTriangle(new Vector3[2], new Vector3[2], new Vector2[2], triangle.submeshIndex);
        MeshTriangle rightMeshTriangle = new MeshTriangle(new Vector3[2], new Vector3[2], new Vector2[2], triangle.submeshIndex);

        bool left = false;
        bool right = false;

        for (int i = 0; i < 3; i++)
        {
            if (leftSide[i])
            {
                if (!left)
                {
                    left = true;

                    leftMeshTriangle.verts[0] = triangle.verts[i];
                    leftMeshTriangle.verts[1] = leftMeshTriangle.verts[0];

                    leftMeshTriangle.uvs[0] = triangle.uvs[i];
                    leftMeshTriangle.uvs[1] = leftMeshTriangle.uvs[0];

                    leftMeshTriangle.norms[0] = triangle.norms[i];
                    leftMeshTriangle.norms[1] = leftMeshTriangle.norms[0];
                }
                else
                {
                    leftMeshTriangle.verts[1] = triangle.verts[i];
                    leftMeshTriangle.norms[1] = triangle.norms[i];
                    leftMeshTriangle.uvs[1] = triangle.uvs[i];
                }
            }
            else
            {
                if (!right)
                {
                    right = true;

                    rightMeshTriangle.verts[0] = triangle.verts[i];
                    rightMeshTriangle.verts[1] = rightMeshTriangle.verts[0];

                    rightMeshTriangle.uvs[0] = triangle.uvs[i];
                    rightMeshTriangle.uvs[1] = rightMeshTriangle.uvs[0];

                    rightMeshTriangle.norms[0] = triangle.norms[i];
                    rightMeshTriangle.norms[1] = rightMeshTriangle.norms[0];

                }
                else
                {
                    rightMeshTriangle.verts[1] = triangle.verts[i];
                    rightMeshTriangle.norms[1] = triangle.norms[i];
                    rightMeshTriangle.uvs[1] = triangle.uvs[i];
                }
            }
        }

        float normalizedDistance;
        float distance;
        plane.Raycast(new Ray(leftMeshTriangle.verts[0], (rightMeshTriangle.verts[0] - leftMeshTriangle.verts[0]).normalized), out distance);

        normalizedDistance = distance / (rightMeshTriangle.verts[0] - leftMeshTriangle.verts[0]).magnitude;
        Vector3 vertLeft = Vector3.Lerp(leftMeshTriangle.verts[0], rightMeshTriangle.verts[0], normalizedDistance);
        addedverts.Add(vertLeft);

        Vector3 normalLeft = Vector3.Lerp(leftMeshTriangle.norms[0], rightMeshTriangle.norms[0], normalizedDistance);
        Vector2 uvLeft = Vector2.Lerp(leftMeshTriangle.uvs[0], rightMeshTriangle.uvs[0], normalizedDistance);

        plane.Raycast(new Ray(leftMeshTriangle.verts[1], (rightMeshTriangle.verts[1] - leftMeshTriangle.verts[1]).normalized), out distance);

        normalizedDistance = distance / (rightMeshTriangle.verts[1] - leftMeshTriangle.verts[1]).magnitude;
        Vector3 vertRight = Vector3.Lerp(leftMeshTriangle.verts[1], rightMeshTriangle.verts[1], normalizedDistance);
        addedverts.Add(vertRight);

        Vector3 normalRight = Vector3.Lerp(leftMeshTriangle.norms[1], rightMeshTriangle.norms[1], normalizedDistance);
        Vector2 uvRight = Vector2.Lerp(leftMeshTriangle.uvs[1], rightMeshTriangle.uvs[1], normalizedDistance);

        //TESTING OUR FIRST TRIANGLE
        MeshTriangle currentTriangle;
        Vector3[] updatedverts = { leftMeshTriangle.verts[0], vertLeft, vertRight };
        Vector3[] updatednorms = { leftMeshTriangle.norms[0], normalLeft, normalRight };
        Vector2[] updateduvs = { leftMeshTriangle.uvs[0], uvLeft, uvRight };

        currentTriangle = new MeshTriangle(updatedverts, updatednorms, updateduvs, triangle.submeshIndex);

        //If our verts ant the same
        if (updatedverts[0] != updatedverts[1] && updatedverts[0] != updatedverts[2])
        {
            if (Vector3.Dot(Vector3.Cross(updatedverts[1] - updatedverts[0], updatedverts[2] - updatedverts[0]), updatednorms[0]) < 0)
            {
                FlipTriangle(currentTriangle);
            }
            leftMesh.AddTriangle(currentTriangle);
        }

        //SECOND TRIANGLE 
        updatedverts = new Vector3[] { leftMeshTriangle.verts[0], leftMeshTriangle.verts[1], vertRight };
        updatednorms = new Vector3[] { leftMeshTriangle.norms[0], leftMeshTriangle.norms[1], normalRight };
        updateduvs = new Vector2[] { leftMeshTriangle.uvs[0], leftMeshTriangle.uvs[1], uvRight };


        currentTriangle = new MeshTriangle(updatedverts, updatednorms, updateduvs, triangle.submeshIndex);
        //If our verts arent the same
        if (updatedverts[0] != updatedverts[1] && updatedverts[0] != updatedverts[2])
        {
            if (Vector3.Dot(Vector3.Cross(updatedverts[1] - updatedverts[0], updatedverts[2] - updatedverts[0]), updatednorms[0]) < 0)
            {
                FlipTriangle(currentTriangle);
            }
            leftMesh.AddTriangle(currentTriangle);
        }

        //THIRD TRIANGLE 
        updatedverts = new Vector3[] { rightMeshTriangle.verts[0], vertLeft, vertRight };
        updatednorms = new Vector3[] { rightMeshTriangle.norms[0], normalLeft, normalRight };
        updateduvs = new Vector2[] { rightMeshTriangle.uvs[0], uvLeft, uvRight };

        currentTriangle = new MeshTriangle(updatedverts, updatednorms, updateduvs, triangle.submeshIndex);
        //If our verts arent the same
        if (updatedverts[0] != updatedverts[1] && updatedverts[0] != updatedverts[2])
        {
            if (Vector3.Dot(Vector3.Cross(updatedverts[1] - updatedverts[0], updatedverts[2] - updatedverts[0]), updatednorms[0]) < 0)
            {
                FlipTriangle(currentTriangle);
            }
            rightMesh.AddTriangle(currentTriangle);
        }

        //FOURTH TRIANGLE 
        updatedverts = new Vector3[] { rightMeshTriangle.verts[0], rightMeshTriangle.verts[1], vertRight };
        updatednorms = new Vector3[] { rightMeshTriangle.norms[0], rightMeshTriangle.norms[1], normalRight };
        updateduvs = new Vector2[] { rightMeshTriangle.uvs[0], rightMeshTriangle.uvs[1], uvRight };

        currentTriangle = new MeshTriangle(updatedverts, updatednorms, updateduvs, triangle.submeshIndex);
        //If our verts arent the same
        if (updatedverts[0] != updatedverts[1] && updatedverts[0] != updatedverts[2])
        {
            if (Vector3.Dot(Vector3.Cross(updatedverts[1] - updatedverts[0], updatedverts[2] - updatedverts[0]), updatednorms[0]) < 0)
            {
                FlipTriangle(currentTriangle);
            }
            rightMesh.AddTriangle(currentTriangle);
        }
    }

    private void FlipTriangle(MeshTriangle _triangle)
    {
        Vector3 temp = _triangle.verts[2];
        _triangle.verts[2] = _triangle.verts[0];
        _triangle.verts[0] = temp;

        temp = _triangle.norms[2];
        _triangle.norms[2] = _triangle.norms[0];
        _triangle.norms[0] = temp;

        (_triangle.uvs[2], _triangle.uvs[0]) = (_triangle.uvs[0], _triangle.uvs[2]);
    }

    public void FillCut(List<Vector3> _addedverts, Plane _plane, GeneratedMesh _leftMesh, GeneratedMesh _rightMesh)
    {
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> polygon = new List<Vector3>();

        for (int i = 0; i < _addedverts.Count; i++)
        {
            if (!verts.Contains(_addedverts[i]))
            {
                polygon.Clear();
                polygon.Add(_addedverts[i]);
                polygon.Add(_addedverts[i + 1]);

                verts.Add(_addedverts[i]);
                verts.Add(_addedverts[i + 1]);

                EvaluatePairs(_addedverts, verts, polygon);
                Fill(polygon, _plane, _leftMesh, _rightMesh);
            }
        }
    }

    public void EvaluatePairs(List<Vector3> _addedverts, List<Vector3> _verts, List<Vector3> _polygone)
    {
        bool isDone = false;
        while (!isDone)
        {
            isDone = true;
            for (int i = 0; i < _addedverts.Count; i += 2)
            {
                if (_addedverts[i] == _polygone[_polygone.Count - 1] && !_verts.Contains(_addedverts[i + 1]))
                {
                    isDone = false;
                    _polygone.Add(_addedverts[i + 1]);
                    _verts.Add(_addedverts[i + 1]);
                }
                else if (_addedverts[i + 1] == _polygone[_polygone.Count - 1] && !_verts.Contains(_addedverts[i]))
                {
                    isDone = false;
                    _polygone.Add(_addedverts[i]);
                    _verts.Add(_addedverts[i]);
                }
            }
        }
    }

    private void Fill(List<Vector3> _verts, Plane _plane, GeneratedMesh _leftMesh, GeneratedMesh _rightMesh)
    {
        //Firstly we need the center we do this by adding up all the verts and then calculating the average
        Vector3 centerPosition = Vector3.zero;
        for (int i = 0; i < _verts.Count; i++)
        {
            centerPosition += _verts[i];
        }
        centerPosition /= _verts.Count;

        //We now need an Upward Axis we use the plane we cut the mesh with for that 
        Vector3 up = new Vector3()
        {
            x = _plane.normal.x,
            y = _plane.normal.y,
            z = _plane.normal.z
        };

        Vector3 left = Vector3.Cross(_plane.normal, up);

        Vector3 displacement = Vector3.zero;
        Vector2 uv1 = Vector2.zero;
        Vector2 uv2 = Vector2.zero;

        for (int i = 0; i < _verts.Count; i++)
        {
            displacement = _verts[i] - centerPosition;
            uv1 = new Vector2()
            {
                x = .5f + Vector3.Dot(displacement, left),
                y = .5f + Vector3.Dot(displacement, up)
            };

            displacement = _verts[(i + 1) % _verts.Count] - centerPosition;
            uv2 = new Vector2()
            {
                x = .5f + Vector3.Dot(displacement, left),
                y = .5f + Vector3.Dot(displacement, up)
            };

            Vector3[] verts = { _verts[i], _verts[(i + 1) % _verts.Count], centerPosition };
            Vector3[] normals = { -_plane.normal, -_plane.normal, -_plane.normal };
            Vector2[] uvs = { uv1, uv2, new(0.5f, 0.5f) };

            MeshTriangle currentTriangle = new MeshTriangle(verts, normals, uvs, ogMesh.subMeshCount + 1);

            if (Vector3.Dot(Vector3.Cross(verts[1] - verts[0], verts[2] - verts[0]), normals[0]) < 0)
            {
                FlipTriangle(currentTriangle);
            }
            _leftMesh.AddTriangle(currentTriangle);

            normals = new[] { _plane.normal, _plane.normal, _plane.normal };
            currentTriangle = new MeshTriangle(verts, normals, uvs, ogMesh.subMeshCount + 1);

            if (Vector3.Dot(Vector3.Cross(verts[1] - verts[0], verts[2] - verts[0]), normals[0]) < 0)
            {
                FlipTriangle(currentTriangle);
            }
            _rightMesh.AddTriangle(currentTriangle);

        }
    }
}