using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ReubenMiller.Fracture.Demo;

namespace ReubenMiller.Fracture
{
    /// <summary>
    /// This is a demo class for how to implment RayCast activation for destructibles
    /// it works by sending a ray from the center of the screen, with the destructible layermask
    /// meaning that it will only collide with destructibles, then it will call "FractureThis"
    /// to destroy the object it collided with.
    /// </summary>
    public class RayCastActivation : MonoBehaviour
    {
        public LayerMask destructibleLayer;

        public int fractureCount = 0;

        [Header("Ray Details")]
        public float maxDistance;
        public Camera cam;

        [Header("Explosion Force")]
        public float explosionForce;
        public float explosionRadius;

        /// <summary>
        /// if cam has not been assigned will get the main camera in the scene
        /// </summary>
        public void Start()
        {
            if (cam == null)
                cam = Camera.main;
        }

        /// <summary>
        /// Fires the ray from the ceter for the screen in the destructible layer
        /// </summary>
        public void FireRay(GameObject effect, bool spawnEffect)
        {
            RaycastHit hit;
            // create a ray that points in from the center of the screen
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            // if the raycast hits and object then destroy it with "FractureThis"
            if (Physics.Raycast(ray, out hit, maxDistance, destructibleLayer))
            { 
                hit.collider.gameObject.GetComponent<RuntimeFracture>().FractureThis(explosionForce, hit.point, explosionRadius, fractureCount);
                if (spawnEffect)
                    Instantiate(effect, hit.point, Quaternion.identity);
            }
        }
    }
}
