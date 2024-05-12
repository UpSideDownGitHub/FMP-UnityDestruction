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

        //[Header("CSG options")]
        //public GameObject mainMesh;
        //public GameObject collisionHolder;

        [Header("Trigger Options")]
        public string triggerTag;

        private GameObject fragmentRoot;
        private Vector3 ogPos;
        private Quaternion ogRot;
        private Vector3 ogSca;

        public void OnCollisionEnter(Collision collision)
        {
            if (collision.contactCount > 0)
            {
                if (collision.collider.CompareTag(triggerTag))
                {
                    Destroy(GetComponent<Rigidbody>(), collision.gameObject);
                }
            }
        }

        public void Start()
        {
            // set to default position

        }

        public void Destroy(Rigidbody collision, GameObject colObject)
        {
            // save size and set to 0
            var ogPos = colObject.transform.position;
            var ogRot = colObject.transform.rotation;
            var ogSca = colObject.transform.localScale;
            colObject.transform.localScale = Vector3.one;
            colObject.transform.position = Vector3.zero;
            colObject.transform.rotation = Quaternion.identity;

            // remove this part of the object from the visual mesh
            Model result = CSG.Subtract(colObject, gameObject);
            Instantiate((Mesh)result, Vector3.zero, Quaternion.identity);
            colObject.GetComponent<MeshFilter>().mesh = result.mesh;
            colObject.GetComponent<MeshRenderer>().materials = result.materials.ToArray();
            colObject.GetComponent<MeshFilter>().mesh.Optimize();

            // fracture the part that fell off
            //FractureThis(collision.gameObject.GetComponent<Rigidbody>());

            // set to correct size
            colObject.transform.position = ogPos;
            colObject.transform.localScale = ogSca;
            colObject.transform.rotation = ogRot;

            // Update the connections (to destroy this object)
            //colObject.GetComponent<Connections>().ObjectDestroyed();
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
