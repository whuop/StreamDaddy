using StreamDaddy.Editor.Configs.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Editor.Configs
{
    [System.Serializable]
    [ConfigPath("Assets/StreamDaddy/Configs/", "StreamDaddyEditorConfig.asset")]
    public class StreamDaddyConfig : ConfigBase
    {
        [SerializeField]
        private Vector3Int m_chunkSize;
        public Vector3Int ChunkSize { get { return m_chunkSize; } set { m_chunkSize = value; } }
        
        private StreamDaddyConfig() : base()
        {
        }
    }
}


