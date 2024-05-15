using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;
using static UnityEditor.Progress;

namespace UnityFracture
{
    public static class Fracturer
    {
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
                                    Material insideMat)
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
            foreach (MeshData meshData in fragments)
            {
                CreateFragement(meshData,
                                sourceObject,
                                fractureTemplate,
                                parent,
                                ref i);
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
        public static void CreateFragement(MeshData meshData,
                                           GameObject sourceObject,
                                           GameObject template,
                                           Transform parent,
                                           ref int i)
        {
            // If there is no mesh data, don't create an object
            if (meshData.triangles.Length == 0) return;

            Mesh[] meshes;
            Mesh fragmentMesh = meshData.ToMesh();

            var parentSize = sourceObject.GetComponent<MeshFilter>().sharedMesh.bounds.size;
            var parentMass = 1f;
            if (sourceObject.GetComponent<Rigidbody>())
                parentMass = sourceObject.GetComponent<Rigidbody>().mass;

            GameObject fragment = GameObject.Instantiate(template, parent);
            fragment.name = $"Fragment{i}";
            fragment.transform.localPosition = Vector3.zero;
            fragment.transform.localRotation = Quaternion.identity;
            fragment.transform.localScale = sourceObject.transform.localScale;

            fragmentMesh.name = Guid.NewGuid().ToString();

            // Update mesh to the new sliced mesh
            var meshFilter = fragment.GetComponent<MeshFilter>();
            meshFilter.sharedMesh = fragmentMesh;

            var collider = fragment.GetComponent<MeshCollider>();

            // If fragment collisions are disabled, collider will be null
            collider.sharedMesh = fragmentMesh;
            collider.convex = true;
            collider.sharedMaterial = fragment.GetComponent<Collider>().sharedMaterial;

            // Compute mass of the sliced object by dividing mesh bounds by density
            var rigidBody = fragment.GetComponent<Rigidbody>();
            var size = fragmentMesh.bounds.size;
            float density = (parentSize.x * parentSize.y * parentSize.z) / parentMass;
            rigidBody.mass = (size.x * size.y * size.z) / density;

            i++;

        }

        /// <summary>
        /// Creates the template for the fragment
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="insideMat">The inside mat.</param>
        /// <param name="keepTag">if set to <c>true</c> [keep tag].</param>
        /// <returns></returns>
        public static GameObject CreateTemplate(GameObject sender, Material insideMat, bool keepTag = true)
        {
            // create the object
            GameObject obj = new("Fragment");
            // if need to keep the same tag then set the tag
            if (keepTag)
                obj.tag = sender.tag;

            // copy over the mesh render & filter
            obj.AddComponent<MeshFilter>();
            var meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = new Material[2] { sender.GetComponent<MeshRenderer>().sharedMaterial,
                insideMat};

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
    }
}