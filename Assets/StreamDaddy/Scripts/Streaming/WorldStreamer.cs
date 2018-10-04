using StreamDaddy.AssetManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Streaming
{
    public class WorldStreamer : MonoBehaviour
    {
        [SerializeField]
        private WorldStream m_worldStream;

        private AssetBundleManager m_bundleManager;
        private AssetManager m_assetManager;

        private ChunkManager m_chunkManager;
        public int NumChunks = 0;

        private List<AreaOfInterest> m_areasOfInterest = new List<AreaOfInterest>();

        private void Awake()
        {
            m_bundleManager = GetComponent<AssetBundleManager>();
            m_assetManager = GetComponent<AssetManager>();
            m_chunkManager = new ChunkManager();
        }

        // Use this for initialization
        void Start()
        {
            for(int i = 0; i < m_worldStream.AssetBundles.Length; i++)
            {
                string bundleName = m_worldStream.AssetBundles[i];
                m_bundleManager.LoadBundle(bundleName);
            }

            Debug.Log("Loaded all asset budles for world");

            AssetChunkData[] chunkData = m_assetManager.GetAllAssetChunkData();
            m_chunkManager.PreWarmChunks(chunkData);
            NumChunks = m_chunkManager.GetChunkCount();

            Debug.Log("Prewarmed chunks for world");
        }

        public void AddAreaOfInterest(AreaOfInterest aoi)
        {
            m_areasOfInterest.Add(aoi);
            Debug.Log("Added area of interest!");
        }

        public void RemoveAreaOfInterest(AreaOfInterest aoi)
        {
            m_areasOfInterest.Remove(aoi);
            Debug.Log("Removed area of interest!");
        }

        // Update is called once per frame
        void Update()
        {
            CheckAreasOfInterest();
        }

        private void CheckAreasOfInterest()
        {
        }
    }
}


