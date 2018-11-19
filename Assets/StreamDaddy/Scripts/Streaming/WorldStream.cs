using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace StreamDaddy.Streaming
{
    public class WorldStream : ScriptableObject
    {
        [SerializeField]
        private string m_worldName;
        public string WorldName { get { return m_worldName; } set { m_worldName = value; } }

        [SerializeField]
        private Vector3Int m_chunkSize;
        public Vector3Int ChunkSize { get { return m_chunkSize; } set { m_chunkSize = value; } }

        [SerializeField]
        private string m_chunkLayoutBundle;
        public string ChunkLayoutBundle { get { return m_chunkLayoutBundle; } set { m_chunkLayoutBundle = value; } }
        
        [SerializeField]
        private List<AssetReference> m_chunkLayoutReferences;
        public List<AssetReference> ChunkLayoutReferences { get { return m_chunkLayoutReferences; } set { m_chunkLayoutReferences = value; } }
        
        [SerializeField]
        private string[] m_assetBundles;
        public string[] AssetBundles { get { return m_assetBundles; } set { m_assetBundles = value; } }
    }
}


