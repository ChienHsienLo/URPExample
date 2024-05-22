using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InstancedIndirect
{

    [CustomEditor(typeof(TerrainTreeCollector))]
    public class TreeInstanceCollectorEditor : Editor
    {
        TerrainTreeCollector instanceCollector;

        public override void OnInspectorGUI()
        {
            if (instanceCollector == null)
            {
                instanceCollector = (TerrainTreeCollector)target;
            }


            if (GUILayout.Button("Collect"))
            {
                instanceCollector.Collect();
            }


            base.OnInspectorGUI();
        }
    }
}