using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace UnityFracture
{
    [CustomEditor(typeof(StressPropogation))]
    public class StressPropagationEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            StressPropogation stressProp = (StressPropogation)target;
            base.OnInspectorGUI();
            if (GUILayout.Button("Get Children"))
                stressProp.GetAllChildren();

        }
    }
}
