using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ReubenMiller.Fracture.Demo
{
    public class God : MonoBehaviour
    {
        [Header("Bullet")]
        public GameObject bulletPrefab; 
        public float bulletSpeed = 10.0f;

        [Header("Effect Spawning")]
        public bool spawnEffect;
        public GameObject effect;

        /// <summary>
        /// detechts when the player tries to shoot
        /// </summary>
        void Update()
        {
            if (Input.GetMouseButtonDown(0)) 
                FireTowardsClick();
        }

        /// <summary>
        /// fires a bullet in the direction of the click
        /// </summary>
        void FireTowardsClick()
        {
            // create a ray in the direction of the click
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 

            // Check for collision with world objects
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // Spawn Particle
            }

            // Spawn the bullet at camera position
            GameObject bulletTemp = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

            // Move the bullet in the rays direction & set effect
            bulletTemp.GetComponent<Rigidbody>().velocity = (ray.direction).normalized * bulletSpeed;
            bulletTemp.GetComponent<AreaActivation>().SetEffect(spawnEffect, effect);

        }
    }
}
