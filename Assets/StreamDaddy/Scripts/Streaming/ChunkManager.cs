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
        private AssetManager m_assetManager;
        private MonoBehaviour m_coroutineStarter;

        private List<Chunk> m_unloadList = new List<Chunk>();
        private List<Chunk> m_loadList = new List<Chunk>();

        public Dictionary<ChunkID, Chunk>.ValueCollection Chunks
        {
            get { return m_chunks.Values; }
        }

        public ChunkManager(AssetManager assetManager)
        {
            m_assetManager = assetManager;
            m_coroutineStarter = assetManager;
            GameObjectPool.PreWarm(2500, 500, 500, 500);
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
                    m_coroutineStarter.StartCoroutine(chunk.LoadChunk(m_assetManager));
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

        public void PreWarmChunks(AssetChunkData[] chunkData)
        {
            for(int i = 0; i < chunkData.Length; i++)
            {
                AssetChunkData data = chunkData[i];
                var chunkID = data.ChunkID;

                AddChunk(new ChunkID(chunkID), data);
            }
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

