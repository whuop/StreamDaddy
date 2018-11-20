using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StreamDaddy.AssetManagement
{
    public class AssetChunkData : ScriptableObject
    {
        /// <summary>
        /// Renderable meshes 
        /// </summary>
        public MeshLayerData[] MeshLayers;
        public MaterialData[] MeshMaterials;
        public TransformData[] MeshTransforms;

        /// <summary>
        /// Colliders
        /// </summary>
        public BoxColliderData[] BoxColliders;
        public SphereColliderData[] SphereColliders;

        /// <summary>
        /// Mesh Colliders, these are special compared to the other colliders as they
        /// have a mesh. Which means we might want to LOD them as well.
        /// </summary>
        public MeshLayerData[] MeshColliderLayers;
        public TransformData[] MeshColliderTransforms;
        
        /// <summary>
        /// Where the Chunk is located in the world.
        /// </summary>
        public Vector3Int ChunkID;
    }

    [System.Serializable]
    public class MeshLayerData
    {
        public MeshData[] Meshes;
    }

    [System.Serializable]
    public class TransformData
    {
        [SerializeField]
        public Vector3 Position;
        [SerializeField]
        public Vector3 Rotation;
        [SerializeField]
        public Vector3 Scale;
    }

    [System.Serializable]
    public class MeshData
    {
        [SerializeField]
        public AssetReference MeshReference;
        [SerializeField]
        public string SubMeshName;

        public Hash128 RuntimeHash;
    }

    [System.Serializable]
    public class MaterialData
    {
        [SerializeField]
        public AssetReference[] MaterialReferences;
    }

    [System.Serializable]
    public class BoxColliderData : TransformData
    {
        [SerializeField]
        public Vector3 Center;
        [SerializeField]
        public Vector3 Size;
    }

    [System.Serializable]
    public class SphereColliderData : TransformData
    {
        [SerializeField]
        public Vector3 Center;
        [SerializeField]
        public float Radius;
    }
    /*
#if UNITY_EDITOR

    [CustomEditor(typeof(AssetChunkData))]
    public class AssetChunkDataEditor : Editor
    {
        private AssetChunkData m_chunkData;

        private bool[] m_meshLayerFoldouts;

        public void OnEnable()
        {
            m_chunkData = (AssetChunkData)target;

            m_meshLayerFoldouts = new bool[m_chunkData.MeshLayers.Length];
            for(int i = 0; i < m_meshLayerFoldouts.Length; i++)
            {
                m_meshLayerFoldouts[i] = false;
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Mesh Layers");
            var assetSettings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;

            EditorGUI.indentLevel = 1;
            for (int i = 0; i < m_chunkData.MeshLayers.Length; i++)
            {
                m_meshLayerFoldouts[i] = EditorGUILayout.Foldout(m_meshLayerFoldouts[i], "LOD " + i);
                if (m_meshLayerFoldouts[i])
                {
                    for(int j = 0; j < m_chunkData.MeshLayers[i].Meshes.Length; j++)
                    {
                        var meshData = m_chunkData.MeshLayers[i].Meshes[j];
                        
                        EditorGUILayout.LabelField(meshData.MeshReference.ToString());
                    }
                }
            }
            EditorGUI.indentLevel = 0;

        }
    }

#endif
*/
}


