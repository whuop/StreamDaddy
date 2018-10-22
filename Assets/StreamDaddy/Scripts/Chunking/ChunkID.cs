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

        public static bool operator !=(ChunkID a, ChunkID b)
        {
            return !a.Equals(b);
        }

        public static bool operator ==(ChunkID a, ChunkID b)
        {
            return a.Equals(b);
        }

        public static ChunkID operator + (ChunkID a, ChunkID b)
        {
            return new ChunkID(a.X + b.X,
                                a.Y + b.Y,
                                a.Z + b.Z);
        }

        public static ChunkID operator - (ChunkID a, ChunkID b)
        {
            return new ChunkID(a.X - b.X,
                                a.Y - b.Y,
                                a.Z - b.Z);
        }

        public Vector3 AsVector3()
        {
            return new Vector3(X, Y, Z);
        }

        public override int GetHashCode()
        {
            var hashCode = -307843816;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            return hashCode;
        }

        public static ChunkID FromVector3(Vector3 worldPosition, Vector3Int chunkSize)
        {
            //  Round to approximate chunk position
            float x = worldPosition.x / (float)chunkSize.x;
            float y = worldPosition.y / (float)chunkSize.y;
            float z = worldPosition.z / (float)chunkSize.z;

            //  Floor to chunk position ID ( chunk index in EditorChunkManager )
            int cx = (int)Mathf.Floor(x);
            int cy = (int)Mathf.Floor(y);
            int cz = (int)Mathf.Floor(z);

            return new ChunkID(cx, cy, cz);
        }
    }
}

