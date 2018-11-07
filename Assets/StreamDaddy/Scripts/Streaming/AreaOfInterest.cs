using StreamDaddy.Chunking;
using StreamDaddy.Streaming;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Streaming
{
    public class AreaOfInterest : MonoBehaviour
    {
        /// <summary>
        /// The size of the area to be loaded.
        /// The size is in number of chunks, not in any other unit of measurement.
        /// </summary>
        [SerializeField]
        private int m_maxDepth;

        [SerializeField]
        private bool m_debugRender = true;
        
        private WorldStreamer m_streamer;

        private ChunkID m_chunkPosition;
        public ChunkID ChunkPosition { get { return m_chunkPosition; } }

        private ChunkID m_lastChunkPosition;
        public ChunkID LastChunkPosition { get { return m_lastChunkPosition; } }
        
        private List<ChunkID> m_positiveDelta = new List<ChunkID>();
        public List<ChunkID> PositiveDelta { get { return m_positiveDelta; } }
        private List<ChunkID> m_negativeDelta = new List<ChunkID>();
        public List<ChunkID> NegativeDelta { get { return m_negativeDelta; } }

        private ChunkID[] m_chunks;

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

        public void UpdateChunkPosition()
        {
            m_negativeDelta.Clear();
            m_positiveDelta.Clear();

            m_lastChunkPosition = m_chunkPosition;
            m_chunkPosition = ChunkID.FromVector3(transform.position, m_streamer.ChunkSize);
            Debug.Log("ChunkPos:" + m_chunkPosition.ToString());

            if (m_lastChunkPosition != m_chunkPosition)
            {
                Debug.Log(string.Format("Switched chunk from {0} to {1}", m_lastChunkPosition, m_chunkPosition));

                for (int i = 0; i < m_chunks.Length; i++)
                {
                    var posDelta = m_chunkPosition + m_chunks[i];

                    m_positiveDelta.Add(posDelta);
                }

                for (int i = 0; i < m_chunks.Length; i++)
                {
                    var negDelta = m_lastChunkPosition + m_chunks[i];
                    if (!m_positiveDelta.Contains(negDelta))
                    {
                        m_negativeDelta.Add(negDelta);
                    }
                }
            }
        }

        private void CalculateAreaSize()
        {
            ChunkID? current = new ChunkID(0, 0, 0);

            var visited = new HashSet<ChunkID?>();

            var queue = new Queue<ChunkID?>();
            queue.Enqueue(current);
            queue.Enqueue(null);

            int depth = 0;
            while (queue.Count > 0 && depth <= m_maxDepth)
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

                if (visited.Contains(current))
                    continue;

                visited.Add(current);
                for(int j = 0; j < 7; j++)
                {
                    var neighbour = FindNeighbour(j, current);
                    if (!visited.Contains(neighbour))
                    {
                        queue.Enqueue(neighbour);
                    }
                }
            }
            
            m_chunks = new ChunkID[visited.Count];
            int i = 0;
            foreach(var c in visited)
            {
                if (c.HasValue)
                {
                    m_chunks[i] = c.Value;
                    i++;
                }
            }
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
            if (!m_debugRender)
                return;

            if (!Application.isPlaying || m_chunks == null)
                return;

            Color color = Gizmos.color;
            Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 0.2f);
            
            foreach(var c in m_chunks)
            {
                Vector3 chunkPos = (c + m_chunkPosition).AsVector3();
                Vector3 chunkSize = m_streamer.ChunkSize;
                chunkPos.x *= chunkSize.x;
                chunkPos.y *= chunkSize.y;
                chunkPos.z *= chunkSize.z;
                chunkPos += chunkSize * 0.5f;

                Gizmos.DrawCube(chunkPos, chunkSize);
            }
            
            Gizmos.color = color;
        }

#endif
    }
}

