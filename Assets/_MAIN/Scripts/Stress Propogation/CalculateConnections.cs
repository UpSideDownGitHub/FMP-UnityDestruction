using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityFracture.CalculateConnections;

namespace UnityFracture
{
    /// <summary>
    /// class to calculate the connections of the children of an object, this is mean to be run
    /// before runtime, to increase performance
    /// </summary>
    public class CalculateConnections : MonoBehaviour
    {
        public float epsillon = 0.01f;
        /// <summary>
        /// data structure to hold a basic line so the can be drawn to screen
        /// </summary>
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
        private List<Line> lines = new();

        /// <summary>
        /// Draws the connections as line gizmos
        /// </summary>
        public void OnDrawGizmos()
        {
            foreach (var line in lines)
            {
                Gizmos.DrawLine(line.p1, line.p2);
            }
        }

        /// <summary>
        /// Calculates the connections betwee all of the children of the object.
        /// </summary>
        public void calculateConnections()
        {
            // remove all of the previous connections between the children
            var childMeshes = gameObject.GetComponentsInChildren<MeshCollider>();
            foreach (var collider in childMeshes)
            {
                collider.gameObject.GetComponent<Connections>().RemoveAllConnections();
            }

            // looping through all of the children calculate the connections between them
            foreach (var collider in childMeshes)
            {
                // get the triangle & verts of the current child
                int triangleCount = collider.sharedMesh.triangles.Length;
                var verts = collider.sharedMesh.vertices;
                var tris = collider.sharedMesh.triangles;

                // looping through all of the found trinagles
                for (int i = 0; i < triangleCount; i += 3)
                {
                    // find the points of the current triangle
                    Vector3[] points = new Vector3[3];
                    points[0] = verts[tris[i + 0]];
                    points[1] = verts[tris[i + 1]];
                    points[2] = verts[tris[i + 2]];
                    
                    // move the points to there world position
                    collider.transform.TransformPoints(points);

                    // calculate the center of the triangle
                    var center = new Vector3((points[0].x + points[1].x + points[2].x) / 3,
                        (points[0].y + points[1].y + points[2].y) / 3,
                        (points[0].z + points[1].z + points[2].z) / 3);

                    // looping through all of the childre again
                    foreach (var collider2 in childMeshes)
                    {
                        // if is the same collider then skip
                        if (collider2 == collider)
                            continue;

                        // get the closest point on the collider from the center of the given cirlce
                        var point = collider2.ClosestPoint(center);

                        // if the distance is close to 0 (touching)
                        if (Vector3.Distance(point, center) < epsillon)
                        {
                            // if this connection does not already exist then add a new connection between the two object
                            // as well as adding the line the list of lines for debugging.
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