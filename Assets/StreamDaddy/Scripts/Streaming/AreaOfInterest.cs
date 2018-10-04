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

        // Use this for initialization
        void Start()
        {
            m_streamer = GameObject.FindObjectOfType<WorldStreamer>();

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
                return;
            }

            m_streamer.RemoveAreaOfInterest(this);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

