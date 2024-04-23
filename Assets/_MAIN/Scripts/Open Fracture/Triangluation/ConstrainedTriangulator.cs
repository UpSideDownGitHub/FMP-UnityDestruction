using System.Collections.Generic;
using UnityEngine;

namespace UnityFracture
{
    public class ConstrainedTriangluator 
    {
        protected const int V1 = 0; // Vertex 1
        protected const int V2 = 1; // Vertex 2
        protected const int V3 = 2; // Vertex 3
        protected const int E12 = 3; // Adjacency data for edge (V1 -> V2)
        protected const int E23 = 4; // Adjacency data for edge (V2 -> V3)
        protected const int E31 = 5; // Adjacency data for edge (V3 -> V1)
        private static readonly int[] edgeVertex1 = new int[] { 0, 0, 0, V1, V2, V3 };
        private static readonly int[] edgeVertex2 = new int[] { 0, 0, 0, V2, V3, V1 };
        private static readonly int[] oppositePoint = new int[] { 0, 0, 0, V3, V1, V2 };
        private static readonly int[] nextEdge = new int[] { 0, 0, 0, E23, E31, E12 };
        private static readonly int[] previousEdge = new int[] { 0, 0, 0, E31, E12, E23 };
        protected const int SUPERTRIANGLE = 0;
        protected const int OUT_OF_BOUNDS = -1;
        protected int N;
        protected int triangleCount;
        protected int[,] triangulation;
        public TriangulationPoint[] points;
        protected bool[] skipTriangle;
        protected Vector3 normal;
        public float normalizationScaleFactor = 1f;
        private List<EdgeConstraint> constraints;
        private int[] vertexTriangles;
        private bool[] visited;

        public ConstrainedTriangluator(List<MeshVertex> inputPoints, List<EdgeConstraint> constraints, Vector3 normal) 
        {
            this.constraints = constraints;

            if (inputPoints == null || inputPoints.Count < 3)
                return;

            N = inputPoints.Count;
            triangleCount = 2 * N + 1;
            triangulation = new int[triangleCount, 6];
            skipTriangle = new bool[triangleCount];
            points = new TriangulationPoint[N + 3];
            this.normal = normal;

            Vector3 e1 = (inputPoints[0].position - inputPoints[1].position).normalized;
            Vector3 e2 = normal.normalized;
            Vector3 e3 = Vector3.Cross(e1, e2).normalized;

            for (int i = 0; i < N; i++)
            {
                var position = inputPoints[i].position;
                var coords = new Vector2(Vector3.Dot(position, e1), Vector3.Dot(position, e3));
                this.points[i] = new TriangulationPoint(i, coords);
            }
        }

        public int[] Triangulate()
        {
            if (N < 3)
                return new int[] {};

            AddSuperTriangle();
            NormalizeCoordinates();
            ComputeTriangulation();

            if (constraints.Count > 0)
            {
                ApplyConstraints();
                DiscardTrianglesViolatingConstraints();
            }
            DiscardTrianglesWithSuperTriangleVertices();
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

            return triangles.ToArray();
        }

        public void AddSuperTriangle()
        {
            this.points[N] = new TriangulationPoint(N, new Vector2(-100f, -100f));
            this.points[N + 1] = new TriangulationPoint(N + 1, new Vector2(0f, 100f));
            this.points[N + 2] = new TriangulationPoint(N + 2, new Vector2(100f, -100f));
            triangulation[SUPERTRIANGLE, V1] = N;
            triangulation[SUPERTRIANGLE, V2] = N + 1;
            triangulation[SUPERTRIANGLE, V3] = N + 2;
            triangulation[SUPERTRIANGLE, E12] = OUT_OF_BOUNDS;
            triangulation[SUPERTRIANGLE, E23] = OUT_OF_BOUNDS;
            triangulation[SUPERTRIANGLE, E31] = OUT_OF_BOUNDS;
        }

        protected void NormalizeCoordinates()
        {
            float xMin = float.MaxValue;
            float xMax = float.MinValue;
            float yMin = float.MaxValue;
            float yMax = float.MinValue;
            for (int i = 0; i < N; i++)
            {
                var point = points[i];
                if (point.coords.x < xMin) xMin = point.coords.x;
                if (point.coords.y < yMin) yMin = point.coords.y;
                if (point.coords.x > xMax) xMax = point.coords.x;
                if (point.coords.y > yMax) yMax = point.coords.y;
            }
            normalizationScaleFactor = Mathf.Max(xMax - xMin, yMax - yMin);
            for (int i = 0; i < N; i++)
            {
                var point = points[i];
                var normalizedPos = new Vector2(
                    (point.coords.x - xMin) / normalizationScaleFactor,
                    (point.coords.y - yMin) / normalizationScaleFactor);

                points[i].coords = normalizedPos;
            }
        }

        protected bool ComputeTriangulation()
        {
            int tSearch = 0;
            int tLast = 0;
            var sortedPoints = SortPointsIntoBins();
            for (int i = 0; i < N; i++)
            {
                TriangulationPoint point = sortedPoints[i];

                int counter = 0;
                bool pointInserted = false;
                while (!pointInserted)
                {
                    if (counter++ > tLast || tSearch == OUT_OF_BOUNDS)
                        break;
                    var v1 = this.points[triangulation[tSearch, V1]].coords;
                    var v2 = this.points[triangulation[tSearch, V2]].coords;
                    var v3 = this.points[triangulation[tSearch, V3]].coords;

                    if (!Math.IsPointOnRightSideOfLine(v1, v2, point.coords))
                        tSearch = triangulation[tSearch, E12];
                    else if (!Math.IsPointOnRightSideOfLine(v2, v3, point.coords))
                        tSearch = triangulation[tSearch, E23];
                    else if (!Math.IsPointOnRightSideOfLine(v3, v1, point.coords))
                        tSearch = triangulation[tSearch, E31];
                    else
                    {
                        InsertPointIntoTriangle(point, tSearch, tLast);
                        tLast += 2;
                        tSearch = tLast;
                        pointInserted = true;
                    }
                }
            }
            return true;
        }

        protected TriangulationPoint[] SortPointsIntoBins()
        {
            int n = Mathf.RoundToInt(Mathf.Pow((float)N, 0.25f));
            int binCount = n * n;
            for (int k = 0; k < N; k++)
            {
                var point = this.points[k];
                int i = (int)(0.99f * n * point.coords.y);
                int j = (int)(0.99f * n * point.coords.x);
                point.bin = BinarySort.GetBinNumber(i, j, n);
            }

            return BinarySort.Sort<TriangulationPoint>(this.points, N, binCount);
        }

        protected void InsertPointIntoTriangle(TriangulationPoint p, int t, int triangleCount)
        {
            int t1 = t;
            int t2 = triangleCount + 1;
            int t3 = triangleCount + 2;
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
            UpdateAdjacency(triangulation[t, E12], t, t3);
            UpdateAdjacency(triangulation[t, E23], t, t2);
            triangulation[t1, V2] = triangulation[t, V3];
            triangulation[t1, V3] = triangulation[t, V1];
            triangulation[t1, V1] = p.index;
            triangulation[t1, E23] = triangulation[t, E31];
            triangulation[t1, E12] = t2;
            triangulation[t1, E31] = t3;
            RestoreDelauneyTriangulation(p, t1, t2, t3);
        }
        protected void UpdateAdjacency(int t, int tOld, int tNew)
        {
            int sharedEdge;
            if (t == OUT_OF_BOUNDS)
                return;
            else if (FindSharedEdge(t, tOld, out sharedEdge))
                triangulation[t, sharedEdge] = tNew;
        }
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
        protected void RestoreDelauneyTriangulation(TriangulationPoint p, int t1, int t2, int t3)
        {
            int t4;
            Stack<(int, int)> s = new Stack<(int, int)>();
            s.Push((t1, triangulation[t1, E23]));
            s.Push((t2, triangulation[t2, E23]));
            s.Push((t3, triangulation[t3, E23]));
            while (s.Count > 0)
            {
                (t1, t2) = s.Pop();
                if (t2 == OUT_OF_BOUNDS)
                    continue;

                else if (SwapQuadDiagonalIfNeeded(p.index, t1, t2, out t3, out t4))
                {
                    s.Push((t1, t3));
                    s.Push((t2, t4));
                }
            }
        }
        protected bool SwapQuadDiagonalIfNeeded(int p, int t1, int t2, out int t3, out int t4)
        {
            int q4 = p;
            int q1, q2, q3;
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
            else 
            {
                q1 = triangulation[t2, V1];
                q2 = triangulation[t2, V3];
                q3 = triangulation[t2, V2];
                t3 = triangulation[t2, E12];
                t4 = triangulation[t2, E23];
            }
            if (SwapTest(points[q1].coords, points[q2].coords, points[q3].coords, points[q4].coords))
            {
                UpdateAdjacency(t3, t2, t1);
                UpdateAdjacency(triangulation[t1, E31], t1, t2);
                triangulation[t1, V1] = q4;
                triangulation[t1, V2] = q1;
                triangulation[t1, V3] = q3;
                triangulation[t2, V1] = q4;
                triangulation[t2, V2] = q3;
                triangulation[t2, V3] = q2;
                triangulation[t2, E12] = t1;
                triangulation[t2, E23] = t4;
                triangulation[t2, E31] = triangulation[t1, E31];
                triangulation[t1, E23] = t3;
                triangulation[t1, E31] = t2;
                return true;
            }
            else
                return false;
        }
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
        internal void ApplyConstraints()
        {
            visited = new bool[triangulation.GetLength(0)];
            vertexTriangles = new int[N + 3];
            for (int i = 0; i < triangulation.GetLength(0); i++)
            {
                vertexTriangles[triangulation[i, V1]] = i;
                vertexTriangles[triangulation[i, V2]] = i;
                vertexTriangles[triangulation[i, V3]] = i;
            }
            foreach (EdgeConstraint constraint in constraints)
            {
                if (constraint.v1 == constraint.v2) continue;

                Queue<EdgeConstraint> intersectingEdges = FindIntersectingEdges(constraint, vertexTriangles);
                RemoveIntersectingEdges(constraint, intersectingEdges);
            }
        }
        internal Queue<EdgeConstraint> FindIntersectingEdges(EdgeConstraint constraint, int[] vertexTriangles)
        {
            Queue<EdgeConstraint> intersectingEdges = new Queue<EdgeConstraint>();
            EdgeConstraint startEdge;
            if (FindStartingEdge(vertexTriangles, constraint, out startEdge))
                intersectingEdges.Enqueue(startEdge);
            else
                return intersectingEdges;

            int t = startEdge.t1;
            int edgeIndex = startEdge.t1Edge;
            int lastTriangle = t;
            bool finalTriangleFound = false;
            while (!finalTriangleFound)
            {
                lastTriangle = t;
                t = triangulation[t, edgeIndex];
                Vector2 v_i = points[constraint.v1].coords;
                Vector2 v_j = points[constraint.v2].coords;
                Vector2 v1 = points[triangulation[t, V1]].coords;
                Vector2 v2 = points[triangulation[t, V2]].coords;
                Vector2 v3 = points[triangulation[t, V3]].coords;

                if (TriangleContainsVertex(t, constraint.v2))
                    finalTriangleFound = true;
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
                    Debug.LogWarning("Failed to find final triangle, exiting early.");
                    break;
                }
            }
            return intersectingEdges;
        }
        protected bool TriangleContainsVertex(int t, int v)
        {
            return triangulation[t, V1] == v || triangulation[t, V2] == v || triangulation[t, V3] == v;
        }
        internal bool FindStartingEdge(int[] vertexTriangles, EdgeConstraint constraint, out EdgeConstraint startingEdge)
        {
            startingEdge = new EdgeConstraint(-1, -1);
            int v_i = constraint.v1;
            int v_j = constraint.v2;
            int tSearch = vertexTriangles[v_i];
            for (int i = 0; i < visited.Length; i++)
            {
                visited[i] = false;
            }
            bool intersectionFound = false;
            bool noCandidatesFound = false;
            int intersectingEdgeIndex = E12;
            int tE12, tE23, tE31;
            while (!intersectionFound && !noCandidatesFound)
            {
                visited[tSearch] = true;
                if (TriangleContainsConstraint(tSearch, constraint))
                    return false;
                else if (EdgeConstraintIntersectsTriangle(tSearch, constraint, out intersectingEdgeIndex))
                    intersectionFound = true;
                    break;

                tE12 = triangulation[tSearch, E12];
                tE23 = triangulation[tSearch, E23];
                tE31 = triangulation[tSearch, E31];
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
        internal bool TriangleContainsConstraint(int t, EdgeConstraint constraint)
        {
            return (triangulation[t, V1] == constraint.v1 || triangulation[t, V2] == constraint.v1 || triangulation[t, V3] == constraint.v1) &&
                   (triangulation[t, V1] == constraint.v2 || triangulation[t, V2] == constraint.v2 || triangulation[t, V3] == constraint.v2);
        }
        internal void RemoveIntersectingEdges(EdgeConstraint constraint, Queue<EdgeConstraint> intersectingEdges)
        {
            List<EdgeConstraint> newEdges = new List<EdgeConstraint>();
            EdgeConstraint edge, newEdge;

            int counter = 0;

            while (intersectingEdges.Count > 0 && counter <= intersectingEdges.Count)
            {
                edge = intersectingEdges.Dequeue();

                Quad quad;
                if (FindQuadFromSharedEdge(edge.t1, edge.t1Edge, out quad))
                {
                    if (Math.LinesIntersect(points[quad.q4].coords,
                            points[quad.q3].coords,
                            points[quad.q1].coords,
                            points[quad.q2].coords))
                    {
                        SwapQuadDiagonal(quad, intersectingEdges, newEdges, constraints);
                        newEdge = new EdgeConstraint(quad.q3, quad.q4, quad.t1, quad.t2, E31);

                        if (Math.LinesIntersect(points[constraint.v1].coords,
                                points[constraint.v2].coords,
                                points[quad.q3].coords,
                                points[quad.q4].coords))
                        {
                            intersectingEdges.Enqueue(newEdge);
                        }
                        else
                        {
                            counter = 0;
                            newEdges.Add(newEdge);
                        }
                    }
                    else
                        intersectingEdges.Enqueue(edge);
                }

                counter++;
            }

            if (newEdges.Count > 0)
                RestoreConstrainedDelauneyTriangulation(constraint, newEdges);
        }
        internal void RestoreConstrainedDelauneyTriangulation(EdgeConstraint constraint, List<EdgeConstraint> newEdges)
        {
            bool swapOccurred = true;
            int counter = 0;
            while (swapOccurred)
            {
                counter++;
                swapOccurred = false;
                for (int i = 0; i < newEdges.Count; i++)
                {
                    EdgeConstraint edge = newEdges[i];
                    if (edge == constraint)
                        continue;
                    Quad quad;
                    if (FindQuadFromSharedEdge(edge.t1, edge.t1Edge, out quad))
                    {
                        if (SwapTest(points[quad.q1].coords, points[quad.q2].coords, points[quad.q3].coords, points[quad.q4].coords))
                        {
                            SwapQuadDiagonal(quad, newEdges, constraints, null);
                            int v_m = quad.q3;
                            int v_n = quad.q4;
                            newEdges[i] = new EdgeConstraint(v_m, v_n, quad.t1, quad.t2, E31);
                            swapOccurred = true;
                        }
                    }
                }
            }
        }
        internal bool FindQuadFromSharedEdge(int t1, int t1SharedEdge, out Quad quad)
        {      
            int q1, q2, q3, q4;
            int t1L, t1R, t2L, t2R;
            int t2 = triangulation[t1, t1SharedEdge];
            int t2SharedEdge;
            if (FindSharedEdge(t2, t1, out t2SharedEdge))
            {
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
                else
                {
                    q2 = triangulation[t2, V3];
                    q1 = triangulation[t2, V1];
                    q3 = triangulation[t2, V2];
                }
                q4 = triangulation[t1, oppositePoint[t1SharedEdge]];
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
        internal void SwapQuadDiagonal(Quad quad, IEnumerable<EdgeConstraint> edges1, IEnumerable<EdgeConstraint> edges2, IEnumerable<EdgeConstraint> edges3)
        {
            int t1 = quad.t1;
            int t2 = quad.t2;
            int t1R = quad.t1R;
            int t1L = quad.t1L;
            int t2R = quad.t2R;
            int t2L = quad.t2L;
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
            UpdateAdjacency(t2L, t2, t1);
            UpdateAdjacency(t1R, t1, t2);
            UpdateEdgesAfterSwap(edges1, t1, t2, t1L, t1R, t2L, t2R);
            UpdateEdgesAfterSwap(edges2, t1, t2, t1L, t1R, t2L, t2R);
            UpdateEdgesAfterSwap(edges3, t1, t2, t1L, t1R, t2L, t2R);
            vertexTriangles[quad.q1] = t1;
            vertexTriangles[quad.q2] = t2;
        }
        internal void UpdateEdgesAfterSwap(IEnumerable<EdgeConstraint> edges, int t1, int t2, int t1L, int t1R, int t2L, int t2R)
        {
            if (edges == null)
                return;

            foreach (EdgeConstraint edge in edges)
            {
                if (edge.t1 == t1 && edge.t2 == t1R)
                {
                    edge.t1 = t2;
                    edge.t2 = t1R;
                    edge.t1Edge = E31;
                }
                else if (edge.t1 == t1 && edge.t2 == t1L)
                    edge.t1Edge = E12;
                else if (edge.t1 == t1R && edge.t2 == t1)
                    edge.t2 = t2;
                else if (edge.t1 == t1L && edge.t2 == t1)
                {
                    // Unchanged
                }
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
        internal void DiscardTrianglesViolatingConstraints()
        {
            for (int i = 0; i < triangleCount; i++)
            {
                skipTriangle[i] = true;
            }
            HashSet<(int, int)> boundaries = new HashSet<(int, int)>();
            for (int i = 0; i < this.constraints.Count; i++)
            {
                EdgeConstraint constraint = this.constraints[i];
                boundaries.Add((constraint.v1, constraint.v2));
            }
            for (int i = 0; i < visited.Length; i++)
            {
                visited[i] = false;
            }
            Queue<int> frontier = new Queue<int>();
            int v1, v2, v3;
            bool boundaryE12, boundaryE23, boundaryE31;
            for (int i = 0; i < triangleCount; i++)
            {
                if (visited[i])
                    continue;
                v1 = triangulation[i, V1];
                v2 = triangulation[i, V2];
                v3 = triangulation[i, V3];
                boundaryE12 = boundaries.Contains((v1, v2));
                boundaryE23 = boundaries.Contains((v2, v3));
                boundaryE31 = boundaries.Contains((v3, v1));

                if (boundaryE12 || boundaryE23 || boundaryE31)
                {
                    skipTriangle[i] = false;
                    frontier.Clear();
                    if (!boundaryE12)
                        frontier.Enqueue(triangulation[i, E12]);
                    if (!boundaryE23)
                        frontier.Enqueue(triangulation[i, E23]);
                    if (!boundaryE31)
                        frontier.Enqueue(triangulation[i, E31]);
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
        protected void DiscardTrianglesWithSuperTriangleVertices()
        {
            for (int i = 0; i < triangleCount; i++)
            {
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
