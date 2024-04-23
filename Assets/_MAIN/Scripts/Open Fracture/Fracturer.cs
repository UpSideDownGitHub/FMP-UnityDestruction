using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;
using static UnityEditor.Progress;

namespace UnityFracture
{
    public static class Fracturer
    {
        public static void Fracture(GameObject sourceObject,
                                    GameObject fractureTemplate,
                                    Transform parent,
                                    int fragmentCount,
                                    Material insideMat)
        {
            // CHANGED THE FOLLOWING LINE TO MESH insted of SHARED MESH
            MeshData sourceMesh = new MeshData(sourceObject.GetComponent<MeshFilter>().mesh);
            var fragments = new Queue<MeshData>();
            fragments.Enqueue(sourceMesh);

            MeshData topSlice, bottomSlice;
            while (fragments.Count < fragmentCount)
            {
                MeshData meshData = fragments.Dequeue();
                meshData.Calculatebounds();
                Vector3 normal = new Vector3(
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(-1f, 1f));

                Slicer.Slice(meshData,
                                 normal,
                                 meshData.bounds.center,
                                 out topSlice,
                                 out bottomSlice);

                fragments.Enqueue(topSlice);
                fragments.Enqueue(bottomSlice);
            }

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

        public static void CreateFragement(MeshData meshData,
                                           GameObject sourceObject,
                                           GameObject template,
                                           Transform parent,
                                           ref int i)
        {

            if (meshData.triangles.Length == 0) return;

            Mesh[] meshes;
            Mesh fragmentMesh = meshData.ToMesh();

            meshes = new Mesh[] { fragmentMesh }; // remove this line once below done
                                                  // calculate the floating meshes

            var parentSize = sourceObject.GetComponent<MeshFilter>().sharedMesh.bounds.size;
            var parentMass = 1f;
            if (sourceObject.GetComponent<Rigidbody>())
                parentMass = sourceObject.GetComponent<Rigidbody>().mass;

            for (int k = 0; k < meshes.Length; k++)
            {
                GameObject fragment = GameObject.Instantiate(template, parent);
                fragment.name = $"Fragment{i}";
                fragment.transform.localPosition = Vector3.zero;
                fragment.transform.localRotation = Quaternion.identity;
                fragment.transform.localScale = sourceObject.transform.localScale;

                meshes[k].name = Guid.NewGuid().ToString();

                var meshFilter = fragment.GetComponent<MeshFilter>();
                meshFilter.sharedMesh = meshes[k];

                var collider = fragment.GetComponent<MeshCollider>();

                collider.sharedMesh = meshes[k];
                collider.convex = true;
                collider.sharedMaterial = fragment.GetComponent<Collider>().sharedMaterial;

                var rigidBody = fragment.GetComponent<Rigidbody>();

                var size = fragmentMesh.bounds.size;
                float density = (parentSize.x * parentSize.y * parentSize.z) / parentMass;
                rigidBody.mass = (size.x * size.y * size.z) / density;

                i++;
            }
        }
    }
}