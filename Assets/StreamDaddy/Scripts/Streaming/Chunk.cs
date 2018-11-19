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

        public IEnumerator LoadChunk(int lodLevel)
        {
            m_chunkState = ChunkState.Loading;

            if (m_terrain != null)
                m_terrain.gameObject.SetActive(true);

            if (m_chunkData != null)
            {
                //  Fetch the meshes for the LOD level to load
                var layer = m_chunkData.MeshLayers[lodLevel];
                for(int i = 0; i < layer.Meshes.Length; i++)
                {
                    yield return new WaitForEndOfFrame();
                    var meshData = layer.Meshes[i];
                    var transform = m_chunkData.MeshTransforms[i];

                    var mesh = AddressablesLoader.GetMesh(meshData.MeshReference.RuntimeKey);
                    Material[] materials = new Material[meshData.MaterialReferences.Length];
                    for(int j = 0; j < meshData.MaterialReferences.Length; j++)
                    {
                        materials[j] = AddressablesLoader.GetMaterial(meshData.MaterialReferences[j].RuntimeKey);
                    }

                    Renderable renderer = GameObjectPool.GetRenderer(mesh, materials, transform.Position, transform.Rotation, transform.Scale);
                    m_renderers.Add(renderer);
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

                var colliderLayer = m_chunkData.MeshColliderLayers[lodLevel];
                for(int i = 0; i < colliderLayer.MeshColliders.Length; i++)
                {
                    yield return new WaitForEndOfFrame();
                    MeshColliderData data = colliderLayer.MeshColliders[i];
                    TransformData transform = m_chunkData.MeshColliderTransforms[i];

                    Mesh mesh = AddressablesLoader.GetMesh(data.MeshReference.RuntimeKey);
                    MeshCollideable collideable = GameObjectPool.GetMeshCollider(transform.Position, transform.Rotation, transform.Scale, mesh);
                    m_meshColliders.Add(collideable);
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

