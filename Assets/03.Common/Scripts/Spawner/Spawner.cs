using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject subject;
    public Transform root;
    public int countPerUnit;
    public Vector2 area;
    public Vector3 scaleOffsetRange;

    public void Spawn()
    {
        int totalCount = Mathf.FloorToInt(area.x * area.y * countPerUnit);
        
        for(int i =0; i< totalCount;i++)
        {
            GameObject sub = GameObject.Instantiate(subject);
            Transform t = sub.transform;

            float x = Random.Range(0.0f, area.x);
            float z = Random.Range(0.0f, area.y);
            Vector3 pos = new Vector3(x, 0, z);

            Vector3 scale = new Vector3(
                Random.Range(1 - scaleOffsetRange.x, 1 + scaleOffsetRange.x),
                Random.Range(1 - scaleOffsetRange.y, 1 + scaleOffsetRange.y),
                Random.Range(1 - scaleOffsetRange.z, 1 + scaleOffsetRange.z)
                );

            
            Vector3 euler = t.localEulerAngles;
            t.localEulerAngles = new Vector3(
                euler.x,
                Random.Range(0, 360),
                euler.z
                );
            
            t.localScale = scale;
            t.SetParent(root);
            t.position = pos;
        }
    }
 
}

