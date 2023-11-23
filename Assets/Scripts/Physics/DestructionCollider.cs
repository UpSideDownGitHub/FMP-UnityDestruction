using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructionCollider : MonoBehaviour
{
    public GameObject object1;
    public Mesh object1Mesh;
    public GameObject object2;
    public Mesh object2Mesh;

    public void Update()
    {
        if (Vector3.Distance(object1.transform.position, object2.transform.position) <
            FindBoundingSphereRadius(object1Mesh.vertices, object1Mesh.bounds.center) +
            FindBoundingSphereRadius(object2Mesh.vertices, object2Mesh.bounds.center))
        {
            print("Object Are Colliding");
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(object1.transform.position, FindBoundingSphereRadius(object1Mesh.vertices, object1Mesh.bounds.center));
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(object2.transform.position, FindBoundingSphereRadius(object2Mesh.vertices, object2Mesh.bounds.center));
    }

    public float FindBoundingSphereRadius(Vector3[] points, Vector3 center)
    {
        if (points.Length == 0)
            return -1;

        float curDist, maxDist = 0;
        for (int i = 0; i < points.Length; i++)
        {
            curDist = Mathf.Pow(points[i].y - center.x, 2) +
                Mathf.Pow(points[i].y - center.y, 2) +
                Mathf.Pow(points[i].z - center.z, 2);

            if (curDist > maxDist)
                maxDist = curDist;
        }
        return Mathf.Sqrt(maxDist);
    }
}
