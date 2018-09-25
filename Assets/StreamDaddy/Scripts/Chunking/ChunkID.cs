using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Chunking
{
    public struct ChunkID
    {
        private Vector3Int m_id;
        public Vector3Int ID { get { return m_id; } }

        public int X { get { return m_id.x; } set { m_id.x = value; } }
        public int Y { get { return m_id.y; } set { m_id.y = value; } }
        public int Z { get { return m_id.z; } set { m_id.z = value; } }

        public ChunkID(int x, int y, int z)
        {
            m_id = new Vector3Int(x, y, z);
        }

        public ChunkID(Vector3Int position)
        {
            m_id = position;
        }

        public override string ToString()
        {
            return "{x:" + X + " y:" + Y + " z:" + Z + "}";
        }

        public override bool Equals(object obj)
        {
            ChunkID id = (ChunkID)obj;
            if (id.X == X && id.Y == Y && id.Z == Z)
                return true;
            return false;
        }
    }
}

