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

        [SerializeField]
        private float m_areaOfInterestCheckTime = 0.2f;

        private AssetBundleManager m_bundleManager;
        private AssetManager m_assetManager;

        private ChunkManager m_chunkManager;
        public int NumChunks = 0;

        private List<AreaOfInterest> m_areasOfInterest = new List<AreaOfInterest>();

        public Vector3Int ChunkSize { get { return m_worldStream.ChunkSize; } }

        

        private void Awake()
        {
            m_bundleManager = GetComponent<AssetBundleManager>();
            m_assetManager = GetComponent<AssetManager>();
            m_chunkManager = new ChunkManager(m_assetManager);
        }

        // Use this for initialization
        void Start()
        {
            for(int i = 0; i < m_worldStream.AssetBundles.Length; i++)
            {
                string bundleName = m_worldStream.AssetBundles[i];
                m_bundleManager.LoadBundle(bundleName);
            }

            m_bundleManager.LoadBundle(m_worldStream.ChunkLayoutBundle);

            PrewarmWorld();

            Debug.Log("Loaded all asset budles for world");

            StartCoroutine(LoadAllChunks());
            //StartCoroutine(CheckAreasOfInterest());
        }

        private void PrewarmWorld()
        {
            AssetChunkData[] chunkData = m_assetManager.GetAllAssetChunkData();
            m_chunkManager.PreWarmChunks(chunkData);
        }

        private IEnumerator LoadAllChunks()
        {
            yield return new WaitForSeconds(2.0f);

            AssetChunkData[] chunkData = m_assetManager.GetAllAssetChunkData();
            m_chunkManager.PreWarmChunks(chunkData);

            NumChunks = m_chunkManager.GetChunkCount();
            Debug.Log("Prewarmed chunks for world");

            m_chunkManager.LoadAllChunks();
            Debug.Log("Loaded chunks!");
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

        private IEnumerator CheckAreasOfInterest()
        {
            yield return new WaitForSeconds(2.0f);
            while(true)
            {
                Vector3Int chunkSize = m_worldStream.ChunkSize;

                for (int i = 0; i < m_areasOfInterest.Count; i++)
                {
                    AreaOfInterest areaOfInterest = m_areasOfInterest[i];
                    m_chunkManager.LoadChunk(areaOfInterest.ChunkPosition);
                    yield return new WaitForEndOfFrame();
                }

                yield return new WaitForSeconds(m_areaOfInterestCheckTime);
            }
            
        }
    }
}


