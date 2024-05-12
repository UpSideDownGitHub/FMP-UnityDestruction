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
        }
    }
}