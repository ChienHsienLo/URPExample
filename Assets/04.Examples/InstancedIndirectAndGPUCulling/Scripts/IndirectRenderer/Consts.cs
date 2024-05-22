using UnityEngine;

namespace InstancedIndirect
{
    public class Consts
    {
        public static int vpMatrixID = Shader.PropertyToID("_vpMatrix");
        public static int worldPositionID = Shader.PropertyToID("_worldPosition");
        public static int visibleID = Shader.PropertyToID("_visibleID");
        public static int maxDistanceID = Shader.PropertyToID("_maxDistance");
        public static int boundExtendID = Shader.PropertyToID("_boundExtend");
        public static int maxCountID = Shader.PropertyToID("_maxCount");
        public static int camPositionWSID = Shader.PropertyToID("_camPositinWS");
        public static string cullingCSKernel = "CSCullingMain";
    }
}
