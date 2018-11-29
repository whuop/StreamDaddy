using StreamDaddy.Chunking;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Streaming
{
    public struct ChunkLODLoader
    {
        public ChunkID ChunkID;
        public int LodLevel;

        public override bool Equals(object obj)
        {
            ChunkLODLoader other = (ChunkLODLoader)obj;
            return this.ChunkID.Equals(other.ChunkID);
        }

        public override int GetHashCode()
        {
            return this.ChunkID.GetHashCode();
        }
    }

    public class AreaOfInterest : MonoBehaviour
    {
        private struct DepthNode
        {
            public ChunkID ID;
            public int Depth;

            public DepthNode(ChunkID id, int depth)
            {
                ID = id;
                Depth = depth;
            }

            public override bool Equals(object obj)
            {
                DepthNode otherNode = (DepthNode)obj;
                if (this.ID.X == otherNode.ID.X &&
                    this.ID.Y == otherNode.ID.Y &&
                    this.ID.Z == otherNode.ID.Z)
                    return true;
                return false;
            }

            public override int GetHashCode()
            {
                return 1213502048 + EqualityComparer<ChunkID>.Default.GetHashCode(ID);
            }
        }

        /// <summary>
        /// The size of the area to be loaded.
        /// The size is in number of chunks, not in any other unit of measurement.
        /// </summary>
        [SerializeField]
        private int m_lod0Depth = 2;
        [SerializeField]
        private int m_lod1Depth = 1;
        [SerializeField]
        private int m_lod2Depth = 1;
        [SerializeField]
        private int m_lod3Depth = 1;

        [SerializeField]
        private bool m_debugLod0 = false;
        [SerializeField]
        private bool m_debugLod1 = false;
        [SerializeField]
        private bool m_debugLod2 = false;
        [SerializeField]
        private bool m_debugLod3 = false;
        [SerializeField]
        private bool m_debugNegativeDelta = false;
        [SerializeField]
        private bool m_debugPositiveDelta = false;

        /// <summary>
        /// The combined depth is the max depth of the search for all the different LOD levels to load.
        /// </summary>
        private int m_combinedDepth;
        
        private WorldStreamer m_streamer;

        private ChunkID m_chunkPosition;
        public ChunkID ChunkPosition { get { return m_chunkPosition; } }

        private ChunkID m_lastChunkPosition;
        public ChunkID LastChunkPosition { get { return m_lastChunkPosition; } }
        
        private List<ChunkLODLoader> m_positiveDelta = new List<ChunkLODLoader>();
        public List<ChunkLODLoader> PositiveDelta { get { return m_positiveDelta; } }
        private List<ChunkLODLoader> m_negativeDelta = new List<ChunkLODLoader>();
        public List<ChunkLODLoader> NegativeDelta { get { return m_negativeDelta; } }

        private ChunkLODLoader[] m_chunks;

        private void Awake()
        {
            m_combinedDepth = m_lod0Depth + m_lod1Depth + m_lod2Depth + m_lod3Depth;
        }

        // Use this for initialization
        void Start()
        {
            m_streamer = GameObject.FindObjectOfType<WorldStreamer>();
            m_chunkPosition = ChunkID.FromVector3(new Vector3(-9999, -9999, -9999), m_streamer.ChunkSize);
            m_lastChunkPosition = ChunkID.FromVector3(new Vector3(-9999, -9999, -9999), m_streamer.ChunkSize);

            if (m_streamer == null)
            {
                Debug.LogError("Could not find a world streamer in the scene!");
                return;
            }

            CalculateAreaSize();
            m_streamer.AddAreaOfInterest(this);
        }

        private void OnDestroy()
        {
            if (m_streamer == null)
            {
                Debug.LogError("Could not find a world streamer in the scene!");
            }
            else
            {
                m_streamer.RemoveAreaOfInterest(this);
            }
        }

        /// <summary>
        /// Updates the chunk position of the area of interest. This means that it checks if 
        /// this area of interest has moved from one chunk to another.
        /// </summary>
        /// <returns>True if chunk position has changed, false otherwise.</returns>
        public bool UpdateChunkPosition()
        {
            m_lastChunkPosition = m_chunkPosition;
            m_chunkPosition = ChunkID.FromVector3(transform.position, m_streamer.ChunkSize);
            bool positionChanged = false;

            if (m_lastChunkPosition != m_chunkPosition)
            {
                positionChanged = true;
                m_negativeDelta.Clear();
                m_positiveDelta.Clear();
                Debug.Log(string.Format("Switched chunk from {0} to {1}", m_lastChunkPosition, m_chunkPosition));

                for (int i = 0; i < m_chunks.Length; i++)
                {
                    var posDelta = m_chunkPosition + m_chunks[i].ChunkID;

                    m_positiveDelta.Add(new ChunkLODLoader { ChunkID = posDelta, LodLevel = m_chunks[i].LodLevel } );
                }

                int numDeltas = 0;
                for (int i = 0; i < m_chunks.Length; i++)
                {
                    var negDelta = new ChunkLODLoader { ChunkID = m_lastChunkPosition + m_chunks[i].ChunkID, LodLevel = m_chunks[i].LodLevel };
                    if (!m_positiveDelta.Contains(negDelta))
                    {
                        m_negativeDelta.Add(negDelta);
                        numDeltas++;
                    }
                }
            }
            return positionChanged;
        }

        private void CalculateAreaSize()
        {
            ChunkID? current = new ChunkID(0, 0, 0);

            var visited = new HashSet<DepthNode>();

            var queue = new Queue<ChunkID?>();
            queue.Enqueue(current);
            queue.Enqueue(null);

            int depth = 0;
            while (queue.Count > 0 && depth < m_combinedDepth)
            {
                current = queue.Dequeue();

                if (!current.HasValue)
                {
                    depth++;
                    queue.Enqueue(null);
                    //  Double null means we've hit the end
                    if (queue.Peek() == null)
                        break;
                    else
                        continue;
                }

                var node = new DepthNode { ID = current.Value, Depth = depth };
                if (visited.Contains(node))
                {
                    continue;
                }
                    

                visited.Add(node);
                for(int j = 0; j < 7; j++)
                {
                    var neighbour = FindNeighbour(j, current);
                    var neighbourNode = new DepthNode { ID = neighbour, Depth = depth };
                    if (!visited.Contains(neighbourNode))
                    {
                        queue.Enqueue(neighbour);
                    }
                }
            }
            
            m_chunks = new ChunkLODLoader[visited.Count];
            int i = 0;
            foreach(var c in visited)
            {
                m_chunks[i] = new ChunkLODLoader { ChunkID = c.ID, LodLevel = GetLodLevelFromDepth(c.Depth) };
                i++;
            }
        }

        private int GetLodLevelFromDepth(int depth)
        {
            if (depth < m_lod0Depth)
                return 0;
            else if (depth < m_lod0Depth + m_lod1Depth)
                return 1;
            else if (depth < m_lod0Depth + m_lod1Depth + m_lod2Depth)
                return 2;
            else if (depth < m_lod0Depth + m_lod1Depth + m_lod2Depth + m_lod3Depth)
                return 3;
            return -1;
        }

        private ChunkID FindNeighbour(int index, ChunkID? parent)
        {
            int x = parent.Value.X;
            int y = parent.Value.Y;
            int z = parent.Value.Z;
            
            switch(index)
            {
                case 1:
                    x--;
                    break;
                case 2:
                    x++;
                    break;
                case 3:
                    y--;
                    break;
                case 4:
                    y++;
                    break;
                case 5:
                    z--;
                    break;
                case 6:
                    z++;
                    break;
            }

            return new ChunkID(x, y, z);
        }

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {

            if (!Application.isPlaying || m_chunks == null)
                return;

            Color color = Gizmos.color;
            float alpha = 0.1f;
            foreach(var c in m_chunks)
            {
                int currentLodLevel = c.LodLevel;
                Vector3 chunkPos = (c.ChunkID + m_chunkPosition).AsVector3();
                Vector3 chunkSize = m_streamer.ChunkSize;
                chunkPos.x *= chunkSize.x;
                chunkPos.y *= chunkSize.y;
                chunkPos.z *= chunkSize.z;
                chunkPos += chunkSize * 0.5f;

                if (c.ChunkID.Y > 0 || c.ChunkID.Y < 0)
                    continue;
                
                if (currentLodLevel == 0 && m_debugLod0)
                {
                    Gizmos.color = new Color(0.0f, 1.0f, 0.0f, alpha);
                    Gizmos.DrawCube(chunkPos, chunkSize);
                }

                else if (currentLodLevel == 1 && m_debugLod1)
                {
                    Gizmos.color = new Color(1.0f, 0.0f, 0.0f, alpha);
                    Gizmos.DrawCube(chunkPos, chunkSize);
                }

                else if (currentLodLevel == 2 && m_debugLod2)
                {
                    Gizmos.color = new Color(0.0f, 0.0f, 1.0f, alpha);
                    Gizmos.DrawCube(chunkPos, chunkSize);
                }

                else if (currentLodLevel == 3 && m_debugLod3)
                {
                    Gizmos.color = new Color(1.0f, 1.0f, 0.0f, alpha);
                    Gizmos.DrawCube(chunkPos, chunkSize);
                }
            }

            if (m_debugPositiveDelta)
            {
                foreach (var c in m_positiveDelta)
                {
                    Vector3 chunkPos = c.ChunkID.AsVector3();
                    Vector3 chunkSize = m_streamer.ChunkSize;
                    chunkPos.x *= chunkSize.x;
                    chunkPos.y *= chunkSize.y;
                    chunkPos.z *= chunkSize.z;
                    chunkPos += chunkSize * 0.5f;

                    if (c.ChunkID.Y > 0 || c.ChunkID.Y < 0)
                        continue;

                    Gizmos.color = new Color(1.0f, 0.0f, 1.0f, alpha);
                    Gizmos.DrawCube(chunkPos, chunkSize);
                }
            }

            if (m_debugNegativeDelta)
            {
                foreach (var c in m_negativeDelta)
                {
                    Vector3 chunkPos = c.ChunkID.AsVector3();
                    Vector3 chunkSize = m_streamer.ChunkSize;
                    chunkPos.x *= chunkSize.x;
                    chunkPos.y *= chunkSize.y;
                    chunkPos.z *= chunkSize.z;
                    chunkPos += chunkSize * 0.5f;

                    if (c.ChunkID.Y > 0 || c.ChunkID.Y < 0)
                        continue;

                    Gizmos.color = new Color(0.0f, 1.0f, 1.0f, alpha);
                    Gizmos.DrawCube(chunkPos, chunkSize);
                }
            }
            

            Gizmos.color = color;
        }

#endif
    }
}

