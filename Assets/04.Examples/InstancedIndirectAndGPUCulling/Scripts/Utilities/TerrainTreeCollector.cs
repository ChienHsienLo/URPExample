using UnityEngine;

namespace InstancedIndirect
{
    public class TerrainTreeCollector : MonoBehaviour
    {
        public Terrain terrain;
        public IndirectRenderer indirectRenderer;

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
            if (terrain == null || indirectRenderer == null)
            {
                return;
            }

            ResetData();

            TreeInstance[] trees = terrain.terrainData.treeInstances;

            TreeInstance treeInstance = terrain.terrainData.treeInstances[0];
            TreePrototype treePrototype = terrain.terrainData.treePrototypes[treeInstance.prototypeIndex];

            indirectRenderer.mesh = treePrototype.prefab.GetComponent<MeshFilter>().sharedMesh;
            indirectRenderer.material = treePrototype.prefab.GetComponent<Renderer>().sharedMaterial;


            if (tempTreeTransform == null)
            {
                tempTreeTransform = new GameObject("temp tree transform : delete this").transform;
            }


            for (int i = 0; i < trees.Length; i++)
            {
                TreeInstance tree = trees[i];

                GetInstanceData(indirectRenderer.instanceData, tree);
            }

            if (tempTreeTransform != null)
            {
                DestroyImmediate(tempTreeTransform.gameObject);
            }

        }


        Quaternion RadiansToQuaternion(Vector3 radians)
        {
            Vector3 degrees = radians * Mathf.Rad2Deg; // Convert radians to degrees
            return Quaternion.Euler(degrees); // Create quaternion from euler angles in degrees
        }

        //Matrix4x4 GetTreeToWorldMatrix(TreeInstance treeInstance)
        //{
        //    // Convert the normalized position to world position
        //    Vector3 position = Vector3.Scale(treeInstance.position, terrain.terrainData.size) + terrain.transform.position;

        //    // Scale matrix
        //    float heightScale = treeInstance.heightScale;
        //    float widthScale = treeInstance.widthScale;
        //    Matrix4x4 scaleMatrix = Matrix4x4.Scale(new Vector3(widthScale, heightScale, widthScale));

        //    // Combine translation and scale
        //    Matrix4x4 matrix = Matrix4x4.Translate(position) * scaleMatrix;
        //    matrix = Matrix4x4.TRS(position, RadiansToQuaternion(new Vector3(0.0f, treeInstance.rotation, 0.0f)), new Vector3(widthScale, heightScale, widthScale));

        //    return matrix;
        //}

        //Matrix4x4 GetWorldToObjectMatrix(TreeInstance treeInstance)
        //{
        //    Matrix4x4 treeToWorldMatrix = GetTreeToWorldMatrix(treeInstance);
        //    Matrix4x4 worldToObjectMatrix = treeToWorldMatrix.inverse;
        //    return worldToObjectMatrix;
        //}


        Transform tempTreeTransform;
        void GetInstanceData(InstanceData data, TreeInstance tree)
        {
            tempTreeTransform.position = new Vector3(tree.position.x * terrain.terrainData.size.x,
                tree.position.y * terrain.terrainData.size.y,
                tree.position.z * terrain.terrainData.size.z);

            Vector3 scale = new Vector3(tree.widthScale, tree.heightScale, tree.widthScale);
            Quaternion rotation = RadiansToQuaternion(new Vector3(0.0f, tree.rotation, 0.0f));

            tempTreeTransform.localScale = scale;
            tempTreeTransform.rotation = rotation;

            Matrix4x4 o2w = tempTreeTransform.localToWorldMatrix;

            Matrix4x4 w2o = tempTreeTransform.worldToLocalMatrix;

            data.o2w.Add(o2w);
            data.w2o.Add(w2o);
            data.worldPosition.Add(tempTreeTransform.position);
        }
    }
}
