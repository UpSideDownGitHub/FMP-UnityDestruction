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
        public bool ignoreCollision;

        [Header("Trigger Options")]
        public string triggerTag;
        public float minCollisionForce;

        private GameObject fragmentRoot;

        public void OnCollisionEnter(Collision collision)
        {
            if (ignoreCollision)
                return;
            if (collision.contactCount > 0)
            {
                if (!fractureCollision)
                    FractureThis(gameObject);
                else
                    FractureThis(collision.gameObject);

                if (destroyPost)
                    Destroy(gameObject);
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

                if (Application.isEditor && !Application.isPlaying)
                {
                    fragmentRoot.AddComponent<DestructionController>();
                    fragmentRoot.AddComponent<CalculateConnections>();
                }
                else
                {
                    obj.GetComponent<Connections>().ObjectDestroyed();
                    obj.GetComponentInParent<StressPropogation>().PartDestroyed();
                    Destroy(obj);
                }

                // set the destroy times on all of the child objects
                int childCount = fragmentRoot.transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    if (Application.isEditor && !Application.isPlaying)
                    { 
                        fragmentRoot.transform.GetChild(i).gameObject.AddComponent<Connections>();
                        var fracture = fragmentRoot.transform.GetChild(i).gameObject.AddComponent<FractureObject>();
                        fracture.fragmentCount = this.fragmentCount;
                        fracture.insideMat = this.insideMat;
                        fracture.ignoreCollision = true;
                    }
                    else
                        fragmentRoot.transform.GetChild(i).gameObject.AddComponent<FadeDestroy>();
                }

                obj.SetActive(false);
                if (Application.isEditor && !Application.isPlaying)
                    DestroyImmediate(fragmentTemplate);
                else
                    Destroy(fragmentTemplate);
            }
        }
    }
}