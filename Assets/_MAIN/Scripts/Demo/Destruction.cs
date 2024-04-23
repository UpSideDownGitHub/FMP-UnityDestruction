using System;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace UnityFracture.Demo
{
    public class Destruction : MonoBehaviour
    {
        [Header("Fracture Options")]
        [Range(1, 1024)]
        public int fragmentCount;
        public Material insideMat;

        [Header("CSG options")]
        public GameObject parent;

        [Header("Trigger Options")]
        public string triggerTag;
        public float minCollisionForce;

        private GameObject fragmentRoot;

        public void OnCollisionEnter(Collision collision)
        {
            if (collision.contactCount > 0)
            {
                float collisionForce = collision.impulse.magnitude / Time.fixedDeltaTime;
                if (collision.collider.CompareTag(triggerTag) &&
                    collisionForce > minCollisionForce)
                {
                    // set to default position
                    Vector3 prevPos = parent.transform.position;
                    Quaternion prevRot = parent.transform.rotation;
                    Vector3 prevSca = parent.transform.localScale;
                    parent.transform.localScale = Vector3.one;
                    parent.transform.position = Vector3.zero;
                    parent.transform.rotation = Quaternion.identity;

                    // remove this part of the object from the visual mesh
                    Model result = CSG.Subtract(parent, gameObject);
                    parent.GetComponent<MeshFilter>().sharedMesh = result.mesh;
                    parent.GetComponent<MeshRenderer>().sharedMaterials = result.materials.ToArray();

                    // fracture the part that fell off
                    FractureThis(collision.gameObject.GetComponent<Rigidbody>());

                    // reset the position of the parent
                    parent.transform.position = prevPos;
                    parent.transform.localScale = prevSca;
                    parent.transform.rotation = prevRot;

                    // Kill this object
                    Destroy(gameObject);
                }
            }
        }

        public void FractureThis(Rigidbody collisionRB)
        {
            var mesh = GetComponent<MeshFilter>().sharedMesh;
            if (mesh != null)
            {
                fragmentRoot = new GameObject($"{name}Fragments");
                fragmentRoot.transform.SetParent(transform.parent);

                fragmentRoot.transform.position = transform.position;
                fragmentRoot.transform.rotation = transform.rotation;
                fragmentRoot.transform.localScale = Vector3.one;

                GameObject fragmentTemplate = CreateTemplate(collisionRB);

                Fracturer.Fracture(gameObject,
                    fragmentTemplate,
                    fragmentRoot.transform,
                    fragmentCount,
                    insideMat);

                Destroy(fragmentTemplate);
                gameObject.SetActive(false);
            }
        }

        public GameObject CreateTemplate(Rigidbody collisionRB)
        {
            GameObject obj = new("Fragment") { tag = tag };
            obj.AddComponent<MeshFilter>();

            var meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = new Material[2] { 
                GetComponent<MeshRenderer>().sharedMaterial,
                insideMat};

            var thisCollider = GetComponent<Collider>();
            var fragmentCollider = obj.AddComponent<MeshCollider>();
            fragmentCollider.convex = true;
            fragmentCollider.sharedMaterial = thisCollider.sharedMaterial;
            fragmentCollider.isTrigger = thisCollider.isTrigger;

            var fragmentRigidBody = obj.AddComponent<Rigidbody>();
            fragmentRigidBody.velocity = collisionRB.velocity;
            fragmentRigidBody.angularVelocity = collisionRB.angularVelocity;
            fragmentRigidBody.drag = collisionRB.drag;
            fragmentRigidBody.angularDrag = collisionRB.angularDrag;
            fragmentRigidBody.useGravity = collisionRB.useGravity;

            return obj;
        }
    }
}
