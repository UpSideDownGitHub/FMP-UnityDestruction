using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace UnityFracture
{
    [CustomEditor(typeof(DestructionController))]
    public class DestructionControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DestructionController controller = (DestructionController)target;
            base.OnInspectorGUI();
            if (GUILayout.Button("Optmize Children"))
                controller.OptmizeChildren();

        }
    }
}
