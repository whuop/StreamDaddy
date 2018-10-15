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
        private Vector3Int m_areaSize;
        
        private WorldStreamer m_streamer;

        private ChunkID m_chunkPosition;
        public ChunkID ChunkPosition { get { return m_chunkPosition; } }

        private ChunkID m_lastChunkPosition;
        public ChunkID LastChunkPosition { get { return m_lastChunkPosition; } }
        
        private List<ChunkID> m_positiveDelta = new List<ChunkID>();
        public List<ChunkID> PositiveDelta { get { return m_positiveDelta; } }
        private List<ChunkID> m_negativeDelta = new List<ChunkID>();
        public List<ChunkID> NegativeDelta { get { return m_negativeDelta; } }

        // Use this for initialization
        void Start()
        {
            m_streamer = GameObject.FindObjectOfType<WorldStreamer>();
            m_chunkPosition = ChunkID.FromVector3(transform.position, m_streamer.ChunkSize);
            m_lastChunkPosition = ChunkID.FromVector3(transform.position, m_streamer.ChunkSize);

            if (m_streamer == null)
            {
                Debug.LogError("Could not find a world streamer in the scene!");
                return;
            }

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

            if (m_lastChunkPosition != m_chunkPosition)
            {
                Debug.Log(string.Format("Switched chunk from {0} to {1}", m_lastChunkPosition, m_chunkPosition));

                m_negativeDelta.Add(m_lastChunkPosition);
                m_positiveDelta.Add(m_chunkPosition);
            }
        }

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
                return;
            Color color = Gizmos.color;

            Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 0.2f);

            Vector3 chunkPos = m_chunkPosition.ID;
            Vector3 chunkSize = m_streamer.ChunkSize;
            chunkPos.x *= chunkSize.x;
            chunkPos.y *= chunkSize.y;
            chunkPos.z *= chunkSize.z;
            chunkPos += chunkSize * 0.5f;
            
            Gizmos.DrawCube(chunkPos, chunkSize);
            
            Gizmos.color = color;
        }

#endif
    }
}

