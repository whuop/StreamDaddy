using StreamDaddy.AssetManagement;
using StreamDaddy.Chunking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Streaming
{
    public class ChunkManager
    {
        private Dictionary<ChunkID, Chunk> m_chunks = new Dictionary<ChunkID, Chunk>();

        public ChunkManager()
        {

        }

        public int GetChunkCount()
        {
            return m_chunks.Count;
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

        }
    }

}

