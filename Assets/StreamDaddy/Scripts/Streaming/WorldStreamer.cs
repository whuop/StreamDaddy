using StreamDaddy.AssetManagement;
using StreamDaddy.Pooling;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace StreamDaddy.Streaming
{
    public class WorldStreamer : MonoBehaviour
    {
        [SerializeField]
        private WorldStream m_worldStream;
        public WorldStream WorldStream { get { return m_worldStream; } set { m_worldStream = value; } }

        [SerializeField]
        private float m_areaOfInterestCheckTime = 0.2f;

        [SerializeField]
        private List<Terrain> m_worldTerrains = new List<Terrain>();
        public List<Terrain> WorldTerrains { get { return m_worldTerrains; } set { m_worldTerrains = value; } }

        [SerializeField]
        private bool m_debugRender = true;

        [SerializeField]
        private int m_numRenderables;
        [SerializeField]
        private int m_numBoxColliders;
        [SerializeField]
        private int m_numSphereColliders;
        [SerializeField]
        private int m_numMeshColliders;

        private ChunkManager m_chunkManager;
        
        private List<AreaOfInterest> m_areasOfInterest = new List<AreaOfInterest>();

        public Vector3Int ChunkSize { get { return m_worldStream.ChunkSize; } }
        
        private void Awake()
        {
            m_chunkManager = new ChunkManager(this, m_worldStream.ChunkSize);
            AddressablesLoader.Initialize(OnFinishedLoadingLayouts);
        }

        private void OnDestroy()
        {
        }

        // Use this for initialization
        void Start()
        {
            PrewarmWorld();
        }

        private void Update()
        {
            m_numRenderables = GameObjectPool.CreatedRenderers;
            m_numBoxColliders = GameObjectPool.CreatedBoxColliders;
            m_numSphereColliders = GameObjectPool.CreatedSphereColliders;
            m_numMeshColliders = GameObjectPool.CreatedMeshColliders;
        }

        private void PrewarmWorld()
        {
            Debug.Log(string.Format("[StreamDaddy] Loading bundle {0}", m_worldStream.ChunkLayoutBundle));

            AddressablesLoader.LoadWorldLayouts(m_worldStream);
            
        }

        private void OnFinishedLoadingLayouts(List<AssetChunkData> chunkLayouts)
        {
            Debug.Log("Finished loading layouts!!");
            m_chunkManager.PreWarmChunks(chunkLayouts, m_worldTerrains);

            for(int i = 0; i < chunkLayouts.Count; i++)
            {
                AddressablesLoader.LoadChunkAssets(chunkLayouts[i]);
            }

            //  Start the area of interest check.
            StartCoroutine(CheckAreasOfInterest());
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
            yield return new WaitForSeconds(5.0f);
            Debug.Log("Booting up AOI check");
            //PrewarmWorld();
            Vector3Int chunkSize = m_worldStream.ChunkSize;
            while (true)
            {
                for (int i = 0; i < m_areasOfInterest.Count; i++)
                {
                    AreaOfInterest areaOfInterest = m_areasOfInterest[i];
                    bool changed = areaOfInterest.UpdateChunkPosition();

                    if (changed)
                    {
                        m_chunkManager.LoadChunks(areaOfInterest.PositiveDelta);
                        m_chunkManager.UnloadChunks(areaOfInterest.NegativeDelta);
                    }
                    
                    yield return new WaitForEndOfFrame();
                }
                yield return new WaitForSeconds(m_areaOfInterestCheckTime);
            }
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (!m_debugRender)
                return;

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


