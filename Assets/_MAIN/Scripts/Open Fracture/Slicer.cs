using UnityEngine;

namespace UnityFracture
{
    public static class Slicer
    {
        public static void Slice(MeshData meshData,
                                 Vector3 sliceNormal,
                                 Vector3 sliceOrigin,
                                 out MeshData topSlice,
                                 out MeshData bottomSlice)
        {
            topSlice = new MeshData(meshData.vertexCount, meshData.triangleCount);
            bottomSlice = new MeshData(meshData.vertexCount, meshData.triangleCount);

            bool[] side = new bool[meshData.vertexCount];

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

            FillCutFaces(topSlice, bottomSlice, -sliceNormal);

        }

        public static void SplitTriangles(MeshData meshData,
                                          MeshData topSlice,
                                          MeshData bottomSlice,
                                          Vector3 sliceNormal,
                                          Vector3 sliceOrigin,
                                          bool[] side,
                                          int index)
        {
            int[] triangles = meshData.Gettriangles(index);

            int a, b, c;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                a = triangles[i];
                b = triangles[i + 1];
                c = triangles[i + 2];

                if (side[a] && side[b] && side[c])
                    topSlice.AddMappedTriangle(a, b, c, index);
                else if (!side[a] && !side[b] && !side[c])
                    bottomSlice.AddMappedTriangle(a, b, c, index);
                else
                {
                    if (side[b] && side[c] && !side[a])
                    {
                        SplitTriangle(b, c, a, sliceNormal, sliceOrigin, meshData, topSlice, bottomSlice, index, true);
                    }
                    else if (side[c] && side[a] && !side[b])
                    {
                        SplitTriangle(c, a, b, sliceNormal, sliceOrigin, meshData, topSlice, bottomSlice, index, true);
                    }
                    else if (side[a] && side[b] && !side[c])
                    {
                        SplitTriangle(a, b, c, sliceNormal, sliceOrigin, meshData, topSlice, bottomSlice, index, true);
                    }
                    else if (!side[b] && !side[c] && side[a])
                    {
                        SplitTriangle(b, c, a, sliceNormal, sliceOrigin, meshData, topSlice, bottomSlice, index, false);
                    }
                    else if (!side[c] && !side[a] && side[b])
                    {
                        SplitTriangle(c, a, b, sliceNormal, sliceOrigin, meshData, topSlice, bottomSlice, index, false);
                    }
                    else if (!side[a] && !side[b] && side[c])
                    {
                        SplitTriangle(a, b, c, sliceNormal, sliceOrigin, meshData, topSlice, bottomSlice, index, false);
                    }
                }
            }
        }

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
                var norm13 = (v1.normal + s13 * (v3.normal - v1.normal)).normalized;
                var norm23 = (v2.normal + s23 * (v3.normal - v2.normal)).normalized;
                var uv13 = v1.uv + s13 * (v3.uv - v1.uv);
                var uv23 = v2.uv + s23 * (v3.uv - v2.uv);

                topSlice.AddCutFaceVertex(v13, norm13, uv13);
                topSlice.AddCutFaceVertex(v23, norm23, uv23);
                bottomSlice.AddCutFaceVertex(v13, norm13, uv13);
                bottomSlice.AddCutFaceVertex(v23, norm23, uv23);

                int index13_A = topSlice.vertices.Count - 2;
                int index23_A = topSlice.vertices.Count - 1;
                int index13_B = bottomSlice.vertices.Count - 2;
                int index23_B = bottomSlice.vertices.Count - 1;

                if (v3BelowCutPlane)
                {
                    topSlice.AddTriangle(index23_A, index13_A, topSlice.indexMap[v2Index], index);
                    topSlice.AddTriangle(index13_A, topSlice.indexMap[v1Index], topSlice.indexMap[v2Index], index);
                    bottomSlice.AddTriangle(bottomSlice.indexMap[v3Index], index13_B, index23_B, index);
                    topSlice.constraints.Add(new EdgeConstraint(topSlice.cutvertices.Count - 2, topSlice.cutvertices.Count - 1));
                    bottomSlice.constraints.Add(new EdgeConstraint(bottomSlice.cutvertices.Count - 1, bottomSlice.cutvertices.Count - 2));
                }
                else
                {
                    topSlice.AddTriangle(index13_A, index23_A, topSlice.indexMap[v3Index], index);
                    bottomSlice.AddTriangle(bottomSlice.indexMap[v1Index], bottomSlice.indexMap[v2Index], index13_B, index);
                    bottomSlice.AddTriangle(bottomSlice.indexMap[v2Index], index23_B, index13_B, index);
                    topSlice.constraints.Add(new EdgeConstraint(topSlice.cutvertices.Count - 1, topSlice.cutvertices.Count - 2));
                    bottomSlice.constraints.Add(new EdgeConstraint(bottomSlice.cutvertices.Count - 2, bottomSlice.cutvertices.Count - 1));
                }
            }
        }

        public static bool LinePlaneIntersection(Vector3 a,
                                             Vector3 b,
                                             Vector3 n,
                                             Vector3 p0,
                                             out Vector3 x,
                                             out float s)
        {
            s = 0;
            x = Vector3.zero;
            if (a == b)
                return false;
            else if (n == Vector3.zero)
                return false;
            s = Vector3.Dot(p0 - a, n) / Vector3.Dot(b - a, n);
            if (s >= 0 && s <= 1)
            {
                x = a + (b - a) * s;
                return true;
            }
            return false;
        }

        public static void FillCutFaces(MeshData topSlice,
                                        MeshData bottomSlice,
                                        Vector3 sliceNormal)
        {
            topSlice.WeldCutFacevertices();
            if (topSlice.cutvertices.Count < 3) return;

            var triangulator = new ConstrainedTriangluator(topSlice.cutvertices, topSlice.constraints, sliceNormal);
            int[] triangles = triangulator.Triangulate();

            // Update normal and UV for the cut face vertices
            for (int i = 0; i < topSlice.cutvertices.Count; i++)
            {
                var vertex = topSlice.cutvertices[i];
                var point = triangulator.points[i];

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

            int offsetTop = topSlice.vertices.Count;
            int offsetBottom = bottomSlice.vertices.Count;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                topSlice.AddTriangle(
                    offsetTop + triangles[i],
                    offsetTop + triangles[i + 1],
                    offsetTop + triangles[i + 2],
                    0);

                bottomSlice.AddTriangle(
                    offsetBottom + triangles[i],
                    offsetBottom + triangles[i + 2],
                    offsetBottom + triangles[i + 1],
                    1);
            }
        }
    }
}