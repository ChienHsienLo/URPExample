using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPosition : MonoBehaviour
{
    public float radius = 1.0f;
    public float heightOffset = 1.0f;
    int playerPositionID = Shader.PropertyToID("_PlayerPosition");
    int heightOffsetID = Shader.PropertyToID("_PlayerPositionHeightOffset");

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;

        Vector4 position = new Vector4(pos.x, pos.y, pos.z, radius);

        Shader.SetGlobalVector(playerPositionID, position);
        Shader.SetGlobalFloat(heightOffsetID, heightOffset);
    }
}
