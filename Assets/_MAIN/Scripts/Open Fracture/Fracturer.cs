using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace ReubenMiller.Fracture
{
    public static class Fracturer
    {



        /// <summary>
        /// Asynchronously generates the mesh fragments based on the provided options. The generated fragment objects are
        /// stored as children of `fragmentParent`
        /// </summary>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="fractureTemplate">The fracture template.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="fragmentCount">The fragment count.</param>
        /// <param name="insideMat">The inside mat.</param>
        public static IEnumerator FractureAsync(GameObject sourceObject,
                                    GameObject fractureTemplate,
                                    Transform parent,
                                    int fragmentCount,
                                    bool detectFloating,
                                    Action onCompletion)
        {
            // Define our source mesh data for the fracturing
            MeshData sourceMesh = new MeshData(sourceObject.GetComponent<MeshFilter>().sharedMesh);
            yield return null;
            // We begin by fragmenting the source mesh, then process each fragment in a FIFO queue
            // until we achieve the target fragment count.
            var fragments = new Queue<MeshData>();
            fragments.Enqueue(sourceMesh);

            // Subdivide the mesh into multiple fragments until we reach the fragment limit
            MeshData topSlice, bottomSlice;
            while (fragments.Count < fragmentCount)
            {
                MeshData meshData = fragments.Dequeue();
                meshData.Calculatebounds();

                // Select an arbitrary fracture plane normal
                Vector3 normal = new Vector3(
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(-1f, 1f));

                // slice the mesh along this normal
                Slicer.Slice(meshData,
                                 normal,
                                 meshData.bounds.center,
                                 out topSlice,
                                 out bottomSlice);

                // perform next slice on the next frame
                yield return null;

                fragments.Enqueue(topSlice);
                fragments.Enqueue(bottomSlice);
            }

            // create all of the fragments
            int i = 0;
            List<Mesh> meshes = new();
            foreach (MeshData meshData in fragments)
            {
                yield return null; // DELETE ME
                meshes.Add(meshData.ToMesh());
            }
            var colliderMeshes = CreateCollidersFast(meshes.ToArray());
            foreach (Mesh meshData in colliderMeshes)
            {
                yield return null;
                CreateFragement(meshData,
                                sourceObject,
                                fractureTemplate,
                                parent,
                                detectFloating,
                                ref i);
            }
            onCompletion?.Invoke();
        }

        /// <summary>
        /// Generates the mesh fragments based on the provided options. The generated fragment objects are
        /// stored as children of `fragmentParent`
        /// </summary>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="fractureTemplate">The fracture template.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="fragmentCount">The fragment count.</param>
        /// <param name="insideMat">The inside mat.</param>
        public static void Fracture(GameObject sourceObject,
                                    GameObject fractureTemplate,
                                    Transform parent,
                                    int fragmentCount,
                                    bool detectFloating)
        {
            // Define our source mesh data for the fracturing
            MeshData sourceMesh = new MeshData(sourceObject.GetComponent<MeshFilter>().sharedMesh);

            // We begin by fragmenting the source mesh, then process each fragment in a FIFO queue
            // until we achieve the target fragment count.
            var fragments = new Queue<MeshData>();
            fragments.Enqueue(sourceMesh);

            // Subdivide the mesh into multiple fragments until we reach the fragment limit
            MeshData topSlice, bottomSlice;
            while (fragments.Count < fragmentCount)
            {
                MeshData meshData = fragments.Dequeue();
                meshData.Calculatebounds();

                // Select an arbitrary fracture plane normal
                Vector3 normal = new Vector3(
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(-1f, 1f));

                // slice the mesh along this normal
                Slicer.Slice(meshData,
                                 normal,
                                 meshData.bounds.center,
                                 out topSlice,
                                 out bottomSlice);

                fragments.Enqueue(topSlice);
                fragments.Enqueue(bottomSlice);
            }

            // create all of the fragments
            int i = 0;

            // convert all of the meshes to meshes, and generate their colliders fast
            List<Mesh> meshes = new();
            foreach (MeshData meshData in fragments)
            {
                meshes.Add(meshData.ToMesh());
            }
            var colliderMeshes = CreateCollidersFast(meshes.ToArray());
            int j = 0;
            foreach (MeshData meshData in fragments)
            {
                CreateFragement(colliderMeshes[j],
                                sourceObject,
                                fractureTemplate,
                                parent,
                                detectFloating,
                                ref i);
                j++;
            }

        }

        /// <summary>
        /// Creates a new GameObject from the fragment data
        /// </summary>
        /// <param name="meshData">The mesh data.</param>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="template">The template.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="i">The i.</param>
        public static void CreateFragement(Mesh fragmentMesh,
                                           GameObject sourceObject,
                                           GameObject template,
                                           Transform parent,
                                           bool detectFloating,
                                           ref int i)
        {
            // If there is no mesh data, don't create an object
            if (fragmentMesh.triangles.Length == 0) return;

            Mesh[] meshes;

            if (detectFloating)
            { 
                meshes = FindDisconnectedMeshes(fragmentMesh);
                meshes = CreateCollidersFast(meshes);
            }
            else
                meshes = new Mesh[] { fragmentMesh };

            for (int k = 0; k < meshes.Length; k++)
            {
                var parentSize = sourceObject.GetComponent<MeshFilter>().sharedMesh.bounds.size;
                var parentMass = 1f;
                if (sourceObject.GetComponent<Rigidbody>())
                    parentMass = sourceObject.GetComponent<Rigidbody>().mass;

                GameObject fragment = GameObject.Instantiate(template, parent);
                fragment.name = $"Fragment{i}";
                fragment.transform.localPosition = Vector3.zero;
                fragment.transform.localRotation = Quaternion.identity;
                fragment.transform.localScale = sourceObject.transform.localScale;

                meshes[k].name = Guid.NewGuid().ToString();

                // Update mesh to the new sliced mesh
                var meshFilter = fragment.GetComponent<MeshFilter>();
                meshFilter.sharedMesh = meshes[k];                

                var collider = fragment.GetComponent<MeshCollider>();

                // If fragment collisions are disabled, collider will be null
                collider.sharedMesh = meshes[k];
                collider.convex = true; // THIS MIGHT CAUSE AND ISSUE, BUT SHOULDNT AS TELLING IT TO GEN CONVEX
                collider.sharedMaterial = fragment.GetComponent<Collider>().sharedMaterial;

                // Compute mass of the sliced object by dividing mesh bounds by density
                var rigidBody = fragment.GetComponent<Rigidbody>();
                var size = meshes[k].bounds.size;
                float density = (parentSize.x * parentSize.y * parentSize.z) / parentMass;
                rigidBody.mass = (size.x * size.y * size.z) / density;

                fragment.SetActive(false);

                i++;
            }
        }

        /// <summary>
        /// Creates the template for the fragment
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="insideMat">The inside mat.</param>
        /// <param name="keepTag">if set to <c>true</c> [keep tag].</param>
        /// <returns></returns>
        public static GameObject CreateTemplate(GameObject sender, Material insideMat, bool copyMaterials = false, bool keepTag = true)
        {
            // create the object
            GameObject obj = new("Fragment");
            // if need to keep the same tag then set the tag
            if (keepTag)
                obj.tag = sender.tag;

            // copy over the mesh render & filter
            obj.AddComponent<MeshFilter>();
            var meshRenderer = obj.AddComponent<MeshRenderer>();
            if (copyMaterials)
                meshRenderer.sharedMaterials = sender.GetComponent<MeshRenderer>().sharedMaterials;
            else
                meshRenderer.sharedMaterials = new Material[2] { sender.GetComponent<MeshRenderer>().sharedMaterial, insideMat };

            // copy over the collider
            var thisCollider = sender.GetComponent<Collider>();
            var fragmentCollider = obj.AddComponent<MeshCollider>();
            fragmentCollider.convex = true;
            fragmentCollider.sharedMaterial = thisCollider.sharedMaterial;
            fragmentCollider.isTrigger = thisCollider.isTrigger;

            // copy over the rigidbody
            var thisRigidBody = sender.GetComponent<Rigidbody>();
            var fragmentRigidBody = obj.AddComponent<Rigidbody>();
            fragmentRigidBody.velocity = thisRigidBody.velocity;
            fragmentRigidBody.angularVelocity = thisRigidBody.angularVelocity;
            fragmentRigidBody.drag = thisRigidBody.drag;
            fragmentRigidBody.angularDrag = thisRigidBody.angularDrag;
            fragmentRigidBody.useGravity = thisRigidBody.useGravity;

            // return the created object
            return obj;
        }

        public static Mesh[] CreateCollidersFast(Mesh[] meshes, int meshesPerJob = 1)
        {
            NativeArray<int> meshIds = new NativeArray<int>(meshes.Length, Allocator.TempJob);

            for (int i = 0; i < meshes.Length; ++i)
            {
                meshIds[i] = meshes[i].GetInstanceID();
            }

            // This spreads the expensive operation over all cores.
            var job = new BakeJob(meshIds);
            job.Schedule(meshIds.Length, meshesPerJob).Complete();

            meshIds.Dispose();

            return meshes;
        }

        public struct BakeJob : IJobParallelFor
        {
            private NativeArray<int> meshIds;

            public BakeJob(NativeArray<int> meshIds)
            {
                this.meshIds = meshIds;
            }

            public void Execute(int index)
            {
                Physics.BakeMesh(meshIds[index], true, MeshColliderCookingOptions.UseFastMidphase);
            }
        }

        // Description of vertex attributes for the island mesh
        private static VertexAttributeDescriptor[] layout = new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
        };

        /// <summary>
        /// Identifies all disconnected sets of geometry contained within the mesh.
        /// Each set of geometry is split into a separate meshes. 
        /// </summary>
        /// <param name="mesh">The mesh to search</param>
        /// <returns>Returns an array of all disconnected meshes found.</returns>
        public static Mesh[] FindDisconnectedMeshes(Mesh mesh)
        {
            // Each disconnected set of geometry is referred to as an "island"
            List<Mesh> islands = new List<Mesh>();

            // Extract mesh data
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;
            var normals = mesh.normals;
            var uvs = mesh.uv;

            // For each triangle, find the corresponding sub-mesh index. (Mesh.triangles contains
            // the triangles for all sub-meshes)
            int[] triangleSubMesh = new int[triangles.Length / 3];
            int subMeshIndex = 0;
            int subMeshSize = mesh.GetTriangles(subMeshIndex).Length / 3;
            for (int i = 0; i < triangles.Length / 3; i++)
            {
                if (i >= subMeshSize)
                {
                    subMeshIndex++;
                    subMeshSize += mesh.GetTriangles(subMeshIndex).Length / 3;
                }
                triangleSubMesh[i] = subMeshIndex;
            }

            // Identify coincident vertices
            List<int>[] coincidentVertices = new List<int>[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                coincidentVertices[i] = new List<int>();
            }
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v_i = vertices[i];
                for (int k = i + 1; k < vertices.Length; k++)
                {
                    Vector3 v_k = vertices[k];
                    if (v_i == v_k)
                    {
                        coincidentVertices[k].Add(i);
                        coincidentVertices[i].Add(k);
                    }
                }
            }

            // Find the triangles the each vertex belongs to. Need to do this for each submesh
            List<int>[] vertexTriangles = new List<int>[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                vertexTriangles[i] = new List<int>();
            }

            int v1, v2, v3;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                // Index of the triangle
                int t = i / 3;

                v1 = triangles[i];
                v2 = triangles[i + 1];
                v3 = triangles[i + 2];

                vertexTriangles[v1].Add(t);
                vertexTriangles[v2].Add(t);
                vertexTriangles[v3].Add(t);
            }

            // Search the mesh geometry and identify all islands
            // 1) Start by finding a vertex that has not yet been visited
            // 2) Insert the vertex into a queue, begin a breadth-first search
            // 3) Dequeue the next vertex 'v'
            // 4) Find all triangles that 'v' is connected to. Add each triangle to a list
            // 5) Enqueue the vertices for each connected triangle if they haven't been visited yet
            // 6) Enqueue all vertices coincident with 'v' if they haven't been visited yet
            // 7) Repeat Steps 3-6 until the queue is empty
            // 8) Take the list of triangles and use the existing mesh data to create a new island mesh
            // 9) Go back to Step 1, continue until all vertices have been visited.

            bool[] visitedVertices = new bool[vertices.Length];
            bool[] visitedTriangles = new bool[triangles.Length];
            Queue<int> frontier = new Queue<int>();

            // Vertex data for the island mesh. Only initialize once and keep track of pointer to last element to minimize GC
            NativeArray<MeshVertex> islandVertices = new NativeArray<MeshVertex>(vertices.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            // Array containing triangle data for the island mesh. Need to keep track of triangles for each sub-mesh separately
            int[][] islandTriangles = new int[mesh.subMeshCount][];
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                islandTriangles[i] = new int[triangles.Length];
            }

            // Counters to keep track of how many vertices
            int vertexCount = 0;
            int totalIndexCount = 0;
            int[] subMeshIndexCounts = new int[mesh.subMeshCount];

            for (int i = 0; i < vertices.Length; i++)
            {
                if (visitedVertices[i]) continue;

                // Reset the vertex/triangle counts
                vertexCount = 0;
                totalIndexCount = 0;
                for (int j = 0; j < mesh.subMeshCount; j++)
                {
                    subMeshIndexCounts[j] = 0;
                }

                // Search the mesh geometry starting at vertex 'i'. Search is performed by looking up
                // the triangles that contain each vertex, adding their vertices, etc. until all
                // triangles have been visited.
                frontier.Enqueue(i);

                // Index map between source mesh vertex array and the sub mesh vertex arrays
                int[] vertexMap = new int[vertices.Length];
                // Initialize map to '-1' to serve as "unmapped" value
                for (int j = 0; j < vertices.Length; j++)
                {
                    vertexMap[j] = -1;
                }

                while (frontier.Count > 0)
                {
                    int k = frontier.Dequeue();

                    // Ignore vertex if we've already visited it
                    if (visitedVertices[k])
                        continue;
                    else
                        visitedVertices[k] = true;

                    // Add this vertex array for the island mesh
                    // Map between the original vertex index to the vertex's new index in the island
                    // mesh vertex array. This will be used to update the indices for the triangles later
                    vertexMap[k] = vertexCount;
                    islandVertices[vertexCount++] = new MeshVertex(vertices[k], normals[k], uvs[k]);

                    // Get the list of all triangles that this vertex is a part of
                    foreach (int t in vertexTriangles[k])
                    {
                        // If triangle is already included, skip it
                        if (!visitedTriangles[t])
                        {
                            visitedTriangles[t] = true;

                            // Loop through each vertex of the triangle and add the non-visited ones
                            // to the search frontier
                            for (int m = t * 3; m < t * 3 + 3; m++)
                            {
                                int v = triangles[m];
                                subMeshIndex = triangleSubMesh[t];
                                islandTriangles[subMeshIndex][subMeshIndexCounts[subMeshIndex]++] = v;
                                totalIndexCount++;

                                frontier.Enqueue(v);

                                // If this vertex is coincident with other vertices, add those to the search frontier
                                foreach (int cv in coincidentVertices[v])
                                {
                                    frontier.Enqueue(cv);
                                }
                            }
                        }
                    }
                }

                // If the island contains at least one triangle, create a new mesh
                if (vertexCount > 0)
                {
                    Mesh island = new Mesh();

                    island.SetIndexBufferParams(totalIndexCount, IndexFormat.UInt32);
                    island.SetVertexBufferParams(vertexCount, layout);
                    island.SetVertexBufferData(islandVertices, 0, 0, vertexCount);

                    // Set the triangles for each submesh
                    island.subMeshCount = mesh.subMeshCount;
                    int indexStart = 0;
                    for (subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++)
                    {
                        var subMeshIndexBuffer = islandTriangles[subMeshIndex];
                        var subMeshIndexCount = subMeshIndexCounts[subMeshIndex];

                        // Map vertex indexes from the original mesh to the island mesh
                        for (int k = 0; k < subMeshIndexCount; k++)
                        {
                            int originalIndex = subMeshIndexBuffer[k];
                            subMeshIndexBuffer[k] = vertexMap[originalIndex];
                        }

                        // Set the index data for this sub mesh
                        island.SetIndexBufferData(subMeshIndexBuffer, 0, indexStart, (int)subMeshIndexCount);
                        island.SetSubMesh(subMeshIndex, new SubMeshDescriptor(indexStart, subMeshIndexCount));

                        indexStart += subMeshIndexCount;
                    }

                    island.RecalculateBounds();

                    islands.Add(island);
                }
            }

            // Loop through rest of triangles
            return islands.ToArray();
        }
    }
}