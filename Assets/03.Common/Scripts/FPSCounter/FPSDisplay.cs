using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    [SerializeField] FPSCounter counter;

    [SerializeField] TMP_Text text;
    
    // Update is called once per frame
    void Update()
    {
        text.text = GetString(counter.AverageFPS);
    }

    Dictionary<int, string> map = new Dictionary<int, string>();

    string GetString(int value)
    {

        if(!map.ContainsKey(value))
        {
            map.Add(value, value.ToString());
        }

        return map[value];
    }


}
