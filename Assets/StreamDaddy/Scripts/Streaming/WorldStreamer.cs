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

            Debug.Log("Loaded all asset budles for world");
            
            StartCoroutine(LoadAllChunks());
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

        // Update is called once per frame
        void Update()
        {
            //CheckAreasOfInterest();
        }

        private void CheckAreasOfInterest()
        {
            Vector3Int chunkSize = m_worldStream.ChunkSize;

            for(int i = 0; i < m_areasOfInterest.Count; i++)
            {
                AreaOfInterest areaOfInterest = m_areasOfInterest[i];
                Vector3 position = areaOfInterest.transform.position;

                //  Round to approximate chunk position
                float x = position.x / (float)chunkSize.x;
                float y = position.y / (float)chunkSize.y;
                float z = position.z / (float)chunkSize.z;

                //  Floor to chunk position ID ( chunk index in EditorChunkManager )
                int cx = (int)Mathf.Floor(x);
                int cy = (int)Mathf.Floor(y);
                int cz = (int)Mathf.Floor(z);

                Debug.Log("Loading Chunk: " + cx + " " + cy + " " + cz);
                m_chunkManager.LoadChunk(new Chunking.ChunkID(cx, cy, cz));
            }
        }
    }
}


