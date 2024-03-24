using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class FractureObject : MonoBehaviour
{
    [Header("Fracture Options")]
    [Range(1, 1024)]
    public int fragmentCount;
    public Material insideMat;

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
                FractureThis();
            }
        }
    }

    public void FractureThis()
    {
        var mesh = GetComponent<MeshFilter>().sharedMesh;
        if (mesh != null)
        {
            fragmentRoot = new GameObject($"{name}Fragments");
            fragmentRoot.transform.SetParent(transform.parent);

            fragmentRoot.transform.position = transform.position;
            fragmentRoot.transform.rotation = transform.rotation;
            fragmentRoot.transform.localScale = Vector3.one;

            GameObject fragmentTemplate = CreateTemplate();

            Fracturer.Fracture(gameObject, 
                fragmentTemplate, 
                fragmentRoot.transform,
                fragmentCount, 
                insideMat);

            Destroy(fragmentTemplate);
            gameObject.SetActive(false);
        }
    }

    public GameObject CreateTemplate()
    {
        GameObject obj = new("Fragment"){ tag = tag };
        obj.AddComponent<MeshFilter>();

        var meshRenderer = obj.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterials = new Material[2] {
            GetComponent<MeshRenderer>().sharedMaterial,
            insideMat
        };

        var thisCollider = GetComponent<Collider>();
        var fragmentCollider = obj.AddComponent<MeshCollider>();
        fragmentCollider.convex = true;
        fragmentCollider.sharedMaterial = thisCollider.sharedMaterial;
        fragmentCollider.isTrigger = thisCollider.isTrigger;

        var thisRigidBody = this.GetComponent<Rigidbody>();
        var fragmentRigidBody = obj.AddComponent<Rigidbody>();
        fragmentRigidBody.velocity = thisRigidBody.velocity;
        fragmentRigidBody.angularVelocity = thisRigidBody.angularVelocity;
        fragmentRigidBody.drag = thisRigidBody.drag;
        fragmentRigidBody.angularDrag = thisRigidBody.angularDrag;
        fragmentRigidBody.useGravity = thisRigidBody.useGravity;
        
        return obj;
    }
}