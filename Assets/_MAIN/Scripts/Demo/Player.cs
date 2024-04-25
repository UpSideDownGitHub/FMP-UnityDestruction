using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace UnityFracture.Demo
{
    public class Player : MonoBehaviour
    {
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
        public float fireDistance;
        public Transform firePoint;
        public GameObject CollisionObject;
        private float _timeOfNextFire;
        public GameObject destructionObject;


        public void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void OnCollisionEnter(Collision collision)
        {
            // check for enter ground
            if (collision.gameObject.CompareTag(groundTag))
                grounded = true;
        }
        public void OnCollisionExit(Collision collision)
        {
            // check for left ground
            if (collision.gameObject.CompareTag(groundTag))
                grounded = false;
        }

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
                // shoot ray in the distance of the house
                RaycastHit hit;
                Physics.Raycast(firePoint.transform.position, firePoint.transform.forward, out hit, fireDistance);
                if (hit.collider != null)
                {
                    Instantiate(destructionObject, hit.point, Quaternion.identity, hit.collider.transform);
                    
                }
            }
        }
    }
}