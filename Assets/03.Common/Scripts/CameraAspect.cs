using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAspect : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] float aspectRatio = 1.0f;

    void Start()
    {
        SetAspect(cam);
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        SetAspect(cam);
    }
#endif

    void SetAspect(Camera c)
    {
        if (!c)
        {
            return;
        }

        if (c.orthographic == true)
        {
            c.aspect = aspectRatio;
        }
    }

}
