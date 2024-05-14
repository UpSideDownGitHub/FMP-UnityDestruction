using UnityEngine;
using UnityEditor;

namespace UnityFracture.Demo
{
    /// <summary>
    /// The class will add the button the PreFracture inspector window, that will allow for the 
    /// fracturing of the objects.
    /// </summary>
    [CustomEditor(typeof(PreFracture))]
    public class FractureEditor : Editor
    {
        /// <summary>
        /// overrides the "OnInspectorGUI" function to make a custom inspector adding the 
        /// Pre-Fracture button.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // get the target component (the PreFracture script)
            PreFracture component = (PreFracture)target;
            // show the base UI
            base.OnInspectorGUI();
            // add the button to the inspector, and when it is pressed, call the prefracture function
            if(GUILayout.Button("Pre-Fracture"))
            {
                component.PreFractureThis();
            }
        }
    }
}
