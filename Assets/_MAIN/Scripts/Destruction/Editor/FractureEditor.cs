using UnityEngine;
using UnityEditor;

namespace UnityFracture.Demo
{ 
    [CustomEditor(typeof(FractureObject))]
    public class FractureEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            FractureObject component = (FractureObject)target;
            base.OnInspectorGUI();

            if(GUILayout.Button("Fracture This"))
            {
                component.FractureThis();
            }
        }
    }
}
