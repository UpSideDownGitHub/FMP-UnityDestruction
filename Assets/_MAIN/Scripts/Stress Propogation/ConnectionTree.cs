using System.Collections.Generic;
using UnityEngine;

namespace UnityFracture
{
    public class ConnectionTree : MonoBehaviour
    {
        public List<TreeNode> rootNodes = new();
        public void NodeDestroyed(Connections connection)
        {
            TreeNode targetNode = null;
            foreach (TreeNode root in rootNodes)
            {
                targetNode = root.FindTreeNode(node => node.connections == connection);
                if (targetNode != null)
                    break;
            }
            
            // loop from the found node and calculate the connections / breaks for the rest of the nodes connected
            foreach(TreeNode root in targetNode)
            {
                // should pritn all of the children nodes to this one 
                if (root != null)
                    root.connections.gameObject.SetActive(false);

            }
        }

        public void CreateTreeFromChildren()
        {
            // Find root nodes
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var connections = transform.GetChild(i).GetComponent<Connections>();
                if (connections.rootObject)
                {
                    var node = new TreeNode(connections);
                    rootNodes.Add(node);
                }
            }

            // generate all of the connections for the tree
            foreach (TreeNode root in rootNodes)
            {
                Queue<TreeNode> queue = new Queue<TreeNode>();
                queue.Enqueue(root);
                while(queue.Count > 0)
                {
                    TreeNode currenNode = queue.Dequeue();
                    for (int i = 0; i < currenNode.connections.connections.Count; i++)
                    {
                        if (currenNode.connections.connections[i] != currenNode.connections)
                        {
                            TreeNode childNode = new TreeNode(currenNode.connections.connections[i]);
                            currenNode.AddChild(childNode);
                            childNode.AddParent(currenNode);
                            queue.Enqueue(childNode);
                        }
                    }
                }
            }
        }
    }
}