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
        Unloading = 1,
        Loading = 2,
        Loaded = 3
    }

    public class Chunk
    {
        private AssetChunkData m_chunkData;
        private ChunkState m_chunkState;
        public ChunkState State { get { return m_chunkState; } set { m_chunkState = value; } }

        private List<Renderable> m_renderers = new List<Renderable>();
        private List<MeshCollideable> m_meshColliders = new List<MeshCollideable>();
        private List<BoxCollideable> m_boxColliders = new List<BoxCollideable>();
        private List<SphereCollideable> m_sphereColliders = new List<SphereCollideable>();

        /// <summary>
        /// Terrain associated with this chunk.
        /// Each chunk can have 1 terrain piece each.
        /// </summary>
        private Terrain m_terrain;

        private ChunkID m_chunkID;
        public ChunkID ID { get { return m_chunkID; } }
        
        public Chunk(AssetChunkData data)
        {
            m_chunkState = ChunkState.Unloaded;
            m_chunkData = data;
            m_chunkID = new ChunkID(data.ChunkID);
        }

        public Chunk(ChunkID id)
        {
            m_chunkState = ChunkState.Unloaded;
            m_chunkID = id;
        }

        public IEnumerator LoadChunk()
        {
            m_chunkState = ChunkState.Loading;

            if (m_terrain != null)
                m_terrain.gameObject.SetActive(true);

            if (m_chunkData != null)
            {
                for (int i = 0; i < m_chunkData.Meshes.Length; i++)
                {
                    yield return new WaitForEndOfFrame();

                    MeshData meshdata = m_chunkData.Meshes[i];

                    // TODO: Change to adresses
                    //string meshName = meshdata.MeshName;
                    //string[] materialNames = meshdata.MaterialNames;

                    Vector3 position = meshdata.Position;
                    Vector3 rotation = meshdata.Rotation;
                    Vector3 scale = meshdata.Scale;

                    /*Mesh mesh = assetManager.GetMeshAsset(meshName);
                    Material[] materials = new Material[materialNames.Length];

                    for (int j = 0; j < materialNames.Length; j++)
                    {
                        materials[j] = assetManager.GetMaterialAsset(materialNames[j]);
                    }

                    Renderable renderer = GameObjectPool.GetRenderer(mesh, materials, position, rotation, scale);
                    */


                    //  Add the renderable to the list of renderables that have been spawned in this chunk
                    //  so that we can easily unload them later.
                    // m_renderers.Add(renderer);
                }

                for (int i = 0; i < m_chunkData.BoxColliders.Length; i++)
                {
                    yield return new WaitForEndOfFrame();

                    BoxColliderData data = m_chunkData.BoxColliders[i];
                    BoxCollideable collideable = GameObjectPool.GetBoxCollider(data.Position, data.Rotation, data.Scale, data.Center, data.Size);

                    m_boxColliders.Add(collideable);
                }

                for (int i = 0; i < m_chunkData.SphereColliders.Length; i++)
                {
                    yield return new WaitForEndOfFrame();

                    SphereColliderData data = m_chunkData.SphereColliders[i];
                    SphereCollideable collideable = GameObjectPool.GetSphereCollider(data.Position, data.Rotation, data.Scale, data.Center, data.Radius);
                    m_sphereColliders.Add(collideable);
                }

                for (int i = 0; i < m_chunkData.MeshColliders.Length; i++)
                {
                    yield return new WaitForEndOfFrame();
                    MeshColliderData data = m_chunkData.MeshColliders[i];

                    //string meshName = data.MeshName;
                    Vector3 position = data.Position;
                    Vector3 rotation = data.Rotation;
                    Vector3 scale = data.Scale;

                    //Mesh mesh = assetManager.GetMeshAsset(meshName);
                    //MeshCollideable collideable = GameObjectPool.GetMeshCollider(position, rotation, scale, mesh);
                    //m_meshColliders.Add(collideable);
                }
            }
            
            m_chunkState = ChunkState.Loaded;
            yield return null;
        }

        public void SetTerrain(Terrain terrain)
        {
            m_terrain = terrain;
        }

        public IEnumerator UnloadChunk()
        {
            if (m_chunkData != null)
            {
                for (int i = 0; i < m_renderers.Count; i++)
                {
                    GameObjectPool.ReturnRenderer(m_renderers[i]);
                }

                for (int i = 0; i < m_boxColliders.Count; i++)
                {
                    GameObjectPool.ReturnBoxCollideable(m_boxColliders[i]);
                }

                for (int i = 0; i < m_sphereColliders.Count; i++)
                {
                    GameObjectPool.ReturnSphereCollideable(m_sphereColliders[i]);
                }

                for (int i = 0; i < m_meshColliders.Count; i++)
                {
                    GameObjectPool.ReturnMeshCollider(m_meshColliders[i]);
                }
            }

            if (m_terrain != null)
                m_terrain.gameObject.SetActive(false);

            m_renderers.Clear();
            m_boxColliders.Clear();
            m_sphereColliders.Clear();
            m_meshColliders.Clear();
            m_chunkState = ChunkState.Unloaded;

            yield return null;
        }
    }
}

