using System;
using System.Collections.Generic;
using UnityEngine;
using UnityFracture.Demo;

namespace UnityFracture
{
    /// <summary>
    /// class to calcualte the stress propagating through the object
    /// calcualtes peices left floating
    /// TODO:
    ///     find peices that are under to much stress and breaks them
    /// </summary>
    public class StressPropogation : MonoBehaviour
    {
        public List<Connections> children = new();
        [Serializable]
        public struct ClusterList
        {
            public ClusterList(List<Connections> connections)
            {
                clusters = connections;
            }
            public List<Connections> clusters;
        }
        public List<ClusterList> clusterList = new();

        /// <summary>
        /// Gets all children.
        /// </summary>
        public void GetAllChildren()
        {
            children.Clear();
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                if (transform.GetChild(i).GetComponent<Connections>())
                    children.Add(transform.GetChild(i).GetComponent<Connections>());
            } 
        }

        /// <summary>
        /// Parts the destroyed.
        /// </summary>
        public void PartDestroyed()
        {
            // if there are no children then get the children
            if (children.Count == 0)
                GetAllChildren();

            // reset checking
            foreach (Connections connection in children)
            {
                connection.needsCheck = true;
            }
            // find all of the clusters of objects
            foreach (Connections connection in children)
            {
                // if this object has not been checked
                if (connection.needsCheck && !connection.destroyed)
                {
                    // find the cluster on this object
                    var cluster = FindCluster(connection);
                    bool needsBreak = true;
                    // if this cluster has a root object then set it to not break
                    foreach (Connections peice in cluster)
                    {
                        if (peice.rootObject)
                        {
                            needsBreak = false;
                            break;
                        }
                    }
                    // if the cluster does not have a root object then break the cluster
                    if (needsBreak)
                    {
                        // break the objects in this cluster
                        foreach(Connections peice in cluster)
                        {
                            //peice.gameObject.GetComponent<FractureObject>().FractureThis();
                            peice.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                            // Add the fade destroy object
                            peice.gameObject.AddComponent<FadeDestroy>().destroyTime = 10;
                            // spawn small smoke effect
                            peice.GetComponent<RuntimeFracture>().SpawnEffect();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds the cluster.
        /// </summary>
        /// <param name="con">The current connection.</param>
        /// <returns>List of connections forming a cluster</returns>
        public List<Connections> FindCluster(Connections con)
        {
            // initilise a queue to hold breadth first search through the graph of connections
            List<int> ids = new List<int>();
            List<Connections> connections = new();
            con.needsCheck = false;
            ids.Add(con.gameObject.GetInstanceID());
            Queue<Connections> queue = new Queue<Connections>();
            queue.Enqueue(con);
            int j = 0;
            int iterMax = 2000;
            // breadth first search to find all connections in this cluster
            while (queue.Count > 0)
            {
                var peice = queue.Dequeue();
                peice.needsCheck = false;
                ids.Add(peice.gameObject.GetInstanceID());
                connections.Add(peice);
                for (int i = 0; i < peice.connections.Count; i++)
                {
                    if (!ids.Contains(peice.connections[i].gameObject.GetInstanceID()))
                        queue.Enqueue(peice.connections[i]);
                }
                j++;
                if (j > iterMax)
                    break;
            }
            return connections;
        }
    }
}
