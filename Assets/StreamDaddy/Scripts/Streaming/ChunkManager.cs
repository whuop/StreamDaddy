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

        public void PreWarmChunks(List<AssetChunkData> chunkData, List<Terrain> terrains)
        {
            for(int i = 0; i < chunkData.Count; i++)
            {
                AssetChunkData data = chunkData[i];
                var chunkID = new ChunkID(data.ChunkID);

                AddChunk(chunkID, data);
            }

            //Inject terrains into the chunks.
            for(int i = 0; i < terrains.Count; i++)
            {
                Terrain terrain = terrains[i];
                GameObject terrainGO = terrain.gameObject;
                var bounds = terrain.terrainData.bounds;

                Vector3 terrainWorldCenter = terrainGO.transform.position;
                terrainWorldCenter += bounds.extents;

                //  Round to approximate chunk position
                float x = terrainWorldCenter.x / (float)m_chunkSize.x;
                float y = terrainWorldCenter.y / (float)m_chunkSize.y;
                float z = terrainWorldCenter.z / (float)m_chunkSize.z;

                //  Floor to chunk position ID ( chunk index in EditorChunkManager )
                int cx = (int)Mathf.Floor(x);
                int cy = (int)Mathf.Floor(y);
                int cz = (int)Mathf.Floor(z);

                //  If there are no streamed assets in the chunk, then it wont exist.
                //  In this case the chunk needs to be created.
                ChunkID chunkKey = new ChunkID((int)cx, (int)cy, (int)cz);
                if (!m_chunks.ContainsKey(chunkKey))
                {
                    m_chunks.Add(chunkKey,new Chunk(chunkKey, m_coroutineStarter));
                }

                //  Only set the terrain for LOD level 0, which is the one with the highest resolution.
                m_chunks[chunkKey].SetTerrain(terrain, 0);
            }
        }

        public void AddChunk(ChunkID id, AssetChunkData data)
        {
            if (!m_chunks.ContainsKey(id))
            {
                m_chunks.Add(id, new Chunk(data, m_coroutineStarter));
            }
            else
            {
                Debug.Log("Chunk already existed! Could not copy AssetChunkData!");
            }
        }

        public void LoadChunk(int x, int y, int z, int lodLevel)
        {
            LoadChunk(new ChunkID(x, y, z), lodLevel);
        }

        public void LoadChunk(Vector3Int id, int lodLevel)
        {
            LoadChunk(new ChunkID(id), lodLevel);
        }

        public void LoadChunk(ChunkID id, int lodLevel)
        {
            if (!m_chunks.ContainsKey(id))
            {
                return;
            }

            Chunk chunk = m_chunks[id];
            chunk.LoadChunk(lodLevel);
        }

        public void LoadChunks(List<ChunkLODLoader> chunkIDs)
        {
            for(int i = 0; i < chunkIDs.Count; i++)
            {
                var lodLoader = chunkIDs[i];
                LoadChunk(lodLoader.ChunkID, lodLoader.LodLevel);
            }
        }

        public void UnloadChunks(List<ChunkLODLoader> chunkIDs)
        {
            for(int i = 0; i < chunkIDs.Count; i++)
            {
                UnloadChunk(chunkIDs[i].ChunkID);
            }
        }

        public void UnloadChunk(int x, int y, int z)
        {
            UnloadChunk(new ChunkID(x, y, z));
        }

        public void UnloadChunk(Vector3Int id)
        {
            UnloadChunk(new ChunkID(id));
        }

        public void UnloadChunk(ChunkID id)
        {
            if (!m_chunks.ContainsKey(id))
            {
                return;
            }

            Chunk chunk = m_chunks[id];
            chunk.UnloadChunk();
        }
    }

}

