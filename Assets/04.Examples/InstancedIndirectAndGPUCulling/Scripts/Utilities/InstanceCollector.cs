using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InstancedIndirect
{
    public class InstanceCollector : MonoBehaviour
    {
        public IndirectRenderer indirectRenderer;
        public Transform root;

        void ResetData()
        {
            if (indirectRenderer.instanceData == null)
            {
                indirectRenderer.instanceData = new InstanceData();
            }

            indirectRenderer.instanceData.o2w.Clear();
            indirectRenderer.instanceData.w2o.Clear();
            indirectRenderer.instanceData.worldPosition.Clear();
        }

        public void Collect()
        {
            if (root == null || indirectRenderer == null)
            {
                return;
            }

            ResetData();

            List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
            root.GetComponentsInChildren<MeshRenderer>(true, meshRenderers);

            if (meshRenderers == null || meshRenderers.Count <= 0)
            {
                return;
            }

            MeshRenderer mr = meshRenderers[0];
            indirectRenderer.material = mr.sharedMaterial;
            indirectRenderer.mesh = mr.GetComponent<MeshFilter>().sharedMesh;
            indirectRenderer.shadowCastingMode = mr.shadowCastingMode;
            indirectRenderer.receiveShadow = mr.receiveShadows;
            indirectRenderer.lightProbeUsage = mr.lightProbeUsage;
            indirectRenderer.layer = mr.gameObject.layer;

            for (int i = 0; i < meshRenderers.Count; i++)
            {
                Transform t = meshRenderers[i].transform;

                GetInstanceData(indirectRenderer.instanceData, t);
            }
        }

        void GetInstanceData(InstanceData data, Transform t)
        {
            data.o2w.Add(t.localToWorldMatrix);
            data.w2o.Add(t.worldToLocalMatrix);
            data.worldPosition.Add(t.position);
        }
    }
}