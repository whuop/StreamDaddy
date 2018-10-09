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

        /// <summary>
        /// The interval at which the position of the area of interest should be checked.
        /// When AreaOfInterest changes chunk position new chunks are loaded, and old unseen chunks are unloaded.
        /// </summary>
        [SerializeField]
        private float m_positionCheckTime = 0.2f;

        private WorldStreamer m_streamer;

        private ChunkID m_chunkPosition;
        public ChunkID ChunkPosition { get { return m_chunkPosition; } }

        private Coroutine m_updateRoutine;

        // Use this for initialization
        void Start()
        {
            m_streamer = GameObject.FindObjectOfType<WorldStreamer>();
            m_chunkPosition = ChunkID.FromVector3(transform.position, m_streamer.ChunkSize);

            if (m_streamer == null)
            {
                Debug.LogError("Could not find a world streamer in the scene!");
                return;
            }

            m_streamer.AddAreaOfInterest(this);

            m_updateRoutine = StartCoroutine(UpdateChunkPosition());
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

            StopCoroutine(m_updateRoutine);
            m_updateRoutine = null;
        }

        private IEnumerator UpdateChunkPosition()
        {
            while(true)
            {
                m_chunkPosition = ChunkID.FromVector3(transform.position, m_streamer.ChunkSize);
                yield return new WaitForSeconds(m_positionCheckTime);
            }
        }
    }
}

