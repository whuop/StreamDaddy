using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Streaming
{
    public class WorldStream : ScriptableObject
    {
        [SerializeField]
        private string m_chunkLayoutBundle;
        public string ChunkLayoutBundle { get { return m_chunkLayoutBundle; } set { m_chunkLayoutBundle = value; } }

        [SerializeField]
        public string[] m_chunkNames;
        public string[] ChunkNames { get { return m_chunkNames; } set { m_chunkNames = value; } }

        [SerializeField]
        public Vector3Int m_chunkSize;
        public Vector3Int ChunkSize { get { return m_chunkSize; } set { m_chunkSize = value; } }

        [SerializeField]
        private string[] m_assetBundles;
        public string[] AssetBundles { get { return m_assetBundles; } set { m_assetBundles = value; } }
    }
}


