using JetBrains.Annotations;
using System;
using System.Linq.Expressions;
using UnityEngine;

namespace UnityFracture.Demo
{
    /// <summary>
    /// The class manages the prefractuing of object, this will be done in editor, as such works in complement
    /// with the FractreEditor class that will a button allowing for the fracturing of the object before runtime.
    /// </summary>
    public class PreFracture : MonoBehaviour
    {
        [Header("Pre-Fracture Options")]
        [Range(1, 1024)]
        public int fragmentCount;
        public Material insideMat;
        [Header("Children Options")]
        public string fractureTag;
        public int fractureLayer;
        public int childFragmentCount;
        public Material childInsideMat;
        public bool spawnEffects;
        public GameObject effectToSpawn;

        private GameObject fragmentRoot;

        /// <summary>
        /// Pre-fractures the object it is attached to, setting up
        /// the children to be correct, IE. so they can be used
        /// for fracturing at runtime.
        /// </summary>
        /// <param name="obj">The object to be fractured.</param>
        public void PreFractureThis()
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
            GameObject fragmentTemplate = Fracturer.CreateTemplate(gameObject, insideMat);

            // Call the fracture on this object, sending the object, template, and other information
            // needed to fracture the object
            Fracturer.Fracture(gameObject,
                fragmentTemplate,
                fragmentRoot.transform,
                fragmentCount,
                insideMat);

            // add all of the main scripts to the fragment root to allow for desired functonality
            // DestructionController -> Allow for optimisation of children
            // CalculateConnections -> To clalcualte the connections in the children
            // StressPropogation -> To propogate the stress through the object
            var destController = fragmentRoot.AddComponent<DestructionController>();
            var calcConnections = fragmentRoot.AddComponent<CalculateConnections>();
            var stressProp = fragmentRoot.AddComponent<StressPropogation>();


            // add needed components to the children to ensure desired functonality as well as 
            // setting the desired preliminary values for the fractuing of them
            // Connections -> allow for StressPropagation to function correctly
            // RuntimeFracture -> Allow for fracturing of the objects at runtime
            int childCount = fragmentRoot.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                fragmentRoot.transform.GetChild(i).gameObject.AddComponent<Connections>();
                var fracture = fragmentRoot.transform.GetChild(i).gameObject.AddComponent<RuntimeFracture>();
                fracture.fragmentCount = childFragmentCount;
                fracture.insideMat = childInsideMat;
                fracture.gameObject.tag = fractureTag;
                fracture.gameObject.layer = fractureLayer;
                fracture.spawnEffect = spawnEffects;
                fracture.effect = effectToSpawn;
                fragmentRoot.transform.GetChild(i).gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            }

            // call the base function on the child object, (to set it up correctly)
            destController.OptmizeChildren();
            stressProp.GetAllChildren();
            calcConnections.calculateConnections();

            // Set this object to false (not destroy in case the user wants to change the parameters)
            // then destroy the template created, DestroyImmediate so it will work in editor.
            gameObject.SetActive(false);
            DestroyImmediate(fragmentTemplate);
        }
    }
}