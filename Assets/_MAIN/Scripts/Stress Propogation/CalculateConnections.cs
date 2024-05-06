using System;
using UnityEngine;

namespace UnityFracture
{
    public class CalculateConnections
    {
        public void calculateConnections(GameObject parentObj)
        {
            // get all children of the parent, these are the peices that need to be connected
            var childMeshes = parentObj.GetComponentsInChildren<MeshFilter>();
            foreach (var filter in childMeshes)
            {
                // loop through all of the triangles of this mesh
                int triangleCount = filter.mesh.triangles.Length;
                for (int i = 0; i < triangleCount; i += 3)
                {
                    // calcualte the plan of the current triangles
                    Plane plane = new Plane(filter.mesh.vertices[filter.mesh.triangles[i]],
                        filter.mesh.vertices[filter.mesh.triangles[i + 1]],
                        filter.mesh.vertices[filter.mesh.triangles[i + 2]]);

                    foreach (var filter2 in childMeshes)
                    {
                        if (filter2 == filter)
                            continue;

                        // loop through all of the triangles of this mesh
                        int triangleCount2 = filter.mesh.triangles.Length;
                        for (int j = 0; j < triangleCount; j += 3)
                        {
                            // calculate the plane of the triagnle
                            Plane plane2 = new Plane(filter2.mesh.vertices[filter2.mesh.triangles[j]],
                                filter2.mesh.vertices[filter2.mesh.triangles[j + 1]],
                                filter2.mesh.vertices[filter2.mesh.triangles[j + 2]]);

                            // if the two triangles lie on the same plane then check for intersection
                            if (plane == plane2)
                            {
                                Vector3[] points = new Vector3[3] { filter2.mesh.vertices[filter2.mesh.triangles[j]],
                                    filter2.mesh.vertices[filter2.mesh.triangles[j + 1]],
                                    filter2.mesh.vertices[filter2.mesh.triangles[j + 2]]};

                                for (int k = 0; k < 3; k++)
                                {
                                    // Compute vectors        
                                    var v0 = filter.mesh.vertices[filter.mesh.triangles[j + 2]] - filter.mesh.vertices[filter.mesh.triangles[j]];
                                    var v1 = filter.mesh.vertices[filter.mesh.triangles[j + 1]] - filter.mesh.vertices[filter.mesh.triangles[j]];
                                    var v2 = points[k] - filter.mesh.vertices[filter.mesh.triangles[j]];

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
        }
    }
}