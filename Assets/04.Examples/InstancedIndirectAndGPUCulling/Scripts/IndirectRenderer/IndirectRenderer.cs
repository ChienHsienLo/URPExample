using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


namespace InstancedIndirect
{
    [System.Serializable]
    public class IndirectRenderer : MonoBehaviour
    {
        public Material material;
        public Mesh mesh;
        public ShadowCastingMode shadowCastingMode;
        public bool receiveShadow;
        public Bounds bounds;
        public int layer;
        public LightProbeUsage lightProbeUsage;

        public ComputeShader cullingShader;
        public float cullDistance;
        public Vector4 instanceBooundExtents;

        public InstanceData instanceData;

        int idO2W = Shader.PropertyToID("_O2WBuffer");
        int idW2O = Shader.PropertyToID("_W2OBuffer");
        int idVisibleID = Shader.PropertyToID("_VisibleIDBuffer");
        Camera _cam;

        public int instanceCount
        {
            get
            {
                if (instanceData == null)
                {
                    return 0;
                }

                return instanceData.count;
            }
        }

        ComputeBuffer bufferWithArgs;

        ComputeBuffer o2wBuffer;
        ComputeBuffer w2oBuffer;
        ComputeBuffer worldPosBuffer;
        ComputeBuffer visibleIDBuffer;

        private void Awake()
        {
            Init();
        }

        void BuildBuffers()
        {
            ReleaseBuffers();

            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = (uint)mesh.GetIndexCount(0);
            args[1] = (uint)instanceData.count;
            args[2] = (uint)mesh.GetIndexStart(0);
            args[3] = (uint)mesh.GetBaseVertex(0);
            args[4] = (uint)0;

            bufferWithArgs = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            bufferWithArgs.SetData(args);

            o2wBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 16);
            w2oBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 16);
            worldPosBuffer = new ComputeBuffer(instanceCount, sizeof(float) * 4);
            visibleIDBuffer = new ComputeBuffer(instanceCount, sizeof(uint), ComputeBufferType.Append);

            o2wBuffer.SetData(instanceData.o2w);
            w2oBuffer.SetData(instanceData.w2o);
            worldPosBuffer.SetData(instanceData.worldPosition);

            material.SetBuffer(idO2W, o2wBuffer);
            material.SetBuffer(idW2O, w2oBuffer);
            material.SetBuffer(idVisibleID, visibleIDBuffer);
        }

        public void Init()
        {
            BuildBuffers();
        }

        public void Render()
        {
            if (mesh == null || material == null)
            {
                return;
            }

            if (cullingShader == null)
            {
                return;
            }

            visibleIDBuffer.SetCounterValue(0);

            Camera cam = GetCamera();
            Matrix4x4 vMatrix = cam.worldToCameraMatrix;
            Matrix4x4 pMatrix = cam.projectionMatrix;
            Matrix4x4 vpMatrix = pMatrix * vMatrix;

            int kernelID = cullingShader.FindKernel(Consts.cullingCSKernel);
            cullingShader.GetKernelThreadGroupSizes(kernelID, out uint x, out uint y, out uint z);

            cullingShader.SetMatrix(Consts.vpMatrixID, vpMatrix);
            cullingShader.SetBuffer(kernelID, Consts.worldPositionID, worldPosBuffer);
            cullingShader.SetBuffer(kernelID, Consts.visibleID, visibleIDBuffer);
            cullingShader.SetFloat(Consts.maxDistanceID, cullDistance);
            cullingShader.SetVector(Consts.boundExtendID, instanceBooundExtents);
            cullingShader.SetInt(Consts.maxCountID, instanceCount);

            Vector4 camPos = cam.transform.position;
            cullingShader.SetVector(Consts.camPositionWSID, camPos);
            cullingShader.Dispatch(kernelID, Mathf.CeilToInt((float)instanceCount / x), 1, 1);

            ComputeBuffer.CopyCount(visibleIDBuffer, bufferWithArgs, sizeof(uint));
            material.SetBuffer(idVisibleID, visibleIDBuffer);
            Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, bufferWithArgs, 0, null, shadowCastingMode, receiveShadow, layer, null, lightProbeUsage);
        }

        void ReleaseBuffers()
        {
            ReleaseBuffer(bufferWithArgs);
            ReleaseBuffer(o2wBuffer);
            ReleaseBuffer(w2oBuffer);
            ReleaseBuffer(worldPosBuffer);
            ReleaseBuffer(visibleIDBuffer);
        }

        void ReleaseBuffer(ComputeBuffer buffer)
        {
            if (buffer != null)
            {
                buffer.Release();
            }
        }

        Camera GetCamera()
        {
            if (_cam == null)
            {
                _cam = Camera.main;
            }

            return _cam;
        }

        void OnDisable()
        {
            ReleaseBuffers();
        }

        void OnDestroy()
        {
            ReleaseBuffers();
        }

    }
}