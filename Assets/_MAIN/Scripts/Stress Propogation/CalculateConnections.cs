using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityFracture.CalculateConnections;

namespace UnityFracture
{
    public class CalculateConnections : MonoBehaviour
    {
        public float epsillon = 0.1f;
        [Serializable]
        public struct Line
        {
            public Line(Vector3 p1, Vector3 p2)
            {
                this.p1 = p1;
                this.p2 = p2;
            }
            public Vector3 p1;
            public Vector3 p2;
        }
        public List<Line> lines = new();

        public void OnDrawGizmos()
        {
            foreach (var line in lines)
            {
                Gizmos.DrawLine(line.p1, line.p2);
            }
        }

        public void calculateConnections()
        {

            var childMeshes = gameObject.GetComponentsInChildren<MeshCollider>();
            foreach (var collider in childMeshes)
            {
                int triangleCount = collider.sharedMesh.triangles.Length;
                var verts = collider.sharedMesh.vertices;
                var tris = collider.sharedMesh.triangles;

                for (int i = 0; i < triangleCount; i += 3)
                {
                    Vector3[] points = new Vector3[3];
                    for (int pointNum = 0; pointNum < 3; pointNum++)
                    {
                        points[pointNum] = verts[tris[i + pointNum]];
                    }
                    
                    collider.transform.TransformPoints(points);

                    var center = new Vector3((points[0].x + points[1].x + points[2].x) / 3,
                        (points[0].y + points[1].y + points[2].y) / 3,
                        (points[0].z + points[1].z + points[2].z) / 3);

                    foreach (var collider2 in childMeshes)
                    {
                        if (collider2 == collider)
                            continue;

                        var point = collider2.ClosestPoint(center);

                        if (Vector3.Distance(point, center) < epsillon)
                        {
                            if (!collider.gameObject.GetComponent<Connections>().HasConnection(collider2.gameObject.GetComponent<Connections>()))
                            {
                                lines.Add(new Line(point, center ));
                                collider.gameObject.GetComponent<Connections>().AddConnection(collider2.gameObject.GetComponent<Connections>()); 
                            }
                            if (!collider2.gameObject.GetComponent<Connections>().HasConnection(collider.gameObject.GetComponent<Connections>()))
                            {
                                collider2.gameObject.GetComponent<Connections>().AddConnection(collider.gameObject.GetComponent<Connections>());
                            }
                        }
                    }
                }
            }

                /*
                // get all children of the parent, these are the peices that need to be connected
                var childMeshes = gameObject.GetComponentsInChildren<MeshFilter>();
                foreach (var filter in childMeshes)
                {
                    // loop through all of the triangles of this sharedMesh
                    int triangleCount = filter.sharedMesh.triangles.Length;
                    for (int i = 0; i < triangleCount; i += 3)
                    {
                        // calcualte the plan of the current triangles
                        Plane plane = new Plane(filter.sharedMesh.vertices[filter.sharedMesh.triangles[i]],
                            filter.sharedMesh.vertices[filter.sharedMesh.triangles[i + 1]],
                            filter.sharedMesh.vertices[filter.sharedMesh.triangles[i + 2]]);

                        foreach (var filter2 in childMeshes)
                        {
                            if (filter2 == filter)
                                continue;

                            // loop through all of the triangles of this sharedMesh
                            int triangleCount2 = filter2.sharedMesh.triangles.Length;
                            for (int j = 0; j < triangleCount2; j += 3)
                            {
                                // calculate the plane of the triagnle
                                Plane plane2 = new Plane(filter2.sharedMesh.vertices[filter2.sharedMesh.triangles[j]],
                                    filter2.sharedMesh.vertices[filter2.sharedMesh.triangles[j + 1]],
                                    filter2.sharedMesh.vertices[filter2.sharedMesh.triangles[j + 2]]);

                                // if the two triangles lie on the same plane then check for intersection
                                if (plane == plane2)
                                {
                                    Vector3[] points = new Vector3[3] { filter2.sharedMesh.vertices[filter2.sharedMesh.triangles[j]],
                                        filter2.sharedMesh.vertices[filter2.sharedMesh.triangles[j + 1]],
                                        filter2.sharedMesh.vertices[filter2.sharedMesh.triangles[j + 2]]};

                                    for (int k = 0; k < 3; k++)
                                    {
                                        // Compute vectors        
                                        var v0 = filter.sharedMesh.vertices[filter.sharedMesh.triangles[i + 2]] - filter.sharedMesh.vertices[filter.sharedMesh.triangles[i]];
                                        var v1 = filter.sharedMesh.vertices[filter.sharedMesh.triangles[i + 1]] - filter.sharedMesh.vertices[filter.sharedMesh.triangles[i]];
                                        var v2 = points[k] - filter.sharedMesh.vertices[filter.sharedMesh.triangles[j]];

                                        // Compute dot products
                                        var dot00 = Vector3.Dot(v0, v0);
                                        var dot01 = Vector3.Dot(v0, v1);
                                        var dot02 = Vector3.Dot(v0, v2);
                                        var dot11 = Vector3.Dot(v1, v1);
                                        var dot12 = Vector3.Dot(v1, v2);

                                        // Compute barycentric coordinates
                                        var invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
                                        var u = (dot11 * dot02 - dot01 * dot12) * invDenom;
                                        var v = (dot00 * dot12 - dot01 * dot02) * invDenom;

                                        // Check if point is in triangle
                                        if ((u >= 0) && (v >= 0) && (u + v < 1))
                                        {
                                            // point lies within the center so add connections
                                            // TODO | OPTIMIZE
                                            //  - this has to do all of the math before hand to then find out that it has already been done, not very good
                                            //  - might be a good place for optimization.
                                            if (!filter.gameObject.GetComponent<Connections>().HasConnection(filter2.gameObject.GetComponent<Connections>()))
                                                filter.gameObject.GetComponent<Connections>().AddConnection(filter2.gameObject.GetComponent<Connections>());
                                            if (!filter2.gameObject.GetComponent<Connections>().HasConnection(filter.gameObject.GetComponent<Connections>()))
                                                filter2.gameObject.GetComponent<Connections>().AddConnection(filter.gameObject.GetComponent<Connections>());
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                */
            }
    }
}