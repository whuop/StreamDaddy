using System;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.AssetManagement
{
    public class AssetManager : MonoBehaviour
    {
        private IAssetContainer<Mesh> m_meshContainer;
        private IAssetContainer<Material> m_materialContainer;
        private IAssetContainer<AssetChunkData> m_chunkDataContainer;
        
        [SerializeField]
        private List<GameObject> m_builtInMeshes;
        /// <summary>
        /// The terrains that are a part of the world streaming. These terrains are not streamed but simply activated or deactivated
        /// to lower the load on CPU and disk reads.
        /// </summary>
        [SerializeField]
        private List<Terrain> m_sceneTerrains;
        public List<Terrain> SceneTerrains { get { return m_sceneTerrains; } set { m_sceneTerrains = value; } }

        public void Awake()
        {
            m_meshContainer = new AssetContainer<Mesh>();
            m_materialContainer = new AssetContainer<Material>();
            m_chunkDataContainer = new AssetContainer<AssetChunkData>();
            LoadBuiltInResources();
        }

        private void LoadBuiltInResources()
        {
            for(int i = 0; i < m_builtInMeshes.Count; i++)
            {
                var meshObj = m_builtInMeshes[i];
                m_meshContainer.Add(meshObj.name, meshObj.GetComponent<MeshFilter>().sharedMesh);
            }
        }

        public void AddAssets(UnityEngine.Object[] bundle)
        {
            Type meshType = typeof(Mesh);
            Type materialType = typeof(Material);
            Type assetChunkDataType = typeof(AssetChunkData);

            for(int i = 0; i < bundle.Length; i++)
            {
                UnityEngine.Object asset = bundle[i];
                Type type = asset.GetType();

                if (type.IsAssignableFrom(meshType))
                {
                    if (m_meshContainer.Contains(asset.name))
                    {
                        Debug.LogError(string.Format("AssetManager already contains Mesh asset with name {0}", asset.name));
                    }
                    else
                    {
                        m_meshContainer.Add(asset.name, (Mesh)asset);
                    }
                    
                }
                else if (type.IsAssignableFrom(materialType))
                {
                    if (m_materialContainer.Contains(asset.name))
                    {
                        Debug.LogError(string.Format("AssetManager already contains Material asset with name {0}", asset.name));
                    }
                    else
                    {
                        m_materialContainer.Add(asset.name, (Material)asset);
                    }
                }
                else if (type.IsAssignableFrom(assetChunkDataType))
                {
                    if (m_chunkDataContainer.Contains(asset.name))
                    {
                        Debug.LogError(string.Format("AssetManager already contains ChunkData asset with name {0}", asset.name));
                    }
                    else
                    {
                        m_chunkDataContainer.Add(asset.name, (AssetChunkData)asset);
                    }
                    
                }

                Debug.Log("Added asset: " + asset.name);
            }
        }

        public void RemoveAsset<T>(string name) where T : UnityEngine.Object
        {
            Type t = typeof(T);
            if (t.IsAssignableFrom(typeof(Mesh)))
            {
                m_meshContainer.Remove(name);
            }
            else if (t.IsAssignableFrom(typeof(Material)))
            {
                m_materialContainer.Remove(name);
            }
            else if (t.IsAssignableFrom(typeof(AssetChunkData)))
            {
                m_chunkDataContainer.Remove(name);
            }
        }

        public void RemoveAssets(string[] names)
        {
            for(int i = 0; i < names.Length; i++)
            {
                string assetName = names[i];
                m_meshContainer.Remove(assetName);
                m_materialContainer.Remove(assetName);
                m_chunkDataContainer.Remove(assetName);
            }
        }

        public Mesh GetMeshAsset(string assetName)
        {
            return m_meshContainer.Get(assetName);
        }
        
        public Material GetMaterialAsset(string assetName)
        {
            return m_materialContainer.Get(assetName);
        }

        public AssetChunkData GetAssetChunkData(string assetName)
        {
            return m_chunkDataContainer.Get(assetName);
        }
        
        public AssetChunkData[] GetAllAssetChunkData()
        {
            return m_chunkDataContainer.GetAllAssets();
        }
    }
}


