using JetBrains.Annotations;
using System;
using System.Linq.Expressions;
using UnityEngine;

namespace ReubenMiller.Fracture
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

        public bool asyncFracture;
        public bool asyncFloating;
        public bool asyncStress;

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
        public void FractureThis(float explosionForce, Vector3 explosionPosition, float explosionRadius, int fragCount = 0)
        {
            if (fragCount != 0)
                fragmentCount = fragCount;

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


            if (asyncFracture)
            {
                // fracture the object asyncontrosly
                StartCoroutine(Fracturer.FractureAsync(gameObject,
                    fragmentTemplate,
                    fragmentRoot.transform,
                    fragmentCount,
                    floatingDetection,
                    () =>
                    {
                        PropogateStress(explosionForce, explosionPosition, explosionRadius, fragmentTemplate);
                    }
                ));
            }
            else
            {
                // Call the fracture on this object, sending the object, template, and other information
                // needed to fracture the object
                Fracturer.Fracture(gameObject,
                    fragmentTemplate,
                    fragmentRoot.transform,
                    fragmentCount,
                    floatingDetection);

                PropogateStress(explosionForce, explosionPosition, explosionRadius, fragmentTemplate);
            }
        }

        /// <summary>
        /// remove the connections from this object, as well as telling the stress propagation
        /// that the object has been destroyed, this will break any neighbouring objects and make them
        /// fall, and make object with to much force fall
        /// 
        /// This function looks complicated but that is just so it can keep the correct order even with the async, that being
        /// -> PropogateStress
        /// -> PartDestroyed
        /// -> End
        /// to ensure the order is correct, Action callbacks are used within the async functions to make them
        /// call the correct thing when they finish executing.
        /// </summary>
        /// <param name="explosionForce">The explosion force.</param>
        /// <param name="explosionPosition">The explosion position.</param>
        /// <param name="explosionRadius">The explosion radius.</param>
        /// <param name="fragmentTemplate">The fragment template.</param>
        public void PropogateStress(float explosionForce, Vector3 explosionPosition, float explosionRadius, GameObject fragmentTemplate)
        {
            try
            {
                gameObject.GetComponent<Connections>().ObjectDestroyed();
                if (asyncStress)
                {
                    StartCoroutine(gameObject.GetComponentInParent<StressPropogation>().PropogateStressAsync(gameObject.GetComponent<Connections>(),
                        () =>
                        {
                            if (asyncFloating)
                            {
                                StartCoroutine(gameObject.GetComponentInParent<StressPropogation>().PartDestroyedAsync(() =>
                                {
                                    EndFracture(explosionForce, explosionPosition, explosionRadius, fragmentTemplate);
                                }));
                            }
                            else
                            {
                                gameObject.GetComponentInParent<StressPropogation>().PartDestroyed();
                                EndFracture(explosionForce, explosionPosition, explosionRadius, fragmentTemplate);
                            }


                        }
                    ));
                }
                else
                {
                    gameObject.GetComponentInParent<StressPropogation>().PropogateStress(gameObject.GetComponent<Connections>());
                    if (asyncFloating)
                    {
                        StartCoroutine(gameObject.GetComponentInParent<StressPropogation>().PartDestroyedAsync(() =>
                        {
                            EndFracture(explosionForce, explosionPosition, explosionRadius, fragmentTemplate);
                        }));
                    }
                    else
                    {
                        gameObject.GetComponentInParent<StressPropogation>().PartDestroyed();
                        EndFracture(explosionForce, explosionPosition, explosionRadius, fragmentTemplate);
                    }
                }
            }
            catch (Exception e) { Debug.LogException(e); }
        }

        /// <summary>
        /// Ends the fracture by setting the children and destroying the object
        /// </summary>
        /// <param name="explosionForce">The explosion force.</param>
        /// <param name="explosionPosition">The explosion position.</param>
        /// <param name="explosionRadius">The explosion radius.</param>
        /// <param name="fragmentTemplate">The fragment template.</param>
        public void EndFracture(float explosionForce, Vector3 explosionPosition, float explosionRadius, GameObject fragmentTemplate)
        {
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