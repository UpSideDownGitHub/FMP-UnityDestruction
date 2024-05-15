using System.Collections.Generic;
using UnityEngine;
using UnityFracture.Demo;

namespace UnityFracture
{
    /// <summary>
    /// class to hold the connections of each of the children objects.
    /// </summary>
    public class Connections : MonoBehaviour
    {
        // the list of connections of the current object
        public List<Connections> connections = new();
        
        public bool rootObject;
        public bool needsCheck = true;
        public bool destroyed = false;

        /// <summary>
        /// Objects the destroyed.
        /// </summary>
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

        /// <summary>
        /// Adds the connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public void AddConnection(Connections connection)
        {
            connections.Add(connection);
        }
        /// <summary>
        /// Removes the connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public void RemoveConnection(Connections connection)
        {
            connections.Remove(connection);
        }
        /// <summary>
        /// Removes all connections.
        /// </summary>
        public void RemoveAllConnections()
        {
            connections.Clear();
        }
        /// <summary>
        /// Determines whether the specified connection is connected to this object.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>
        ///   <c>true</c> if the specified connection has connection; otherwise, <c>false</c>.
        /// </returns>
        public bool HasConnection(Connections connection)
        {
            return connections.Contains(connection);
        }
    }
}