using UnityEngine;

namespace ReubenMiller.Fracture
{
    /*
     * The following code is a re-written version of:
     * Greenheck, D. (2024). OpenFracture [Source Code]. Available from: https://github.com/dgreenheck/OpenFracture [Accessed May 2024].
     *
     * Unless otherwise specified
    */

    /// <summary>
    /// Class which handles slicing a mesh into two pieces given the origin and normal of the slice plane.
    /// </summary>
    public static class Slicer
    {
        /// <summary>
        /// Slices the mesh by the plane specified by `sliceNormal` and `sliceOrigin`
        /// The sliced mesh data is return via out parameters.
        /// </summary>
        /// <param name="meshData">The mesh data.</param>
        /// <param name="sliceNormal">The slice normal.</param>
        /// <param name="sliceOrigin">The slice origin.</param>
        /// <param name="topSlice">The top slice.</param>
        /// <param name="bottomSlice">The bottom slice.</param>
        public static void Slice(MeshData meshData,
                                 Vector3 sliceNormal,
                                 Vector3 sliceOrigin,
                                 out MeshData topSlice,
                                 out MeshData bottomSlice)
        {
            topSlice = new MeshData(meshData.vertexCount, meshData.triangleCount);
            bottomSlice = new MeshData(meshData.vertexCount, meshData.triangleCount);

            // Keep track of what side of the cutting plane each vertex is on
            bool[] side = new bool[meshData.vertexCount];

            // Go through and identify which vertices are above/below the split plane
            for (int i = 0; i < meshData.vertices.Count; i++)
            {
                var vertex = meshData.vertices[i];
                side[i] = vertex.position.IsAbovePlane(sliceNormal, sliceOrigin);
                var slice = side[i] ? topSlice : bottomSlice;
                slice.AddMappedVertex(vertex, i);
            }
            int offset = meshData.vertices.Count;
            for (int i = 0; i < meshData.cutvertices.Count; i++)
            {
                var vertex = meshData.cutvertices[i];
                side[i + offset] = vertex.position.IsAbovePlane(sliceNormal, sliceOrigin);
                var slice = side[i + offset] ? topSlice : bottomSlice;
                slice.AddMappedVertex(vertex, i + offset);
            }

            // split triagnles on the default & cut faces
            SplitTriangles(meshData, topSlice, bottomSlice, sliceNormal, sliceOrigin, side, 0);
            SplitTriangles(meshData, topSlice, bottomSlice, sliceNormal, sliceOrigin, side, 1);

            // Fill in the cut plane for each mesh.
            // The slice normal points to the "above" mesh, so the face normal for the cut face
            // on the above mesh is opposite of the slice normal. Conversely, normal for the
            // cut face on the "below" mesh is in the direction of the slice normal
            FillCutFaces(topSlice, bottomSlice, -sliceNormal);

        }

        /// <summary>
        /// Identifies triangles that are intersected by the slice plane and splits them in two
        /// </summary>
        /// <param name="meshData">The mesh data.</param>
        /// <param name="topSlice">The top slice.</param>
        /// <param name="bottomSlice">The bottom slice.</param>
        /// <param name="sliceNormal">The slice normal.</param>
        /// <param name="sliceOrigin">The slice origin.</param>
        /// <param name="side">The side.</param>
        /// <param name="index">The index.</param>
        public static void SplitTriangles(MeshData meshData,
                                          MeshData topSlice,
                                          MeshData bottomSlice,
                                          Vector3 sliceNormal,
                                          Vector3 sliceOrigin,
                                          bool[] side,
                                          int index)
        {
            int[] triangles = meshData.Gettriangles(index);

            // Keep track of vertices that lie on the intersection plane
            int a, b, c;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                // Get vertex indexes for this triangle
                a = triangles[i];
                b = triangles[i + 1];
                c = triangles[i + 2];

                // Triangle is contained completely within mesh A
                if (side[a] && side[b] && side[c])
                    topSlice.AddMappedTriangle(a, b, c, index);
                // Triangle is contained completely within mesh B
                else if (!side[a] && !side[b] && !side[c])
                    bottomSlice.AddMappedTriangle(a, b, c, index);
                // Triangle is intersected by the slicing plane. Need to subdivide it
                else
                {
                    // In these cases, two vertices of the triangle are above the cut plane and one vertex is below
                    if (side[b] && side[c] && !side[a])
                        SplitTriangle(b, c, a, sliceNormal, sliceOrigin, meshData, topSlice, bottomSlice, index, true);
                    else if (side[c] && side[a] && !side[b])
                        SplitTriangle(c, a, b, sliceNormal, sliceOrigin, meshData, topSlice, bottomSlice, index, true);
                    else if (side[a] && side[b] && !side[c])
                        SplitTriangle(a, b, c, sliceNormal, sliceOrigin, meshData, topSlice, bottomSlice, index, true);
                    // In these cases, two vertices of the triangle are below the cut plane and one vertex is above
                    else if (!side[b] && !side[c] && side[a])
                        SplitTriangle(b, c, a, sliceNormal, sliceOrigin, meshData, topSlice, bottomSlice, index, false);
                    else if (!side[c] && !side[a] && side[b])
                        SplitTriangle(c, a, b, sliceNormal, sliceOrigin, meshData, topSlice, bottomSlice, index, false);
                    else if (!side[a] && !side[b] && side[c])
                        SplitTriangle(a, b, c, sliceNormal, sliceOrigin, meshData, topSlice, bottomSlice, index, false);
                }
            }
        }

        /// <summary>
        /// Splits triangle defined by the points (v1,v2,v3)
        /// </summary>
        /// <param name="v1Index">Index of the v1.</param>
        /// <param name="v2Index">Index of the v2.</param>
        /// <param name="v3Index">Index of the v3.</param>
        /// <param name="sliceNormal">The slice normal.</param>
        /// <param name="sliceOrigin">The slice origin.</param>
        /// <param name="meshData">The mesh data.</param>
        /// <param name="topSlice">The top slice.</param>
        /// <param name="bottomSlice">The bottom slice.</param>
        /// <param name="index">The index.</param>
        /// <param name="v3BelowCutPlane">if set to <c>true</c> [v3 below cut plane].</param>
        public static void SplitTriangle(int v1Index,
                                         int v2Index,
                                         int v3Index,
                                         Vector3 sliceNormal,
                                         Vector3 sliceOrigin,
                                         MeshData meshData,
                                         MeshData topSlice,
                                         MeshData bottomSlice,
                                         int index,
                                         bool v3BelowCutPlane)
        {

            // - `v1`, `v2`, `v3` are the indexes of the triangle relative to the original mesh data
            // - `v1` and `v2` are on the the side of split plane that belongs to meshA
            // - `v3` is on the side of the split plane that belongs to meshB
            // - `vertices`, `normals`, `uv` are the original mesh data used for interpolation  
            //      
            // v3BelowCutPlane = true
            // ======================
            //                                
            //     v1 *_____________* v2   .
            //         \           /      /|\  cutNormal
            //          \         /        |
            //       ----*-------*---------*--
            //        v13 \     /  v23       cutOrigin
            //             \   /
            //              \ /
            //               *  v3         triangle normal out of screen                                                                                  
            //    
            // v3BelowCutPlane = false
            // =======================
            //
            //               *  v3         .                                             
            //              / \           /|\  cutNormal  
            //         v23 /   \ v13       |                    
            //       -----*-----*----------*--
            //           /       \         cut origin                                
            //          /         \                                                                  
            //      v2 *___________* v1    triangle normal out of screen


            float s13;
            float s23;
            Vector3 v13;
            Vector3 v23;

            MeshVertex v1 = v1Index < meshData.vertices.Count ? meshData.vertices[v1Index] :
                meshData.cutvertices[v1Index - meshData.vertices.Count];
            MeshVertex v2 = v2Index < meshData.vertices.Count ? meshData.vertices[v2Index] :
                meshData.cutvertices[v2Index - meshData.vertices.Count];
            MeshVertex v3 = v3Index < meshData.vertices.Count ? meshData.vertices[v3Index] :
                meshData.cutvertices[v3Index - meshData.vertices.Count];

            if (LinePlaneIntersection(v1.position, v3.position, sliceNormal, sliceOrigin, out v13, out s13) &&
                LinePlaneIntersection(v2.position, v3.position, sliceNormal, sliceOrigin, out v23, out s23))
            {
                // Interpolate normals and UV coordinates
                var norm13 = (v1.normal + s13 * (v3.normal - v1.normal)).normalized;
                var norm23 = (v2.normal + s23 * (v3.normal - v2.normal)).normalized;
                var uv13 = v1.uv + s13 * (v3.uv - v1.uv);
                var uv23 = v2.uv + s23 * (v3.uv - v2.uv);

                // Add vertices/normals/uv for the intersection points to each mesh
                topSlice.AddCutFaceVertex(v13, norm13, uv13);
                topSlice.AddCutFaceVertex(v23, norm23, uv23);
                bottomSlice.AddCutFaceVertex(v13, norm13, uv13);
                bottomSlice.AddCutFaceVertex(v23, norm23, uv23);

                // Indices for the intersection vertices (for the original mesh data)
                int index13_A = topSlice.vertices.Count - 2;
                int index23_A = topSlice.vertices.Count - 1;
                int index13_B = bottomSlice.vertices.Count - 2;
                int index23_B = bottomSlice.vertices.Count - 1;

                if (v3BelowCutPlane)
                {
                    // Triangle slice above the cutting plane is a quad, so divide into two triangles
                    topSlice.AddTriangle(index23_A, index13_A, topSlice.indexMap[v2Index], index);
                    topSlice.AddTriangle(index13_A, topSlice.indexMap[v1Index], topSlice.indexMap[v2Index], index);
                    // One triangle must be added to mesh 2
                    bottomSlice.AddTriangle(bottomSlice.indexMap[v3Index], index13_B, index23_B, index);
                    // When looking at the cut-face, the edges should wind counter-clockwise
                    topSlice.constraints.Add(new EdgeConstraint(topSlice.cutvertices.Count - 2, topSlice.cutvertices.Count - 1));
                    bottomSlice.constraints.Add(new EdgeConstraint(bottomSlice.cutvertices.Count - 1, bottomSlice.cutvertices.Count - 2));
                }
                else
                {
                    // Triangle slice above the cutting plane is a simple triangle
                    topSlice.AddTriangle(index13_A, index23_A, topSlice.indexMap[v3Index], index);

                    // Triangle slice below the cutting plane is a quad, so divide into two triangles
                    bottomSlice.AddTriangle(bottomSlice.indexMap[v1Index], bottomSlice.indexMap[v2Index], index13_B, index);
                    bottomSlice.AddTriangle(bottomSlice.indexMap[v2Index], index23_B, index13_B, index);

                    // When looking at the cut-face, the edges should wind counter-clockwise
                    topSlice.constraints.Add(new EdgeConstraint(topSlice.cutvertices.Count - 1, topSlice.cutvertices.Count - 2));
                    bottomSlice.constraints.Add(new EdgeConstraint(bottomSlice.cutvertices.Count - 2, bottomSlice.cutvertices.Count - 1));
                }
            }
        }

        /// <summary>
        /// Determines the intersection between the line segment a->b and the plane defined by the specified normal and origin point. If an intersection point exists, it is returned via the out parameter `intersection`. The parameter `s` is defined below and is used to properly interpolate normals/uvs for intersection vertices.
        /// </summary>
        /// <param name="a">Start point of line</param>
        /// <param name="b">End point of line</param>
        /// <param name="n">Plane normal</param>
        /// <param name="p0">Plane origin</param>
        /// <param name="x">If intersection exists, intersection point return as out parameter.</param>
        /// <param name="s">Returns the parameterization of the intersection where x = a + (b - a) * s</param>
        /// <returns></returns>
        public static bool LinePlaneIntersection(Vector3 a,
                                             Vector3 b,
                                             Vector3 n,
                                             Vector3 p0,
                                             out Vector3 x,
                                             out float s)
        {
            // Initialize out params
            s = 0;
            x = Vector3.zero;

            // Handle degenerate cases
            if (a == b)
                return false;
            else if (n == Vector3.zero)
                return false;

            // `s` is the parameter for the line segment a -> b where 0.0 <= s <= 1.0
            s = Vector3.Dot(p0 - a, n) / Vector3.Dot(b - a, n);
            if (s >= 0 && s <= 1)
            {
                x = a + (b - a) * s;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Fills the cut faces for each sliced mesh. The `sliceNormal` is the normal for the plane and points
        /// in the direction of `topMeshData`
        /// </summary>
        /// <param name="topSlice">The top slice.</param>
        /// <param name="bottomSlice">The bottom slice.</param>
        /// <param name="sliceNormal">The slice normal.</param>
        public static void FillCutFaces(MeshData topSlice,
                                        MeshData bottomSlice,
                                        Vector3 sliceNormal)
        {
            // Since the topSlice and bottomSlice both share the same cut face, we only need to calculate it
            // once. Then the same vertex/triangle data for the face will be used for both slices, except
            // with the normals reversed.

            // First need to weld the coincident vertices for the triangulation to work properly
            topSlice.WeldCutFacevertices();

            // Need at least 3 vertices to triangulate
            if (topSlice.cutvertices.Count < 3) return;

            // Triangulate the cut face
            var triangulator = new ConstrainedTriangluator(topSlice.cutvertices, topSlice.constraints, sliceNormal);
            int[] triangles = triangulator.Triangulate();

            // Update normal and UV for the cut face vertices
            for (int i = 0; i < topSlice.cutvertices.Count; i++)
            {
                var vertex = topSlice.cutvertices[i];
                var point = triangulator.points[i];

                // UV coordinates are based off of the 2D coordinates used for triangulation
                // During triangulation, coordinates are normalized to [0,1], so need to multiply
                // by normalization scale factor to get back to the appropritate scale
                Vector2 uv = new Vector2(
                    (triangulator.normalizationScaleFactor * point.coords.x),
                    (triangulator.normalizationScaleFactor * point.coords.y));

                // Update normals and UV coordinates for the cut vertices
                var topVertex = vertex;
                topVertex.normal = sliceNormal;
                topVertex.uv = uv;

                var bottomVertex = vertex;
                bottomVertex.normal = -sliceNormal;
                bottomVertex.uv = uv;

                topSlice.cutvertices[i] = topVertex;
                bottomSlice.cutvertices[i] = bottomVertex;
            }

            // Add the new triangles to the top/bottom slices
            int offsetTop = topSlice.vertices.Count;
            int offsetBottom = bottomSlice.vertices.Count;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                topSlice.AddTriangle(
                    offsetTop + triangles[i],
                    offsetTop + triangles[i + 1],
                    offsetTop + triangles[i + 2],
                    0);
                // Swap two vertices so triangles are wound CW
                bottomSlice.AddTriangle(
                    offsetBottom + triangles[i],
                    offsetBottom + triangles[i + 2], 
                    offsetBottom + triangles[i + 1],
                    1);
            }
        }
        /// <summary>
        /// Determines whether [is above plane] [the specified n].
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="n">The n.</param>
        /// <param name="o">The o.</param>
        /// <returns>
        ///   <c>true</c> if [is above plane] [the specified n]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAbovePlane(this Vector3 p, Vector3 n, Vector3 o)
        {
            return (n.x * (p.x - o.x) + n.y * (p.y - o.y) + n.z * (p.z - o.z)) >= 0;
        }
    }
}