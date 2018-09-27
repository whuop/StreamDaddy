using StreamDaddy.Chunking;
using StreamDaddy.Editor.Assets;
using StreamDaddy.Editor.Utils;
using StreamDaddy.Streaming;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StreamDaddy.Editor.Chunking
{
    public class EditorChunkManager
    {
        private Dictionary<ChunkID, EditorChunk> m_chunks = new Dictionary<ChunkID, EditorChunk>();
        private Vector3Int m_chunkSize;

        private IAssetBuildStrategy m_buildStrategy = new AssetBuildStrategy();

        private WorldStream m_world = null;
        private string m_worldName = string.Empty;

        public EditorChunkManager()
        {
        }

        public void SetChunkSizeAndClearManager(Vector3Int newChunkSize)
        {
            m_chunkSize = newChunkSize;
            m_chunks.Clear();
        }

        public void AddGameObject(GameObject go)
        {
            //  Round to approximate chunk position
            float x = go.transform.position.x / (float)m_chunkSize.x;
            float y = go.transform.position.y / (float)m_chunkSize.y;
            float z = go.transform.position.z / (float)m_chunkSize.z;
            
            //  Floor to chunk position ID ( chunk index in EditorChunkManager )
            int cx = (int)Mathf.Floor(x);
            int cy = (int)Mathf.Floor(y);
            int cz = (int)Mathf.Floor(z);
            
            ChunkID chunkKey = new ChunkID((int)cx, (int)cy, (int)cz);
            //  Create a new chunk if no chunk exists with the given key
            if (!m_chunks.ContainsKey(chunkKey))
            {
                m_chunks.Add(chunkKey, new EditorChunk(chunkKey, m_chunkSize));
            }
            
            m_chunks[chunkKey].AddChild(go);
        }

        public void BeginWorld(string worldName)
        {
            m_world = ScriptableObject.CreateInstance<WorldStream>();
            m_worldName = worldName;
        }

        public void EndWorld()
        {
            ScriptableObjectUtils.CreateOrReplaceAsset<WorldStream>(m_world,
                                        EditorPaths.GetWorldStreamsFolder() + m_worldName + ".asset");
            m_world = null;
            m_worldName = string.Empty;

            string bundlePath = EditorPaths.STREAMING_DIRECTORY_PATH;
            BuildPipeline.BuildAssetBundles(bundlePath, BuildAssetBundleOptions.ChunkBasedCompression |
                                                        BuildAssetBundleOptions.DisableLoadAssetByFileName |
                                                        BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension |
                                                        BuildAssetBundleOptions.DisableWriteTypeTree,
                                                        BuildTarget.StandaloneWindows64);
        }

        public void ExportAllChunkAssets()
        {
            List<string> assetBundles = new List<string>();
            foreach(var kvp in m_chunks)
            {
                 m_buildStrategy.BuildChunkAssets(m_worldName, kvp.Value, assetBundles);
            }

            m_world.AssetBundles = assetBundles.ToArray();
        }

        public void ExportAllChunkLayouts()
        {
            foreach(var kvp in m_chunks)
            {
                m_buildStrategy.BuildChunkLayout(m_worldName, kvp.Value);
            }

            m_world.ChunkLayoutBundle = m_worldName + "_chunklayout";
        }

        public EditorChunk GetChunk(ChunkID id)
        {
            return m_chunks[id];
        }

        public EditorChunk[] GetAllChunks()
        {
            EditorChunk[] chunks = new EditorChunk[m_chunks.Values.Count];
            m_chunks.Values.CopyTo(chunks, 0);
            return chunks;
        }

        public void Draw()
        {
            foreach(var kvp in m_chunks)
            {
                kvp.Value.Draw();
            }
        }
    }
}


