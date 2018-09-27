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
        private string[] m_assetBundles;
        public string[] AssetBundles { get { return m_assetBundles; } set { m_assetBundles = value; } }
    }
}


