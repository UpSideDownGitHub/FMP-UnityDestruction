using System.Collections.Generic;
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
    /// Class for triangulating a set of 3D points with edge constraints. Supports convex and non-convex polygons
    /// as well as polygons with holes.
    /// </summary>
    public class ConstrainedTriangluator 
    {
        // Vertexs 1, 2, 3
        protected const int V1 = 0; 
        protected const int V2 = 1; 
        protected const int V3 = 2;
        // Adjacency data for edge
        // E12 -> (V1 -> V2)
        // E23 -> (V2 -> V3)
        // E31 -> (V3 -> V1)
        protected const int E12 = 3; 
        protected const int E23 = 4; 
        protected const int E31 = 5; 
        
        // Given the edge E12, E23, E31 the following will return the...
        // First Vertex (V1, V2, V3)
        private static readonly int[] edgeVertex1 = new int[] { 0, 0, 0, V1, V2, V3 };
        // Second vertex (V2, V3, V1)
        private static readonly int[] edgeVertex2 = new int[] { 0, 0, 0, V2, V3, V1 };
        // Opposite Edge (V3, V1, V2)
        private static readonly int[] oppositePoint = new int[] { 0, 0, 0, V3, V1, V2 };
        // Next Clockwise Edge (E23, E31, E12)
        private static readonly int[] nextEdge = new int[] { 0, 0, 0, E23, E31, E12 };
        // Previous Clockwise Edge (E31, E12, E23)
        private static readonly int[] previousEdge = new int[] { 0, 0, 0, E31, E12, E23 };
        
        // index of the suprt triangle
        protected const int SUPERTRIANGLE = 0;
        // index of the boundary edge
        protected const int OUT_OF_BOUNDS = -1;
        
        // number of points to be trianglated (not including super triangle)
        protected int N;
        // total number of triangles generated
        protected int triangleCount;
        
        // triangle vertex and adjacency data
        // 0 -> Triangle Index
        // 1 -> [V1, V2, V3, E12, E23, E31]
        protected int[,] triangulation;

        // points on the plane that need to be triangluated
        public TriangulationPoint[] points;
        
        // array of trangles that should be skipped in final triangulation
        protected bool[] skipTriangle;
        
        // normal of the plane
        protected Vector3 normal;
        // normal scale factor
        public float normalizationScaleFactor = 1f;

        // edge constaints provided during initilisation
        private List<EdgeConstraint> constraints;

        // Map of each vertex to a triangle in the triangluation that contains it
        private int[] vertexTriangles;
        
        // Flag for each triangle to check if has been vistiated when finding the starting edge
        private bool[] visited;

        /// <summary>
        /// Initializes the triangulator with the vertex data to be triangulated given a set of edge constraints
        /// </summary>
        /// <param name="inputPoints">The of points to triangulate.</param>
        /// <param name="constraints">The list of edge constraints which defines how the vertices in `inputPoints` are connected.</param>
        /// <param name="normal">The normal of the plane in which the `inputPoints` lie.</param>
        public ConstrainedTriangluator(List<MeshVertex> inputPoints, List<EdgeConstraint> constraints, Vector3 normal) 
        {
            this.constraints = constraints;

            // Need at least three input vertices to triangulate
            if (inputPoints == null || inputPoints.Count < 3)
                return;

            // set the base values
            N = inputPoints.Count;
            triangleCount = 2 * N + 1;
            triangulation = new int[triangleCount, 6];
            skipTriangle = new bool[triangleCount];
            // extra 3 for super triangle
            points = new TriangulationPoint[N + 3]; 
            this.normal = normal;

            // chose two point in the plane as on basis vector
            Vector3 e1 = (inputPoints[0].position - inputPoints[1].position).normalized;
            Vector3 e2 = normal.normalized;
            Vector3 e3 = Vector3.Cross(e1, e2).normalized;

            // project the 3D vertex ont the 2D plane for all of the verticies
            for (int i = 0; i < N; i++)
            {
                var position = inputPoints[i].position;
                var coords = new Vector2(Vector3.Dot(position, e1), Vector3.Dot(position, e3));
                points[i] = new TriangulationPoint(i, coords);
            }
        }

        /// <summary>
        /// Calculates the triangulation
        /// </summary>
        /// <returns>Returns an array containing the indices of the triangles, mapped to the list of points passed in during initialization.</returns>
        public int[] Triangulate()
        {
            // need at least 3 verticies to triangulate
            if (N < 3)
                return new int[] {};

            // add the suprt triangles, normalze coordinates and start triangulation
            AddSuperTriangle();
            NormalizeCoordinates();
            ComputeTriangulation();

            // if there are any constraints then apply them
            if (constraints.Count > 0)
            {
                ApplyConstraints();
                DiscardTrianglesViolatingConstraints();
            }
            // remove the triangles with super triangle verts
            DiscardTrianglesWithSuperTriangleVertices();
            
            // looping through all of the triangles add all of the triangles
            // that dont contain a super-triangle vertex
            List<int> triangles = new List<int>(3 * triangleCount);
            for (int i = 0; i < triangleCount; i++)
            {
                if (!skipTriangle[i])
                {
                    triangles.Add(triangulation[i, V1]);
                    triangles.Add(triangulation[i, V2]);
                    triangles.Add(triangulation[i, V3]);
                }
            }

            // return the list of triangles
            return triangles.ToArray();
        }

        /// <summary>
        /// Initializes the triangulation by inserting the super triangle
        /// </summary>
        public void AddSuperTriangle()
        {
            // Add new points to the end of the points array
            points[N] = new TriangulationPoint(N, new Vector2(-100f, -100f));
            points[N + 1] = new TriangulationPoint(N + 1, new Vector2(0f, 100f));
            points[N + 2] = new TriangulationPoint(N + 2, new Vector2(100f, -100f));

            // Store supertriangle in the first column of the vertex and adjacency data
            triangulation[SUPERTRIANGLE, V1] = N;
            triangulation[SUPERTRIANGLE, V2] = N + 1;
            triangulation[SUPERTRIANGLE, V3] = N + 2;

            // Zeros signify boundary edges
            triangulation[SUPERTRIANGLE, E12] = OUT_OF_BOUNDS;
            triangulation[SUPERTRIANGLE, E23] = OUT_OF_BOUNDS;
            triangulation[SUPERTRIANGLE, E31] = OUT_OF_BOUNDS;
        }

        /// <summary>
        /// Uniformly scales the 2D coordinates of all the points between [0, 1]
        /// </summary>
        protected void NormalizeCoordinates()
        {
            // base values
            float xMin = float.MaxValue;
            float xMax = float.MinValue;
            float yMin = float.MaxValue;
            float yMax = float.MinValue;
            // find the min/max points in the set of points
            for (int i = 0; i < N; i++)
            {
                var point = points[i];
                if (point.coords.x < xMin) xMin = point.coords.x;
                if (point.coords.y < yMin) yMin = point.coords.y;
                if (point.coords.x > xMax) xMax = point.coords.x;
                if (point.coords.y > yMax) yMax = point.coords.y;
            }
            // calcualte the ormalization coefficent.
            normalizationScaleFactor = Mathf.Max(xMax - xMin, yMax - yMin);

            // normalize each of the points
            for (int i = 0; i < N; i++)
            {
                var point = points[i];
                var normalizedPos = new Vector2(
                    (point.coords.x - xMin) / normalizationScaleFactor,
                    (point.coords.y - yMin) / normalizationScaleFactor);

                points[i].coords = normalizedPos;
            }
        }

        /// <summary>
        /// Computes the triangulation of the point set.
        /// </summary>
        /// <returns>Returns true if the triangulation was successful</returns>
        protected bool ComputeTriangulation()
        {
            // Index of the current triangle being searched
            int tSearch = 0;
            // Index of the last triangle formed
            int tLast = 0;

            // list of sorted points
            var sortedPoints = SortPointsIntoBins();

            // Loop through each point and insert it into the triangulation
            for (int i = 0; i < N; i++)
            {
                TriangulationPoint point = sortedPoints[i];

                // Insert new point into the triangulation. Start by finding the triangle that contains the point `p`
                // Keep track of how many triangles we visited in case search fails and we get stuck in a loop
                int counter = 0;
                bool pointInserted = false;
                while (!pointInserted)
                {
                    // if at last point then stop
                    if (counter++ > tLast || tSearch == OUT_OF_BOUNDS)
                        break;

                    // get coords of triangle verts
                    var v1 = this.points[triangulation[tSearch, V1]].coords;
                    var v2 = this.points[triangulation[tSearch, V2]].coords;
                    var v3 = this.points[triangulation[tSearch, V3]].coords;

                    // Verify that point is on the correct side of each edge of the triangle.
                    // If a point is on the left side of an edge, move to the adjacent triangle and check again. The search
                    // continues until a containing triangle is found or the point is outside of all triangles
                    if (!Math.IsPointOnRightSideOfLine(v1, v2, point.coords))
                        tSearch = triangulation[tSearch, E12];
                    else if (!Math.IsPointOnRightSideOfLine(v2, v3, point.coords))
                        tSearch = triangulation[tSearch, E23];
                    else if (!Math.IsPointOnRightSideOfLine(v3, v1, point.coords))
                        tSearch = triangulation[tSearch, E31];
                    else
                    {
                        // If it is on the right  side of all three edges, it is contained within the triangle (Unity uses CW winding). 
                        InsertPointIntoTriangle(point, tSearch, tLast);
                        tLast += 2;
                        tSearch = tLast;
                        pointInserted = true;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Sorts the points into bins using an ordered grid
        /// </summary>
        /// <returns>Returns the array of sorted points</returns>
        protected TriangulationPoint[] SortPointsIntoBins()
        {
            // Compute the number of bins along each axis
            int n = Mathf.RoundToInt(Mathf.Pow((float)N, 0.25f));
            // Total bin count
            int binCount = n * n;
            // Assign bin numbers to each point by taking the normalized coordinates
            // and dividing them into a n x n grid.
            for (int k = 0; k < N; k++)
            {
                var point = this.points[k];
                int i = (int)(0.99f * n * point.coords.y);
                int j = (int)(0.99f * n * point.coords.x);
                point.bin = BinarySort.GetBinNumber(i, j, n);
            }
            // return the sorted list of points
            return BinarySort.Sort<TriangulationPoint>(this.points, N, binCount);
        }

        /// <summary>
        /// Inserts the point `p` into triangle `t`, replacing it with three new triangles
        /// </summary>
        /// <param name="p">The index of the point to insert</param>
        /// <param name="t">The index of the triangle</param>
        /// <param name="triangleCount">Total number of triangles created so far</param>
        protected void InsertPointIntoTriangle(TriangulationPoint p, int t, int triangleCount)
        {
            //                         V1
            //                         *
            //                        /|\
            //                       /3|2\
            //                      /  |  \
            //                     /   |   \
            //                    /    |    \
            //                   /     |     \
            //                  /  t1  |  t3  \
            //                 /       |       \
            //                /      1 * 1      \
            //               /      __/1\__      \
            //              /    __/       \__    \
            //             / 2__/     t2      \__3 \
            //            / _/3                 2\_ \
            //           *---------------------------*
            //         V3                             V2
            int t1 = t;
            int t2 = triangleCount + 1;
            int t3 = triangleCount + 2;

            // Add the vertex & adjacency information for the two new triangles
            // New vertex is set to first vertex of each triangle to help with
            // restoring the triangulation later on
            triangulation[t2, V1] = p.index;
            triangulation[t2, V2] = triangulation[t, V2];
            triangulation[t2, V3] = triangulation[t, V3];

            triangulation[t2, E12] = t3;
            triangulation[t2, E23] = triangulation[t, E23];
            triangulation[t2, E31] = t1;

            triangulation[t3, V1] = p.index;
            triangulation[t3, V2] = triangulation[t, V1];
            triangulation[t3, V3] = triangulation[t, V2];

            triangulation[t3, E12] = t1;
            triangulation[t3, E23] = triangulation[t, E12];
            triangulation[t3, E31] = t2;

            // Triangle index remains the same for E12, no need to update adjacency
            UpdateAdjacency(triangulation[t, E12], t, t3);
            UpdateAdjacency(triangulation[t, E23], t, t2);

            // Replace existing triangle `t` with `t1`
            triangulation[t1, V2] = triangulation[t, V3];
            triangulation[t1, V3] = triangulation[t, V1];
            triangulation[t1, V1] = p.index;

            triangulation[t1, E23] = triangulation[t, E31];
            triangulation[t1, E12] = t2;
            triangulation[t1, E31] = t3;

            // After the triangles have been inserted, restore the Delauney triangulation
            RestoreDelauneyTriangulation(p, t1, t2, t3);
        }

        /// <summary>
        /// Updates the adjacency information in triangle `t`. Any references to `tOld are
        /// replaced with `tNew`
        /// </summary>
        /// <param name="t">The index of the triangle to update</param>
        /// <param name="tOld">The index to be replaced</param>
        /// <param name="tNew">The new index to replace with</param>
        protected void UpdateAdjacency(int t, int tOld, int tNew)
        {
            // Boundary edge, no triangle exists
            int sharedEdge;
            if (t == OUT_OF_BOUNDS)
                return;
            else if (FindSharedEdge(t, tOld, out sharedEdge))
                triangulation[t, sharedEdge] = tNew;
        }

        /// <summary>
        /// Finds the edge index for triangle `tOrigin` that is adjacent to triangle `tAdjacent`
        /// </summary>
        /// <param name="tOrigin">The origin triangle to search</param>
        /// <param name="tAdjacent">The triangle index to search for</param>
        /// <param name="edgeIndex">Edge index returned as an out parameter</param>
        /// <returns>True if `tOrigin` is adjacent to `tAdjacent` and supplies the
        /// shared edge index via the out parameter. If `tOrigin` is an invalid index or
        /// `tAdjacent` is not adjacent to `tOrigin`, returns false.</returns>
        protected bool FindSharedEdge(int tOrigin, int tAdjacent, out int edgeIndex)
        {
            edgeIndex = 0;
            if (tOrigin == OUT_OF_BOUNDS)
                return false;
            else if (triangulation[tOrigin, E12] == tAdjacent)
            {
                edgeIndex = E12;
                return true;
            }
            else if (triangulation[tOrigin, E23] == tAdjacent)
            {
                edgeIndex = E23;
                return true;
            }
            else if (triangulation[tOrigin, E31] == tAdjacent)
            {
                edgeIndex = E31;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Restores the triangulation to a Delauney triangulation after new triangles have been added.
        /// </summary>
        /// <param name="p">Index of the inserted point</param>
        /// <param name="t1">Index of first triangle to check</param>
        /// <param name="t2">Index of second triangle to check</param>
        /// <param name="t3">Index of third triangle to check</param>
        protected void RestoreDelauneyTriangulation(TriangulationPoint p, int t1, int t2, int t3)
        {
            int t4;
            // create stack and add first 3 points
            Stack<(int, int)> s = new Stack<(int, int)>();
            s.Push((t1, triangulation[t1, E23]));
            s.Push((t2, triangulation[t2, E23]));
            s.Push((t3, triangulation[t3, E23]));
            while (s.Count > 0)
            {
                // Pop next triangle and its adjacent triangle off the stack
                // t1 contains the newly added vertex at V1
                // t2 is adjacent to t1 along the opposite edge of V1
                (t1, t2) = s.Pop();
                if (t2 == OUT_OF_BOUNDS)
                    continue;
                // If t2 circumscribes p, the quadrilateral formed by t1+t2 has the
                // diagonal drawn in the wrong direction and needs to be swapped
                else if (SwapQuadDiagonalIfNeeded(p.index, t1, t2, out t3, out t4))
                {
                    // Push newly formed triangles onto the stack to see if their diagonals
                    // need to be swapped
                    s.Push((t1, t3));
                    s.Push((t2, t4));
                }
            }
        }

        /// <summary>
        /// Swaps the diagonal of the quadrilateral formed by triangle `t` and the
        /// triangle adjacent to the edge that is opposite of the newly added point
        /// </summary>
        /// <param name="p">The index of the inserted point</param>
        /// <param name="t1">Index of the triangle containing p</param>
        /// <param name="t2">Index of the triangle opposite t1 that shares edge E23 with t1</param>
        /// <param name="t3">Index of triangle adjacent to t1 after swap</param>
        /// <param name="t4">Index of triangle adjacent to t2 after swap</param>
        /// <returns>Returns true if the swap was performed. If the swap was not
        /// performed (e.g. returns false), t3 and t4 are unused.
        /// </returns>
        protected bool SwapQuadDiagonalIfNeeded(int p, int t1, int t2, out int t3, out int t4)
        {
            // 1) Form quadrilateral from t1 + t2 (q0->q1->q2->q3)
            // 2) Swap diagonal between q1->q3 to q0->q2
            //
            //               BEFORE                            AFTER
            //  
            //                 q3                                q3
            //    *-------------*-------------*    *-------------*-------------*
            //     \           / \           /      \           /|\           / 
            //      \   t3    /   \   t4    /        \   t3    /3|2\   t4    /  
            //       \       /     \       /          \       /  |  \       /   
            //        \     /       \     /            \     /   |   \     /    
            //         \   /   t2    \   /              \   /    |    \   /     
            //          \ /           \ /                \ /     |     \ /     
            //        q1 *-------------*  q2           q1 * 2 t1 | t2 3 * q2
            //            \2         3/                    \     |     /        
            //             \         /                      \    |    /         
            //              \  t1   /                        \   |   /          
            //               \     /                          \  |  /          
            //                \   /                            \1|1/            
            //                 \1/                              \|/             
            //                  *  q4 == p                       *  q4 == p   
            //

            // Get the vertices of the quad. The new vertex is always located at V1 of the triangle
            int q4 = p;
            int q1, q2, q3;

            // Since t2 might be oriented in any direction, find which edge is adjacent to `t`
            // The 4th vertex of the quad will be opposite this edge. We also need the two triangles
            // t3 and t3 that are adjacent to t2 along the other edges since the adjacency information
            // needs to be updated for those triangles.
            if (triangulation[t2, E12] == t1)
            {
                q1 = triangulation[t2, V2];
                q2 = triangulation[t2, V1];
                q3 = triangulation[t2, V3];
                t3 = triangulation[t2, E23];
                t4 = triangulation[t2, E31];
            }
            else if (triangulation[t2, E23] == t1)
            {
                q1 = triangulation[t2, V3];
                q2 = triangulation[t2, V2];
                q3 = triangulation[t2, V1];
                t3 = triangulation[t2, E31];
                t4 = triangulation[t2, E12];
            }
            else // (triangulation[t2, E31] == t1)
            {
                q1 = triangulation[t2, V1];
                q2 = triangulation[t2, V3];
                q3 = triangulation[t2, V2];
                t3 = triangulation[t2, E12];
                t4 = triangulation[t2, E23];
            }

            // Perform test to see if p lies in the circumcircle of t2
            if (SwapTest(points[q1].coords, points[q2].coords, points[q3].coords, points[q4].coords))
            {
                // Update adjacency for triangles adjacent to t1 and t2
                UpdateAdjacency(t3, t2, t1);
                UpdateAdjacency(triangulation[t1, E31], t1, t2);

                // Perform the swap. As always, put the new vertex as the first vertex of the triangle
                triangulation[t1, V1] = q4;
                triangulation[t1, V2] = q1;
                triangulation[t1, V3] = q3;

                triangulation[t2, V1] = q4;
                triangulation[t2, V2] = q3;
                triangulation[t2, V3] = q2;

                // Update adjacency information (order of operations is important here since we
                // are overwriting data).
                triangulation[t2, E12] = t1;
                triangulation[t2, E23] = t4;
                triangulation[t2, E31] = triangulation[t1, E31];

                // triangulation[t1, E12] = t2;
                triangulation[t1, E23] = t3;
                triangulation[t1, E31] = t2;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Checks to see if the triangle formed by points v1->v2->v3 circumscribes point vP
        /// </summary>
        /// <param name="v1">Coordinates of 1st vertex of triangle</param>
        /// <param name="v2">Coordinates of 2nd vertex of triangle</param>
        /// <param name="v3">Coordinates of 3rd vertex of triangle</param>
        /// <param name="v4">Coordinates of test point</param>
        /// <returns> Returns true if the triangle `t` circumscribes the point `p`</returns>
        protected bool SwapTest(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4)
        {
            float x13 = v1.x - v3.x;
            float x23 = v2.x - v3.x;
            float y13 = v1.y - v3.y;
            float y23 = v2.y - v3.y;
            float x14 = v1.x - v4.x;
            float x24 = v2.x - v4.x;
            float y14 = v1.y - v4.y;
            float y24 = v2.y - v4.y;
            float cosA = x13 * x23 + y13 * y23;
            float cosB = x24 * x14 + y24 * y14;
            if (cosA >= 0 && cosB >= 0)
                return false;
            else if (cosA < 0 && cosB < 0)
                return true;
            else
            {
                float sinA = (x13 * y23 - x23 * y13);
                float sinB = (x24 * y14 - x14 * y24);
                float sinAB = sinA * cosB + sinB * cosA;
                return sinAB < 0;
            }
        }

        /// <summary>
        /// Applys the edge constraints to the triangulation
        /// </summary>
        internal void ApplyConstraints()
        {
            visited = new bool[triangulation.GetLength(0)];

            // Map each vertex to a triangle that contains it
            vertexTriangles = new int[N + 3];
            for (int i = 0; i < triangulation.GetLength(0); i++)
            {
                vertexTriangles[triangulation[i, V1]] = i;
                vertexTriangles[triangulation[i, V2]] = i;
                vertexTriangles[triangulation[i, V3]] = i;
            }

            // Loop through each edge constraint
            foreach (EdgeConstraint constraint in constraints)
            {
                if (constraint.v1 == constraint.v2) continue;

                // We find the edges of the triangulation that intersect the constraint edge and remove them
                // For each intersecting edge, we identify the triangles that share that edge (which form a quad)
                // The diagonal of this quad is flipped.
                Queue<EdgeConstraint> intersectingEdges = FindIntersectingEdges(constraint, vertexTriangles);
                RemoveIntersectingEdges(constraint, intersectingEdges);
            }
        }

        /// <summary>
        /// Searches through the triangulation to find intersecting edges
        /// </summary>
        /// <param name="intersectingEdges"></param>
        internal Queue<EdgeConstraint> FindIntersectingEdges(EdgeConstraint constraint, int[] vertexTriangles)
        {
            Queue<EdgeConstraint> intersectingEdges = new Queue<EdgeConstraint>();

            // Need to find the first edge that the constraint crosses.
            EdgeConstraint startEdge;
            if (FindStartingEdge(vertexTriangles, constraint, out startEdge))
                intersectingEdges.Enqueue(startEdge);
            else
                return intersectingEdges;

            // Search for all triangles that intersect the constraint. Stop when we find a triangle that contains v_j
            int t = startEdge.t1;
            int edgeIndex = startEdge.t1Edge;
            int lastTriangle = t;
            bool finalTriangleFound = false;
            while (!finalTriangleFound)
            {
                // Cross the last intersecting edge and inspect the next triangle
                lastTriangle = t;
                t = triangulation[t, edgeIndex];

                // Get coordinates of constraint end points and triangle vertices
                Vector2 v_i = points[constraint.v1].coords;
                Vector2 v_j = points[constraint.v2].coords;
                Vector2 v1 = points[triangulation[t, V1]].coords;
                Vector2 v2 = points[triangulation[t, V2]].coords;
                Vector2 v3 = points[triangulation[t, V3]].coords;

                // If triangle contains the endpoint of the constraint, the search is done
                if (TriangleContainsVertex(t, constraint.v2))
                    finalTriangleFound = true;
                // Otherwise, the constraint must intersect one edge of this triangle. Ignore the edge that we entered from
                else if ((triangulation[t, E12] != lastTriangle) && Math.LinesIntersect(v_i, v_j, v1, v2))
                {
                    edgeIndex = E12;
                    var edge = new EdgeConstraint(triangulation[t, V1], triangulation[t, V2], t, triangulation[t, E12], edgeIndex);
                    intersectingEdges.Enqueue(edge);
                }
                else if ((triangulation[t, E23] != lastTriangle) && Math.LinesIntersect(v_i, v_j, v2, v3))
                {
                    edgeIndex = E23;
                    var edge = new EdgeConstraint(triangulation[t, V2], triangulation[t, V3], t, triangulation[t, E23], edgeIndex);
                    intersectingEdges.Enqueue(edge);
                }
                else if ((triangulation[t, E31] != lastTriangle) && Math.LinesIntersect(v_i, v_j, v3, v1))
                {
                    edgeIndex = E31;
                    var edge = new EdgeConstraint(triangulation[t, V3], triangulation[t, V1], t, triangulation[t, E31], edgeIndex);
                    intersectingEdges.Enqueue(edge);
                }
                else
                {
                    // Shouldn't reach this point
                    Debug.LogWarning("Failed to find final triangle, exiting early.");
                    break;
                }
            }
            return intersectingEdges;
        }

        /// <summary>
        /// Checks if the triangle `t` contains the specified vertex
        /// </summary>
        /// <param name="t">The index of the triangle</param>
        /// <param name="v">The index of the vertex</param>
        /// <returns>Returns true if the triangle `t` contains the vertex `v`</returns>
        protected bool TriangleContainsVertex(int t, int v)
        {
            return triangulation[t, V1] == v || triangulation[t, V2] == v || triangulation[t, V3] == v;
        }

        /// <summary>
        /// Finds the starting edge for the search to find all edges that intersect the constraint
        /// </summary>
        /// <param name="constraint">The constraint being used to check for intersections</param>
        internal bool FindStartingEdge(int[] vertexTriangles, EdgeConstraint constraint, out EdgeConstraint startingEdge)
        {
            // Initialize out parameter to default value
            startingEdge = new EdgeConstraint(-1, -1);

            // v_i->v_j are the start/end points of the constraint, respectively
            int v_i = constraint.v1;
            int v_j = constraint.v2;

            // Start the search with an initial triangle that contains v_i
            int tSearch = vertexTriangles[v_i];

            // Reset visited states
            for (int i = 0; i < visited.Length; i++)
            {
                visited[i] = false;
            }

            // Circle v_i until we find a triangle that contains an edge which intersects the constraint edge
            // This will be the starting triangle in the search for finding all triangles that intersect the constraint
            bool intersectionFound = false;
            bool noCandidatesFound = false;
            int intersectingEdgeIndex = E12;
            int tE12, tE23, tE31;

            while (!intersectionFound && !noCandidatesFound)
            {
                visited[tSearch] = true;

                // Triangulation already contains the constraint so we ignore the constraint
                if (TriangleContainsConstraint(tSearch, constraint))
                    return false;
                // Check if the constraint intersects any edges of this triangle
                else if (EdgeConstraintIntersectsTriangle(tSearch, constraint, out intersectingEdgeIndex))
                { 
                    intersectionFound = true;
                    break; 
                }

                tE12 = triangulation[tSearch, E12];
                tE23 = triangulation[tSearch, E23];
                tE31 = triangulation[tSearch, E31];
                // If constraint does not intersect this triangle, check adjacent triangles by crossing edges that have v_i as a vertex
                // Avoid triangles that we have previously visited in the search
                if (tE12 != OUT_OF_BOUNDS && !visited[tE12] && TriangleContainsVertex(tE12, v_i))
                    tSearch = tE12;
                else if (tE23 != OUT_OF_BOUNDS && !visited[tE23] && TriangleContainsVertex(tE23, v_i))
                    tSearch = tE23;
                else if (tE31 != OUT_OF_BOUNDS && !visited[tE31] && TriangleContainsVertex(tE31, v_i))
                    tSearch = tE31;
                else
                {
                    noCandidatesFound = true;
                    break;
                }
            }
            // if there was not intersection found then add a new starting edge and reutn true
            if (intersectionFound)
            {
                int v_k = triangulation[tSearch, edgeVertex1[intersectingEdgeIndex]];
                int v_l = triangulation[tSearch, edgeVertex2[intersectingEdgeIndex]];
                int triangle2 = triangulation[tSearch, intersectingEdgeIndex];
                startingEdge = new EdgeConstraint(v_k, v_l, tSearch, triangle2, intersectingEdgeIndex);

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Returns true if the edge constraint intersects an edge of triangle `t`
        /// </summary>
        /// <param name="t">The triangle to test</param>
        /// <param name="constraint">The edge constraint</param>
        /// <param name="intersectingEdgeIndex">The index of the intersecting edge (E12, E23, E31)</param>
        /// <returns>Returns true if an intersection is found, otherwise false.</returns>
        internal bool EdgeConstraintIntersectsTriangle(int t, EdgeConstraint constraint, out int intersectingEdgeIndex)
        {
            Vector2 v_i = points[constraint.v1].coords;
            Vector2 v_j = points[constraint.v2].coords;
            Vector2 v1 = points[triangulation[t, V1]].coords;
            Vector2 v2 = points[triangulation[t, V2]].coords;
            Vector2 v3 = points[triangulation[t, V3]].coords;

            if (Math.LinesIntersect(v_i, v_j, v1, v2))
            {
                intersectingEdgeIndex = E12;
                return true;
            }
            else if (Math.LinesIntersect(v_i, v_j, v2, v3))
            {
                intersectingEdgeIndex = E23;
                return true;
            }
            else if (Math.LinesIntersect(v_i, v_j, v3, v1))
            {
                intersectingEdgeIndex = E31;
                return true;
            }
            else
            {
                intersectingEdgeIndex = -1;
                return false;
            }
        }

        /// <summary>
        /// Determines if the triangle contains the edge constraint
        /// </summary>
        /// <param name="t">The triangle to test</param>
        /// <param name="constraint">The edge constraint</param>
        /// <returns>True if the triangle contains one or both of the endpoints of the constraint</returns>
        internal bool TriangleContainsConstraint(int t, EdgeConstraint constraint)
        {
            return (triangulation[t, V1] == constraint.v1 || triangulation[t, V2] == constraint.v1 || triangulation[t, V3] == constraint.v1) &&
                   (triangulation[t, V1] == constraint.v2 || triangulation[t, V2] == constraint.v2 || triangulation[t, V3] == constraint.v2);
        }

        /// <summary>
        /// Remove the edges from the triangulation that intersect the constraint. Find two triangles that
        /// share the intersecting edge, swap the diagonal and repeat until no edges intersect the constraint.
        /// </summary>
        /// <param name="constraint">The constraint to check against</param>
        /// <param name="intersectingEdges">A queue containing the previously found edges that intersect the constraint</param>
        internal void RemoveIntersectingEdges(EdgeConstraint constraint, Queue<EdgeConstraint> intersectingEdges)
        {
            // Remove intersecting edges. Keep track of the new edges that we create
            List<EdgeConstraint> newEdges = new List<EdgeConstraint>();
            EdgeConstraint edge, newEdge;

            // Mark the number of times we have been through the loop. If no new edges
            // have been added after all edges have been visited, stop the loop. Every 
            // time an edge is added to newEdges, reset the counter.
            int counter = 0;

            // Loop through all intersecting edges until they have been properly resolved
            // or they have all been visited with no diagonal swaps.
            while (intersectingEdges.Count > 0 && counter <= intersectingEdges.Count)
            {
                edge = intersectingEdges.Dequeue();

                Quad quad;
                if (FindQuadFromSharedEdge(edge.t1, edge.t1Edge, out quad))
                {
                    // If the quad is convex, we swap the diagonal (a quad is convex if the diagonals intersect)
                    // Otherwise push it back into the queue so we can swap the diagonal later on.
                    if (Math.LinesIntersect(points[quad.q4].coords,
                            points[quad.q3].coords,
                            points[quad.q1].coords,
                            points[quad.q2].coords))
                    {
                        // Swap diagonals of the convex quads whose diagonals intersect the constraint
                        SwapQuadDiagonal(quad, intersectingEdges, newEdges, constraints);

                        // The new diagonal is between Q3 and Q4
                        newEdge = new EdgeConstraint(quad.q3, quad.q4, quad.t1, quad.t2, E31);

                        // If the new diagonal still intersects the constraint edge v_i->v_j,
                        // put back on the list of intersecting eddges
                        if (Math.LinesIntersect(points[constraint.v1].coords,
                                points[constraint.v2].coords,
                                points[quad.q3].coords,
                                points[quad.q4].coords))
                        {
                            intersectingEdges.Enqueue(newEdge);
                        }
                        else
                        {
                            // Otherwise record in list of new edges
                            counter = 0;
                            newEdges.Add(newEdge);
                        }
                    }
                    else
                        intersectingEdges.Enqueue(edge);
                }

                counter++;
            }

            // If any new edges were formed due to a diagonal being swapped, restore the Delauney condition
            // of the triangulation while respecting the constraints
            if (newEdges.Count > 0)
                RestoreConstrainedDelauneyTriangulation(constraint, newEdges);
        }

        /// <summary>
        /// Restores the Delauney triangulation after the constraint has been inserted
        /// </summary>
        /// <param name="constraint">The constraint that was added to the triangulation</param>
        /// <param name="newEdges">The list of new edges that were added</param>
        internal void RestoreConstrainedDelauneyTriangulation(EdgeConstraint constraint, List<EdgeConstraint> newEdges)
        {
            // Iterate over the list of newly created edges and swap non-constraint diagonals until no more swaps take place
            bool swapOccurred = true;
            int counter = 0;
            while (swapOccurred)
            {
                counter++;
                swapOccurred = false;
                for (int i = 0; i < newEdges.Count; i++)
                {
                    EdgeConstraint edge = newEdges[i];
                    // If newly added edge is equal to constraint, we don't want to flip this edge so skip it
                    if (edge == constraint)
                        continue;
                    Quad quad;
                    if (FindQuadFromSharedEdge(edge.t1, edge.t1Edge, out quad))
                    {
                        if (SwapTest(points[quad.q1].coords, points[quad.q2].coords, points[quad.q3].coords, points[quad.q4].coords))
                        {
                            SwapQuadDiagonal(quad, newEdges, constraints, null);

                            // Enqueue the new diagonal
                            int v_m = quad.q3;
                            int v_n = quad.q4;
                            newEdges[i] = new EdgeConstraint(v_m, v_n, quad.t1, quad.t2, E31);
                            swapOccurred = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the quad formed by triangle `t1` and the other triangle that shares the intersecting edge
        /// </summary>
        /// <param name="t1">Base triangle</param>
        /// <param name="intersectingEdge">Edge index that is being intersected</param>
        internal bool FindQuadFromSharedEdge(int t1, int t1SharedEdge, out Quad quad)
        {
            //               q3        
            //      *---------*---------*
            //       \       / \       /
            //        \ t2L /   \ t2R /
            //         \   /     \   /
            //          \ /   t2  \ /
            //        q1 *---------* q2 
            //          / \   t1  / \    
            //         /   \     /   \     
            //        / t1L \   / t1R \   
            //       /       \ /       \  
            //      *---------*---------*
            //               q4       

            int q1, q2, q3, q4;
            int t1L, t1R, t2L, t2R;

            // t2 is adjacent to t1 along t1Edge
            int t2 = triangulation[t1, t1SharedEdge];
            int t2SharedEdge;
            if (FindSharedEdge(t2, t1, out t2SharedEdge))
            {
                // Get the top 3 vertices of the quad from t2
                if (t2SharedEdge == E12)
                {
                    q2 = triangulation[t2, V1];
                    q1 = triangulation[t2, V2];
                    q3 = triangulation[t2, V3];
                }
                else if (t2SharedEdge == E23)
                {
                    q2 = triangulation[t2, V2];
                    q1 = triangulation[t2, V3];
                    q3 = triangulation[t2, V1];
                }
                else // (t2SharedEdge == E31)
                {
                    q2 = triangulation[t2, V3];
                    q1 = triangulation[t2, V1];
                    q3 = triangulation[t2, V2];
                }
                // q4 is the point in t1 opposite of the shared edge
                q4 = triangulation[t1, oppositePoint[t1SharedEdge]];

                // Get the adjacent triangles to make updating adjacency easier
                t1L = triangulation[t1, previousEdge[t1SharedEdge]];
                t1R = triangulation[t1, nextEdge[t1SharedEdge]];
                t2L = triangulation[t2, nextEdge[t2SharedEdge]];
                t2R = triangulation[t2, previousEdge[t2SharedEdge]];
                quad = new Quad(q1, q2, q3, q4, t1, t2, t1L, t1R, t2L, t2R);
                return true;
            }
            quad = new Quad();
            return false;
        }

        /// <summary>
        /// Swaps the diagonal of the quadrilateral q0->q1->q2->q3 formed by t1 and t2
        /// </summary>
        /// <param name="">The quad that will have its diagonal swapped</param>
        internal void SwapQuadDiagonal(Quad quad, IEnumerable<EdgeConstraint> edges1, IEnumerable<EdgeConstraint> edges2, IEnumerable<EdgeConstraint> edges3)
        {
            // BEFORE
            //               q3        
            //      *---------*---------*
            //       \       / \       /
            //        \ t2L /   \ t2R /
            //         \   /     \   /
            //          \ /   t2  \ /
            //        q1 *---------* q2 
            //          / \   t1  / \    
            //         /   \     /   \     
            //        / t1L \   / t1R \   
            //       /       \ /       \  
            //      *---------*---------*
            //               q4           

            // AFTER
            //               q3        
            //      *---------*---------*
            //       \       /|\       /
            //        \ t2L / | \ t2R /
            //         \   /  |  \   /
            //          \ /   |   \ /
            //        q1 * t1 | t2 * q2 
            //          / \   |   / \    
            //         /   \  |  /   \     
            //        / t1L \ | / t1R \   
            //       /       \|/       \  
            //      *---------*---------*
            //               q4      
            int t1 = quad.t1;
            int t2 = quad.t2;
            int t1R = quad.t1R;
            int t1L = quad.t1L;
            int t2R = quad.t2R;
            int t2L = quad.t2L;

            // Perform the swap. As always, put the new vertex as the first vertex of the triangle
            triangulation[t1, V1] = quad.q4;
            triangulation[t1, V2] = quad.q1;
            triangulation[t1, V3] = quad.q3;

            triangulation[t2, V1] = quad.q4;
            triangulation[t2, V2] = quad.q3;
            triangulation[t2, V3] = quad.q2;

            triangulation[t1, E12] = t1L;
            triangulation[t1, E23] = t2L;
            triangulation[t1, E31] = t2;

            triangulation[t2, E12] = t1;
            triangulation[t2, E23] = t2R;
            triangulation[t2, E31] = t1R;

            // Update adjacency for the adjacent triangles
            UpdateAdjacency(t2L, t2, t1);
            UpdateAdjacency(t1R, t1, t2);

            // Now that triangles have moved, need to update edges as well
            UpdateEdgesAfterSwap(edges1, t1, t2, t1L, t1R, t2L, t2R);
            UpdateEdgesAfterSwap(edges2, t1, t2, t1L, t1R, t2L, t2R);
            UpdateEdgesAfterSwap(edges3, t1, t2, t1L, t1R, t2L, t2R);

            // Also need to update the vertexTriangles array since the vertices q1 and q2
            // may have been referencing t2/t1 respectively and they are no longer.
            vertexTriangles[quad.q1] = t1;
            vertexTriangles[quad.q2] = t2;
        }

        /// <summary>
        /// Update the Edges
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="t1L"></param>
        /// <param name="t1R"></param>
        /// <param name="t2L"></param>
        /// <param name="t2R"></param>
        internal void UpdateEdgesAfterSwap(IEnumerable<EdgeConstraint> edges, int t1, int t2, int t1L, int t1R, int t2L, int t2R)
        {
            if (edges == null)
                return;

            // Update edges to reflect changes in triangles
            foreach (EdgeConstraint edge in edges)
            {
                if (edge.t1 == t1 && edge.t2 == t1R)
                {
                    edge.t1 = t2;
                    edge.t2 = t1R;
                    edge.t1Edge = E31;
                }
                // Triangles stay the same
                else if (edge.t1 == t1 && edge.t2 == t1L)
                    edge.t1Edge = E12;
                else if (edge.t1 == t1R && edge.t2 == t1)
                    edge.t2 = t2;
                else if (edge.t1 == t1L && edge.t2 == t1)
                {
                    // Unchanged
                }
                // Triangles stay the same
                else if (edge.t1 == t2 && edge.t2 == t2R)
                    edge.t1Edge = E23;
                else if (edge.t1 == t2 && edge.t2 == t2L)
                {
                    edge.t1 = t1;
                    edge.t2 = t2L;
                    edge.t1Edge = E23;
                }
                else if (edge.t1 == t2R && edge.t2 == t2)
                {
                    // Unchanged
                }
                else if (edge.t1 == t2L && edge.t2 == t2)
                    edge.t2 = t1;
            }
        }

        /// <summary>
        /// Discards triangles that violate the any of the edge constraints
        /// </summary>
        internal void DiscardTrianglesViolatingConstraints()
        {
            // Initialize to all triangles being skipped
            for (int i = 0; i < triangleCount; i++)
            {
                skipTriangle[i] = true;
            }

            // Identify the boundary edges
            HashSet<(int, int)> boundaries = new HashSet<(int, int)>();
            for (int i = 0; i < this.constraints.Count; i++)
            {
                EdgeConstraint constraint = this.constraints[i];
                boundaries.Add((constraint.v1, constraint.v2));
            }
            // Reset visited states
            for (int i = 0; i < visited.Length; i++)
            {
                visited[i] = false;
            }
            // Search frontier
            Queue<int> frontier = new Queue<int>();
            int v1, v2, v3;
            bool boundaryE12, boundaryE23, boundaryE31;
            for (int i = 0; i < triangleCount; i++)
            {
                // If we've already visited this triangle, skip it
                if (visited[i])
                    continue;
                v1 = triangulation[i, V1];
                v2 = triangulation[i, V2];
                v3 = triangulation[i, V3];
                boundaryE12 = boundaries.Contains((v1, v2));
                boundaryE23 = boundaries.Contains((v2, v3));
                boundaryE31 = boundaries.Contains((v3, v1));
                // If this triangle has a boundary edge, start searching for adjacent triangles
                if (boundaryE12 || boundaryE23 || boundaryE31)
                {
                    skipTriangle[i] = false;
                    // Search along edges that are not boundary edges
                    frontier.Clear();
                    if (!boundaryE12)
                        frontier.Enqueue(triangulation[i, E12]);
                    if (!boundaryE23)
                        frontier.Enqueue(triangulation[i, E23]);
                    if (!boundaryE31)
                        frontier.Enqueue(triangulation[i, E31]);

                    // Recursively search along all non-boundary edges, marking the
                    // adjacent triangles as "keep"
                    while (frontier.Count > 0)
                    {
                        int k = frontier.Dequeue();

                        if (k == OUT_OF_BOUNDS || visited[k])
                            continue;
                        skipTriangle[k] = false;
                        visited[k] = true;
                        v1 = triangulation[k, V1];
                        v2 = triangulation[k, V2];
                        v3 = triangulation[k, V3];

                        // Continue searching along non-boundary edges
                        if (!boundaries.Contains((v1, v2)))
                            frontier.Enqueue(triangulation[k, E12]);
                        if (!boundaries.Contains((v2, v3)))
                            frontier.Enqueue(triangulation[k, E23]);
                        if (!boundaries.Contains((v3, v1)))
                            frontier.Enqueue(triangulation[k, E31]);
                    }
                }
            }
        }

        /// <summary>
        /// Marks any triangles that contain super-triangle vertices as discarded
        /// </summary>
        protected void DiscardTrianglesWithSuperTriangleVertices()
        {
            for (int i = 0; i < triangleCount; i++)
            {
                // Add all triangles that don't contain a super-triangle vertex
                if (TriangleContainsVertex(i, N) ||
                    TriangleContainsVertex(i, N + 1) ||
                    TriangleContainsVertex(i, N + 2))
                {
                    skipTriangle[i] = true;
                }
            }
        }
    }
}