using System.Collections.Generic;
using UnityEngine;
using UnityFracture.Demo;

namespace UnityFracture
{
    public class Connections : MonoBehaviour
    {
        /*
         * NEED TO CHANGE THE CALUCLATE FORCEC TO WORK BASED ON A TREE THAT IT WILL LOOP THROUGH, 
         * SO THE PARENT OBJECT WILL NEED A "ConnectionTree" CLASS THAT WILL HOLD THE CONNECTIONS
         * THEN THAT WILL BE CALLED TO DESTROY ALL OF THE CHILDREN, THIS SHOULD ALLOW FOR MORE 
         * DYNAMIC DESTRUCTION AS WELL AS BETTER PROPOGATION OF THE STRESS
         * 
         * STEPS:
         * - OBJECT DESTROYED
         * - FROM THAT POSITON IN TREE CHECK CHILDREN IF THIS IS THEIR SOLE PARENT THEN DESTROY
         *   IF NOT THE SOLE PARENT THEN UPDATE THE STRESS BEING APPLIED TO THAT OBJECT, THEN CHECK FOR DESTROY
         * - CONTINUE THIS FOR THE REST OF THE CHILDREN, MAKING SURE TO CALCUALTE THE STRESSES FOR ANY CHILDREN
         *   UNDER THE DESTROYED OBJECT.
         * 
        */
        public List<Connections> connections = new();
        
        public bool rootObject;
        public bool needsCheck = true;
        public bool destroyed = false;

        //public ShearCalculator shearCalculator = new();
        //public float connectionForce = 100f; // the force of a connection
        public void ObjectDestroyed()
        {
            // remove all connections to this object
            for (int i = 0; i < connections.Count; i++)
            {
                connections[i].RemoveConnection(this);
            }
            destroyed = true;
            connections.Clear();
        }
        //public void CalculateForces()
        //{
        //    Vector3 totalForce = Vector3.zero;
        //    for (int i = 0; i < connections.Count; i++)
        //    {
        //        // calculate the force of this current object being applied
        //        var nDir = (transform.position - connections[i].gameObject.transform.position).normalized;
        //        totalForce += nDir * connectionForce;
        //    }
        //    shearCalculator.UpdateForces(totalForce);
        //    if (shearCalculator.CalculateShear())
        //    {
        //        // shear the object
        //        GetComponent<FractureObject>().FractureThis();
        //    }
        //}

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