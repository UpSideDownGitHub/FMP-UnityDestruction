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
        public float fireRate;
        private float _timeOfNextFire = 0;

        [Header("Effect Spawning")]
        public bool spawnEffect;
        public GameObject effect;

        public RayCastActivation rayCastActivation;

        /// <summary>
        /// initilse the manager to have this raycast activation (can control fragment counts)
        /// </summary>
        public void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            var manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<MainUIManager>();
            manager.rayCastActivation = rayCastActivation;
            rayCastActivation.fractureCount = manager.fragmentCounts[manager.currentFragCount];
        }

        /// <summary>
        /// detechts when the player tries to shoot
        /// </summary>
        void Update()
        {
            if (Input.GetMouseButtonDown(0) && Time.time > _timeOfNextFire)
            {
                _timeOfNextFire = Time.time + fireRate;
                FireTowardsClick();
            }
        }

        /// <summary>
        /// fires a bullet in the direction of the click
        /// </summary>
        void FireTowardsClick()
        {
            // create a ray in the direction of the click
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 

            GameObject bulletTemp = Instantiate(bulletPrefab, ray.GetPoint(0), Quaternion.identity);
            bulletTemp.GetComponent<Rigidbody>().velocity = ray.direction.normalized * bulletSpeed;

            // Fire a ray to start fracuring the object
            rayCastActivation.FireRay(effect, spawnEffect, ray);

        }
    }
}
