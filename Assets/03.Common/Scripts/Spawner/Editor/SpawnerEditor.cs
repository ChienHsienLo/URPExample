using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Spawner))]
public class SpawnerEditor : Editor
{

    Spawner spawner;


    public override void OnInspectorGUI()
    {
        if(spawner==null)
        {
            spawner = (Spawner)target;
        }



        if(GUILayout.Button("Spawn"))
        {
            spawner.Spawn();
        }
        
        base.OnInspectorGUI();


        int totalCount = Mathf.FloorToInt(spawner.area.x * spawner.area.y * spawner.countPerUnit);
        EditorGUILayout.IntField("Total : ", totalCount);


    }


}
