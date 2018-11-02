using StreamDaddy.Chunking;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Editor.Chunking
{
    public class EditorChunkManager
    {
        private Dictionary<ChunkID, EditorChunk> m_chunks = new Dictionary<ChunkID, EditorChunk>();
        public List<EditorChunk> Chunks { get { return new List<EditorChunk>(m_chunks.Values); } }

        private Vector3Int m_chunkSize;

        public EditorChunkManager(Vector3Int chunkSize)
        {
            m_chunkSize = chunkSize;
        }

        public void ClearAllChunks(Vector3Int newChunkSize)
        {
            m_chunks.Clear();
        }
        
        public void AddGameObject(GameObject go)
        {
            //  Round to approximate chunk position
            float x = go.transform.position.x / (float)m_chunkSize.x;
            float y = go.transform.position.y / (float)m_chunkSize.y;
            float z = go.transform.position.z / (float)m_chunkSize.z;
            
            //  Floor to chunk position ID ( chunk index in EditorChunkManager )
            int cx = (int)Mathf.Floor(x);
            int cy = (int)Mathf.Floor(y);
            int cz = (int)Mathf.Floor(z);
            
            ChunkID chunkKey = new ChunkID((int)cx, (int)cy, (int)cz);
            //  Create a new chunk if no chunk exists with the given key
            if (!m_chunks.ContainsKey(chunkKey))
            {
                m_chunks.Add(chunkKey, new EditorChunk(chunkKey, m_chunkSize));
            }

            if (m_chunks[chunkKey].HasChild(go))
                return;

            m_chunks[chunkKey].AddChild(go);
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


