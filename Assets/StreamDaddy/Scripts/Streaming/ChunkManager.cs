using StreamDaddy.AssetManagement;
using StreamDaddy.Chunking;
using StreamDaddy.Pooling;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Streaming
{
    public class ChunkManager
    {
        private Dictionary<ChunkID, Chunk> m_chunks = new Dictionary<ChunkID, Chunk>();
        private MonoBehaviour m_coroutineStarter;

        private List<Chunk> m_unloadList = new List<Chunk>();
        private List<Chunk> m_loadList = new List<Chunk>();

        private Vector3Int m_chunkSize;

        public Dictionary<ChunkID, Chunk>.ValueCollection Chunks
        {
            get { return m_chunks.Values; }
        }

        public ChunkManager(MonoBehaviour coroutineStarter, Vector3Int chunkSize)
        {
            m_chunkSize = chunkSize;
            m_coroutineStarter = coroutineStarter;
            GameObjectPool.PreWarm(3500, 2500, 1500, 1500);
        }

        public int GetChunkCount()
        {
            return m_chunks.Count;
        }

        public void Update()
        {
            //  Loop through all chunks to be loaded and make sure they are.
            for(int i = 0; i < m_loadList.Count; i++)
            {
                Chunk chunk = m_loadList[i];
                if (chunk.State == ChunkState.Unloaded)
                {
                    m_loadList.Remove(chunk);
                    m_coroutineStarter.StartCoroutine(chunk.LoadChunk());
                }
            }
            //  Loop through all chunks that should be unloaded and make sure they are.
            for(int i = 0; i < m_unloadList.Count; i++)
            {
                Chunk chunk = m_unloadList[i];
                if (chunk.State == ChunkState.Loaded)
                {
                    m_unloadList.Remove(chunk);
                    m_coroutineStarter.StartCoroutine(chunk.UnloadChunk());
                }
            }
        }

        public void PreWarmChunks(List<AssetChunkData> chunkData/*, List<Terrain> terrains*/)
        {
            for(int i = 0; i < chunkData.Count; i++)
            {
                AssetChunkData data = chunkData[i];
                var chunkID = new ChunkID(data.ChunkID);

                AddChunk(chunkID, data);
                //  Prewarm chunk making all assets load.
                
            }

            //Inject terrains into the chunks.
            /*for(int i = 0; i < terrains.Count; i++)
            {
                Terrain terrain = terrains[i];
                GameObject terrainGO = terrain.gameObject;
                //  Round to approximate chunk position
                float x = terrainGO.transform.position.x / (float)m_chunkSize.x;
                float y = terrainGO.transform.position.y / (float)m_chunkSize.y;
                float z = terrainGO.transform.position.z / (float)m_chunkSize.z;

                //  Floor to chunk position ID ( chunk index in EditorChunkManager )
                int cx = (int)Mathf.Floor(x);
                int cy = (int)Mathf.Floor(y);
                int cz = (int)Mathf.Floor(z);

                //  If there are no streamed assets in the chunk, then it wont exist.
                //  In this case the chunk needs to be created.
                ChunkID chunkKey = new ChunkID((int)cx, (int)cy, (int)cz);
                if (!m_chunks.ContainsKey(chunkKey))
                {
                    m_chunks.Add(chunkKey,new Chunk(chunkKey));
                }

                m_chunks[chunkKey].SetTerrain(terrain);
            }*/
        }

        public void AddChunk(ChunkID id, AssetChunkData data)
        {
            if (!m_chunks.ContainsKey(id))
            {
                m_chunks.Add(id, new Chunk(data));
            }
            else
            {
                Debug.Log("Chunk already existed! Could not copy AssetChunkData!");
            }
        }

        public void LoadAllChunks()
        {
            var chunks = m_chunks.Values;
            foreach(var chunk in chunks)
            {
                LoadChunk(chunk.ID);
            }
        }

        public void LoadChunk(int x, int y, int z)
        {
            LoadChunk(new ChunkID(x, y, z));
        }

        public void LoadChunk(Vector3Int id)
        {
            LoadChunk(new ChunkID(id));
        }

        public void LoadChunk(ChunkID id)
        {
            if (!m_chunks.ContainsKey(id))
            {
                return;
            }

            Chunk chunk = m_chunks[id];
            if (chunk.State == ChunkState.Loaded || chunk.State == ChunkState.Loading)
                return;
            
            m_loadList.Add(chunk);
        }

        public void LoadChunks(List<ChunkID> chunkIDs)
        {
            for(int i = 0; i < chunkIDs.Count; i++)
            {
                LoadChunk(chunkIDs[i]);
            }
        }

        public void UnloadChunks(List<ChunkID> chunkIDs)
        {
            for(int i = 0; i < chunkIDs.Count; i++)
            {
                UnloadChunk(chunkIDs[i]);
            }
        }

        public void UnloadChunk(int x, int y, int z)
        {
            LoadChunk(new ChunkID(x, y, z));
        }

        public void UnloadChunk(Vector3Int id)
        {
            LoadChunk(new ChunkID(id));
        }

        public void UnloadChunk(ChunkID id)
        {
            if (!m_chunks.ContainsKey(id))
            {
                return;
            }

            Chunk chunk = m_chunks[id];
            if (chunk.State == ChunkState.Unloaded)
                return;

            m_unloadList.Add(chunk);
        }
    }

}

