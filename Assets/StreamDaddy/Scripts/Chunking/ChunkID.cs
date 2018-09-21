using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Chunking
{
    public class ChunkID
    {
        public Vector3Int m_id;

        public ChunkID(int x, int y, int z)
        {
            m_id = new Vector3Int(x, y, z);
        }

        public ChunkID(Vector3Int position)
        {
            m_id = position;
        }
    }
}

