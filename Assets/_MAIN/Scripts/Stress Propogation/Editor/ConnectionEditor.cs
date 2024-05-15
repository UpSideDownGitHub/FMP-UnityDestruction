using UnityEditor;
using UnityEngine;

namespace UnityFracture
{
    /// <summary>
    /// class to make a custom inspector with Find Connections Button
    /// </summary>
    [CustomEditor(typeof(CalculateConnections))]
    public class ConnectionEditor : Editor
    {
        /// <summary>
        /// Implement this function to make a custom inspector with Find Connections Button
        /// </summary>
        public override void OnInspectorGUI()
        {
            // base script
            CalculateConnections connections = (CalculateConnections)target;
            // base UI
            base.OnInspectorGUI();
            // add button to allow for finding connections in editor
            if (GUILayout.Button("Find Connections"))
                connections.calculateConnections();

        }
    }
}
