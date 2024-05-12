using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace UnityFracture
{
    [CustomEditor(typeof(ConnectionTree))]
    public class ConnectionTreeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ConnectionTree connections = (ConnectionTree)target;
            base.OnInspectorGUI();
            if (GUILayout.Button("Find Connections"))
                connections.CreateTreeFromChildren();
        }
    }
}
