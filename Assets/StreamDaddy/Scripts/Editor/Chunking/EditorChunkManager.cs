using StreamDaddy.Chunking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Editor.Chunking
{
    public class EditorChunkManager
    {
        private Dictionary<ChunkID, EditorChunk> m_chunks = new Dictionary<ChunkID, EditorChunk>();
        private Vector3Int m_chunkSize;

        public EditorChunkManager()
        {

        }

        public void SetChunkSizeAndClearManager(Vector3Int newChunkSize)
        {
            m_chunkSize = newChunkSize;
            m_chunks.Clear();
        }

        public void AddGameObject(GameObject go)
        {
            //  Round position to chunk positions
            Vector3Int position = new Vector3Int(Mathf.RoundToInt(go.transform.position.x),
                                Mathf.RoundToInt(go.transform.position.y),
                                Mathf.RoundToInt(go.transform.position.z));
        }
    }
}


