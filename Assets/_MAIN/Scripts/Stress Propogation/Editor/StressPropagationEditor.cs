using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace UnityFracture
{
    /// <summary>
    /// this class adds custom inspector with get children button
    /// </summary>
    [CustomEditor(typeof(StressPropogation))]
    public class StressPropagationEditor : Editor
    {
        /// <summary>
        /// Implement this function to make a custom inspector with get children button
        /// </summary>
        public override void OnInspectorGUI()
        {
            // get base script
            StressPropogation stressProp = (StressPropogation)target;
            // show base UI
            base.OnInspectorGUI();
            // add button
            if (GUILayout.Button("Get Children"))
                stressProp.GetAllChildren();

        }
    }
}
