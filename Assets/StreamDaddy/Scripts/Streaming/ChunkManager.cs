﻿using StreamDaddy.AssetManagement;
using StreamDaddy.Chunking;
using StreamDaddy.Pooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Streaming
{
    public class ChunkManager
    {
        private Dictionary<ChunkID, Chunk> m_chunks = new Dictionary<ChunkID, Chunk>();

        private AssetManager m_assetManager;

        private MonoBehaviour m_coroutineStarter;

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

            m_coroutineStarter.StartCoroutine(chunk.LoadChunk(m_assetManager));
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
            if (chunk.State == ChunkState.Loaded)
            {
                chunk.UnloadChunk();
            }
        }
    }

}
