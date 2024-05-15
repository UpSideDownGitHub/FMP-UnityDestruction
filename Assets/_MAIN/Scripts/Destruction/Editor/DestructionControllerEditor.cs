using UnityEditor;
using UnityEngine;

namespace UnityFracture
{
    /// <summary>
    /// adds a "Optimize Children" button to the Destruction controller inspector 
    /// to allow for optimizing of the meshes of the children once cut
    /// </summary>
    [CustomEditor(typeof(DestructionController))]
    public class DestructionControllerEditor : Editor
    {
        /// <summary>
        /// Overrieds "OnInspectorGUI" to allow for button to optimize children
        /// </summary>
        public override void OnInspectorGUI()
        {
            // get the base component (the script to reaplce the inspector UI)
            DestructionController controller = (DestructionController)target;
            // make the base UI show
            base.OnInspectorGUI();
            // add the optimize children button to optimize the children
            if (GUILayout.Button("Optmize Children"))
                controller.OptmizeChildren();
        }
    }
}
