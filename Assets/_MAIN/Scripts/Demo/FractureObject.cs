using JetBrains.Annotations;
using System;
using UnityEngine;

namespace UnityFracture.Demo
{
    public class FractureObject : MonoBehaviour
    {
        [Header("Fracture Options")]
        [Range(1, 1024)]
        public int fragmentCount;
        public Material insideMat;
        public bool fractureCollision;
        public bool destroyPost;

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
                    if (!fractureCollision)
                        FractureThis(gameObject);
                    else
                        FractureThis(collision.gameObject);

                    if (destroyPost)
                        Destroy(gameObject);
                }
            }
        }

        public void FractureThis()
        {
            FractureThis(gameObject);
        }
        public void FractureThis(GameObject obj)
        {
            var mesh = obj.GetComponent<MeshFilter>().sharedMesh;
            if (mesh != null)
            {
                fragmentRoot = new GameObject($"{obj.name}Fragments");
                fragmentRoot.transform.SetParent(obj.transform.parent);

                fragmentRoot.transform.position = obj.transform.position;
                fragmentRoot.transform.rotation = obj.transform.rotation;
                fragmentRoot.transform.localScale = Vector3.one;

                GameObject fragmentTemplate = Fracturer.CreateTemplate(obj, insideMat);

                Fracturer.Fracture(obj,
                    fragmentTemplate,
                    fragmentRoot.transform,
                    fragmentCount,
                    insideMat);

                fragmentRoot.AddComponent<DestructionController>();

                // set the destroy times on all of the child objects
                int childCount = fragmentRoot.transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    fragmentRoot.transform.GetChild(i).gameObject.AddComponent<FadeDestroy>();
                }

                obj.SetActive(false);
                Destroy(fragmentTemplate);
            }
        }
    }
}