using UnityEditor;
using UnityEngine;

namespace UnityFracture
{
    [CustomEditor(typeof(CalculateConnections))]
    public class ConnectionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            CalculateConnections connections = (CalculateConnections)target;
            base.OnInspectorGUI();
            if (GUILayout.Button("Find Connections"))
                connections.calculateConnections();

        }
    }
}
