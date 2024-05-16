using JetBrains.Annotations;
using System;
using System.Linq.Expressions;
using UnityEngine;

namespace UnityFracture.Demo
{
    /// <summary>
    /// The class manages the fracturing of object at runtime, as such this will break
    /// down the object and then run the stress propagation.
    /// </summary>
    public class RuntimeFracture : MonoBehaviour
    {
        [Header("Fracture Options")]
        [Range(1, 1024)]
        public int fragmentCount;
        public Material insideMat;
        public bool floatingDetection;
        public bool copyMaterials;

        [Header("Effect Options")]
        public bool spawnEffect;
        public GameObject effect;

        private GameObject fragmentRoot;

        /// <summary>
        /// Spawns the effect if spawnEffect is true
        /// </summary>
        public void SpawnEffect()
        {
            if (spawnEffect)
                Instantiate(effect, transform.position, Quaternion.identity);
        }

        /// <summary>
        /// Fracture the current obect
        /// </summary>
        public void FractureThis(float explosionForce, Vector3 explosionPosition, float explosionRadius)
        {
            // get the current mesh of this game object and if there is not mesh then return 
            // to fracture a mesh you need a mesh.
            var mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            if (mesh == null)
                return;

            // create a new gameobject that will hold all of the fragments
            fragmentRoot = new GameObject($"{gameObject.name}Fragments");
            fragmentRoot.transform.SetParent(gameObject.transform.parent);
            fragmentRoot.transform.position = gameObject.transform.position;
            fragmentRoot.transform.rotation = gameObject.transform.rotation;
            fragmentRoot.transform.localScale = Vector3.one;

            // create the template for the fragments, this will take on all of the components
            // of the current object to make it match (Mesh, Rigidbody, Collider)
            GameObject fragmentTemplate = Fracturer.CreateTemplate(gameObject, insideMat, copyMaterials, false);

            // Call the fracture on this object, sending the object, template, and other information
            // needed to fracture the object
            Fracturer.Fracture(gameObject,
                fragmentTemplate,
                fragmentRoot.transform,
                fragmentCount,
                floatingDetection);

            // remove the connections from this object, as well as telling the stress propagation
            // that the object has been destroyed, this will break any neighbouring objects and make them
            // fall
            try
            {
                gameObject.GetComponent<Connections>().ObjectDestroyed();
                gameObject.GetComponentInParent<StressPropogation>().PropogateStress(gameObject.GetComponent<Connections>());
                gameObject.GetComponentInParent<StressPropogation>().PartDestroyed();
            }
            catch
            {
                // There are No Connections/Stress Propagation
            }
            // set the destroy times on all of the child objects, so the fragments dont stay
            // around forever
            int childCount = fragmentRoot.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                fragmentRoot.transform.GetChild(i).gameObject.AddComponent<FadeDestroy>();
                fragmentRoot.transform.GetChild(i).gameObject.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, explosionPosition, explosionRadius);
            }

            // destroy the fragment template, as well as the current gameobject.
            Destroy(fragmentTemplate);
            Destroy(gameObject);
        }
    }
}