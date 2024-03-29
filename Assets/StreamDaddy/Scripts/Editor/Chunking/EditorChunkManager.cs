﻿using StreamDaddy.Chunking;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Editor.Chunking
{
    public class EditorChunkManager
    {
        private Dictionary<ChunkID, EditorChunk> m_chunks = new Dictionary<ChunkID, EditorChunk>();
        public List<EditorChunk> Chunks { get { return new List<EditorChunk>(m_chunks.Values); } }

        private Vector3Int m_chunkSize;
        private string m_worldName;

        public EditorChunkManager(Vector3Int chunkSize, string worldName)
        {
            m_chunkSize = chunkSize;
            m_worldName = worldName;
        }

        public void ClearAllChunks(Vector3Int newChunkSize)
        {
            m_chunks.Clear();
        }

        public void AddMeshFilter(MeshFilter filter, Vector3 position)
        {
            EditorChunk chunk = CreateChunkIfMissing(position);

            chunk.AddMeshFilter(filter);
        }

        public void AddCollider(Collider collider, Vector3 position)
        {
            EditorChunk chunk = CreateChunkIfMissing(position);

            chunk.AddCollider(collider);
        }

        public void SetTerrain(Terrain terrain, Vector3 position)
        {
            EditorChunk chunk = CreateChunkIfMissing(position);

            chunk.SetTerrain(terrain);
        }

        private EditorChunk CreateChunkIfMissing(Vector3 position)
        {
            //  Round to approximate chunk position
            float x = position.x / (float)m_chunkSize.x;
            float y = position.y / (float)m_chunkSize.y;
            float z = position.z / (float)m_chunkSize.z;

            //  Floor to chunk position ID ( chunk index in EditorChunkManager )
            int cx = (int)Mathf.Floor(x);
            int cy = (int)Mathf.Floor(y);
            int cz = (int)Mathf.Floor(z);

            ChunkID chunkKey = new ChunkID((int)cx, (int)cy, (int)cz);
            //  Create a new chunk if no chunk exists with the given key
            if (!m_chunks.ContainsKey(chunkKey))
            {
                m_chunks.Add(chunkKey, new EditorChunk(chunkKey, m_chunkSize, m_worldName));
            }

            return m_chunks[chunkKey];
        }

        public EditorChunk GetChunk(ChunkID id)
        {
            return m_chunks[id];
        }

        public EditorChunk[] GetAllChunks()
        {
            EditorChunk[] chunks = new EditorChunk[m_chunks.Values.Count];
            m_chunks.Values.CopyTo(chunks, 0);
            return chunks;
        }

        public void Draw()
        {
            foreach(var kvp in m_chunks)
            {
                kvp.Value.Draw();
            }
        }
    }
}


