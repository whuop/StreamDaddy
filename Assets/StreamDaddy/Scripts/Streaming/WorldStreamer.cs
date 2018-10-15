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

            m_bundleManager.OnFinishedLoadingBundles += FinishedLoadingBundles;
        }

        private void OnDestroy()
        {
            m_bundleManager.OnFinishedLoadingBundles -= FinishedLoadingBundles;
        }

        // Use this for initialization
        void Start()
        {
            string[] bundles = new string[m_worldStream.AssetBundles.Length + 1];

            for(int i = 0; i < m_worldStream.AssetBundles.Length; i++)
            {
                bundles[i] = m_worldStream.AssetBundles[i];
            }
            bundles[bundles.Length - 1] = m_worldStream.ChunkLayoutBundle;
            
            m_bundleManager.LoadBundles(bundles);
            Debug.Log("Loaded all asset bundles for world");
        }

        private void FinishedLoadingBundles()
        {
            StartCoroutine(CheckAreasOfInterest());
        }

        private void PrewarmWorld()
        {
            AssetChunkData[] chunkData = m_assetManager.GetAllAssetChunkData();
            m_chunkManager.PreWarmChunks(chunkData);
        }

        private IEnumerator LoadAllChunks()
        {
            yield return new WaitForSeconds(2.0f);

            PrewarmWorld();

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
            Debug.Log("Booting up AOI check");
            PrewarmWorld();
            Vector3Int chunkSize = m_worldStream.ChunkSize;
            while (true)
            {
                for (int i = 0; i < m_areasOfInterest.Count; i++)
                {
                    AreaOfInterest areaOfInterest = m_areasOfInterest[i];
                    areaOfInterest.UpdateChunkPosition();

                    m_chunkManager.LoadChunks(areaOfInterest.PositiveDelta);
                    m_chunkManager.UnloadChunks(areaOfInterest.NegativeDelta);

                    yield return new WaitForEndOfFrame();
                }
                yield return new WaitForSeconds(m_areaOfInterestCheckTime);
            }
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            var chunks = m_chunkManager.Chunks;
            Color color = Gizmos.color;
            foreach (var chunk in chunks)
            {
                Gizmos.color = Color.red;

                Vector3 chunkPos = chunk.ID.ID;
                Vector3 chunkSize = ChunkSize;
                chunkPos.x *= chunkSize.x;
                chunkPos.y *= chunkSize.y;
                chunkPos.z *= chunkSize.z;
                chunkPos += chunkSize * 0.5f;

                Gizmos.DrawWireCube(chunkPos, chunkSize);
            }
            Gizmos.color = color;
        }

#endif
    }
}


