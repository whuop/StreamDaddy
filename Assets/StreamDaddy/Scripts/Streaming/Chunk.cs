using StreamDaddy.AssetManagement;
using StreamDaddy.Chunking;
using StreamDaddy.Pooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Streaming
{
    public enum ChunkState
    {
        Unloaded = 0,
        Loading = 2,
        Loaded = 3
    }

    public class Chunk
    {
        private AssetChunkData m_chunkData;
        private ChunkState m_chunkState;
        public ChunkState State { get { return m_chunkState; } set { m_chunkState = value; } }

        private List<Renderable> m_renderers = new List<Renderable>();
        private List<GameObject> m_colliders = new List<GameObject>();

        private ChunkID m_chunkID;
        public ChunkID ID { get { return m_chunkID; } }
        
        public Chunk(AssetChunkData data)
        {
            m_chunkState = ChunkState.Unloaded;
            m_chunkData = data;
            m_chunkID = new ChunkID(data.ChunkID);
        }

        public IEnumerator LoadChunk(AssetManager assetManager)
        {
            if (m_chunkState != ChunkState.Unloaded)
                yield return null;

            m_chunkState = ChunkState.Loading;

            for(int i = 0; i < m_chunkData.MeshNames.Length; i++)
            {
                yield return new WaitForEndOfFrame();
                string meshName = m_chunkData.MeshNames[i];
                MaterialArray materialArray = m_chunkData.Materials[i];

                Vector3 position = m_chunkData.Positions[i];
                Vector3 rotation = m_chunkData.Rotations[i];
                Vector3 scale = m_chunkData.Scales[i];

                Mesh mesh = assetManager.GetMeshAsset(meshName);
                Material[] materials = new Material[materialArray.MaterialNames.Length];
                
                for(int j = 0; j < materialArray.MaterialNames.Length; j++)
                {
                    materials[j] = assetManager.GetMaterialAsset(materialArray.MaterialNames[j]);
                }

                Renderable renderer = GameObjectPool.GetRenderer(mesh, materials, position, rotation, scale);

                //  Add the renderable to the list of renderables that have been spawned in this chunk
                //  so that we can easily unload them later.
                m_renderers.Add(renderer);
            }

            m_chunkState = ChunkState.Loaded;
            yield return null;
        }

        public IEnumerable UnloadChunk()
        {
            for(int i = 0; i < m_renderers.Count; i++)
            {
                GameObjectPool.ReturnRenderer(m_renderers[i]);
            }

            m_renderers.Clear();
            m_chunkState = ChunkState.Unloaded;

            yield return null;
        }
    }
}

