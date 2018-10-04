using StreamDaddy.AssetManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Streaming
{
    public enum ChunkState
    {
        Unloaded = 0,
        PreWarmed = 1,
        Loading = 2,
        Loaded = 3
    }

    public class Chunk
    {
        private AssetChunkData m_chunkData;

        private ChunkState m_chunkState;
        

        
        public Chunk()
        {
            m_chunkState = ChunkState.Unloaded;
        }

        public void PreWarm(AssetChunkData data)
        {
            m_chunkData = data;
            m_chunkState = ChunkState.PreWarmed;
        }

        public IEnumerable LoadChunk()
        {
            m_chunkState = ChunkState.Loading;


            m_chunkState = ChunkState.Loaded;
            yield return null;
        }

        public IEnumerable UnloadChunk()
        {
            m_chunkState = ChunkState.PreWarmed;

            yield return null;
        }
    }
}

