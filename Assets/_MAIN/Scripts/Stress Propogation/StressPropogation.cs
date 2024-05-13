using System;
using System.Collections.Generic;
using UnityEngine;
using UnityFracture.Demo;

namespace UnityFracture
{
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
        
        public void PartDestroyed()
        {
            if (children.Count == 0)
                GetAllChildren();

            // reset checking
            foreach (Connections connection in children)
            {
                connection.needsCheck = true;
            }
            // find the clusters
            print($"Children: {children.Count}");
            foreach (Connections connection in children)
            {
                if (connection.needsCheck && !connection.destroyed)
                {
                    print("Checking Peice For Cluster");
                    var cluster = FindCluster(connection);
                    clusterList.Add(new ClusterList(cluster));
                    bool needsBreak = true;
                    foreach (Connections peice in cluster)
                    {
                        if (peice.rootObject)
                        {
                            needsBreak = false;
                            break;
                        }
                    }
                    if (needsBreak)
                    {
                        // break the objects in this cluster
                        foreach(Connections peice in cluster)
                        {
                            peice.gameObject.GetComponent<FractureObject>().FractureThis();
                            //peice.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                        }
                    }
                }
            }
        }

        public List<Connections> FindCluster(Connections con)
        {
            List<int> ids = new List<int>();
            List<Connections> connections = new();
            con.needsCheck = false;
            ids.Add(con.gameObject.GetInstanceID());
            Queue<Connections> queue = new Queue<Connections>();
            queue.Enqueue(con);
            int j = 0;
            int iterMax = 2000;
            while (queue.Count > 0)
            {
                var peice = queue.Dequeue();
                peice.needsCheck = false;
                ids.Add(peice.gameObject.GetInstanceID());
                connections.Add(peice);
                for (int i = 0; i < peice.connections.Count; i++)
                {
                    if (!ids.Contains(peice.connections[i].gameObject.GetInstanceID()))
                    {
                        queue.Enqueue(peice.connections[i]);
                    }
                }
                j++;
                if (j > iterMax)
                    break;
            }
            return connections;
        }
    }
}
