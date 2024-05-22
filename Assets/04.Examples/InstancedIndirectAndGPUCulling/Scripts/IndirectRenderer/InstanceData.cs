using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InstancedIndirect
{
    [System.Serializable]
    public class InstanceData
    {
        public int count
        {
            get
            {
                if (o2w == null)
                {
                    return 0;
                }

                return o2w.Count;
            }
        }

        public List<Matrix4x4> o2w;
        public List<Matrix4x4> w2o;
        public List<Vector4> worldPosition;

        public InstanceData()
        {
            o2w = new List<Matrix4x4>();
            w2o = new List<Matrix4x4>();
            worldPosition = new List<Vector4>();
        }
    }
}