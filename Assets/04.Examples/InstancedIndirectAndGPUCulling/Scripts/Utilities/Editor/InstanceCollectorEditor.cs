using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InstancedIndirect
{
    [CustomEditor(typeof(InstanceCollector))]
    public class InstanceCollectorEditor : Editor
    {
        InstanceCollector instanceCollector;

        public override void OnInspectorGUI()
        {
            if (instanceCollector == null)
            {
                instanceCollector = (InstanceCollector)target;
            }


            if (GUILayout.Button("Collect"))
            {
                instanceCollector.Collect();
            }


            base.OnInspectorGUI();
        }
    }
}
