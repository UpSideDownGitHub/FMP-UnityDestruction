using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace UnityFracture.Demo
{
    /// <summary>
    /// the two shooting options for the player
    /// </summary>
    public enum PlayerFireOption
    {
        BULLETAREA,
        RAYCAST
    }

    /// <summary>
    /// player will allow for movement, as well as shooting, using 2 moves, raycast and area with bullets
    /// </summary>
    public class Player : MonoBehaviour
    {
        // https://blog.unity.com/engine-platform/free-vfx-image-sequences-flipbooks (Particle Effect Source)
        [Header("Movement")]
        public Rigidbody rb;
        public float movementForce;
        public float maxVelocity;
        
        [Header("Jumping")]
        public float jumpForce;
        public float groundDistance;
        public string groundTag;
        public bool grounded = true;
        
        [Header("Camera")]
        public GameObject cam;

        [Header("Shooting")]
        public float fireRate;
        public PlayerFireOption fireOption;
        private float _timeOfNextFire;

        [Header("Bullet Options")]
        public GameObject bullet;
        public Transform firePoint;
        public float bulletFireForce;

        [Header("RayCast Options")]
        public RayCastActivation rayCastActivation;

        /// <summary>
        /// called before the first update and will lock the cursor the the screen
        /// to allow for better aiming
        /// </summary>
        public void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        /// <summary>
        /// Called when [collision enter].
        /// </summary>
        /// <param name="collision">The collision object.</param>
        public void OnCollisionEnter(Collision collision)
        {
            // check for enter ground
            if (collision.gameObject.CompareTag(groundTag))
                grounded = true;
        }
        /// <summary>
        /// Called when [collision exit].
        /// </summary>
        /// <param name="collision">The collision object</param>
        public void OnCollisionExit(Collision collision)
        {
            // check for left ground
            if (collision.gameObject.CompareTag(groundTag))
                grounded = false;
        }

        /// <summary>
        /// Called once perframe and will apply the movement, rotation, and shooting for the player.
        /// </summary>
        void Update()
        {
            // get input
            var movementHor = Input.GetAxis("Horizontal");
            var movementDep = Input.GetAxis("Vertical");
            bool jump = Input.GetKeyDown(KeyCode.Space);

            // apply forces
            rb.AddRelativeForce(new Vector3(movementHor, 0, movementDep) * movementForce, ForceMode.VelocityChange);
            rb.AddForce(new Vector3(0, jump && grounded ? 1 : 0, 0) * jumpForce, ForceMode.Impulse);

            // clamp movementSpeed
            rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -maxVelocity, maxVelocity), rb.velocity.y, Mathf.Clamp(rb.velocity.z, -maxVelocity, maxVelocity));

            // Set Rotation
            transform.rotation = Quaternion.Euler(0, cam.transform.rotation.eulerAngles.y, 0);

            // shooting
            if (Time.time > _timeOfNextFire && Input.GetMouseButtonDown(0))
            {
                // shoot the bullet using one of 2 options available
                // BULLETAREA -> fires a physical bullet that uses AreaActivation
                // RAYCAST -> uses RayCastActivation
                switch(fireOption)
                {
                    case PlayerFireOption.BULLETAREA:
                        GameObject bulletTemp = Instantiate(bullet, firePoint.transform.position, firePoint.transform.rotation);
                        bullet.GetComponent<Rigidbody>().AddForce(bulletTemp.transform.forward * bulletFireForce);
                        break;
                    case PlayerFireOption.RAYCAST:
                        rayCastActivation.FireRay();
                        break;
                    default:
                        Debug.Log("Error: Player Fire Option Failed");
                        break;
                }
            }
        }
    }
}