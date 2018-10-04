using System;
using UnityEngine;

namespace StreamDaddy.AssetManagement
{
    public class AssetManager : MonoBehaviour
    {
        private IAssetContainer<Mesh> m_meshContainer;
        private IAssetContainer<Material> m_materialContainer;
        private IAssetContainer<AssetChunkData> m_chunkDataContainer;

        public void Awake()
        {
            m_meshContainer = new AssetContainer<Mesh>();
            m_materialContainer = new AssetContainer<Material>();
            m_chunkDataContainer = new AssetContainer<AssetChunkData>();
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
                    m_meshContainer.Add(asset.name, (Mesh)asset);
                }
                else if (type.IsAssignableFrom(materialType))
                {
                    m_materialContainer.Add(asset.name, (Material)asset);
                }
                else if (type.IsAssignableFrom(assetChunkDataType))
                {
                    m_chunkDataContainer.Add(asset.name, (AssetChunkData)asset);
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


