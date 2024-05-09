using System.Collections.Generic;
using UnityEngine;
using UnityFracture.Demo;

namespace UnityFracture
{
    public class Connections : MonoBehaviour
    {
        public List<Connections> connections = new();

        public ShearCalculator shearCalculator = new();

        public float connectionForce = 100f; // the force of a connection

        public void ObjectDestroyed()
        {
            // remove all connections to this object
            for (int i = 0; i < connections.Count; i++)
            {
                connections[i].RemoveConnection(this);
                connections[i].CalculateForces();
            }
            // destroy this
            Destroy(this);
        }

        public void CalculateForces()
        {
            Vector3 totalForce = Vector3.zero;
            for (int i = 0; i < connections.Count; i++)
            {
                // calculate the force of this current object being applied
                var nDir = (transform.position - connections[i].gameObject.transform.position).normalized;
                totalForce += nDir * connectionForce;
            }
            shearCalculator.UpdateForces(totalForce);
            if (shearCalculator.CalculateShear())
            {
                // shear the object
                GetComponent<FractureObject>().FractureThis();
            }
        }

        public void AddConnection(Connections connection)
        {
            connections.Add(connection);
        }
        public void RemoveConnection(Connections connection)
        {
            connections.Remove(connection);
        }
        public bool HasConnection(Connections connection)
        {
            return connections.Contains(connection);
        }
    }
}