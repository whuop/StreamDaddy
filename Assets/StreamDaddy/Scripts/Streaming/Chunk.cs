using StreamDaddy.AssetManagement;
using StreamDaddy.Chunking;
using StreamDaddy.Pooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Streaming
{
    public enum LoadState
    {
        Unloaded = 0,
        Unloading = 1,
        Loading = 2,
        Loaded = 3
    }

    public class LoadedObjects
    {
        private List<Renderable> m_renderers = new List<Renderable>();
        public List<Renderable> Renderers { get { return m_renderers; } }

        private List<MeshCollideable> m_meshColliders = new List<MeshCollideable>();
        public List<MeshCollideable> MeshColliders { get { return m_meshColliders; } }

        private List<BoxCollideable> m_boxColliders = new List<BoxCollideable>();
        public List<BoxCollideable> BoxColliders { get { return m_boxColliders; } }

        private List<SphereCollideable> m_sphereColliders = new List<SphereCollideable>();
        public List<SphereCollideable> SphereColliders { get { return m_sphereColliders; } }

        public void Clear()
        {
            m_renderers.Clear();
            m_meshColliders.Clear();
            m_boxColliders.Clear();
            m_sphereColliders.Clear();
        }
    }

    public class Chunk
    {
        private AssetChunkData m_chunkData;

        private LoadState[] m_loadstates = new LoadState[]
        {
            LoadState.Unloaded,
            LoadState.Unloaded,
            LoadState.Unloaded,
            LoadState.Unloaded
        };
        
        private int m_currentLodLevel = 0;
        public int CurrentLodLevel { get { return m_currentLodLevel; } }

        private LoadedObjects[] m_loadedLODObjects = new LoadedObjects[]
        {
            new LoadedObjects(),
            new LoadedObjects(),
            new LoadedObjects(),
            new LoadedObjects()
        };
        
        /// <summary>
        /// Terrain associated with this chunk.
        /// Each chunk can have 1 terrain piece each.
        /// </summary>
        private Terrain m_terrain;

        private ChunkID m_chunkID;
        public ChunkID ID { get { return m_chunkID; } }
        
        
        public Chunk(AssetChunkData data)
        {
            m_chunkData = data;
            m_chunkID = new ChunkID(data.ChunkID);
        }

        public Chunk(ChunkID id)
        {
            m_chunkID = id;
        }

        public LoadState GetLoadState(int lodLevel)
        {
            return m_loadstates[lodLevel];
        }

        private void SetLoadState(int lodLevel, LoadState state)
        {
            m_loadstates[lodLevel] = state;
        }

        public IEnumerator LoadChunk(int lodLevel)
        {
            SetLoadState(lodLevel, LoadState.Loading);

            if (m_terrain != null)
                m_terrain.gameObject.SetActive(true);

            //  Get the container for this lod level
            LoadedObjects objectContainer = m_loadedLODObjects[lodLevel];

            if (m_chunkData != null)
            {
                //  Fetch the meshes for the LOD level to load
                var layer = m_chunkData.MeshLayers[lodLevel];
                for(int i = 0; i < layer.Meshes.Length; i++)
                {
                    yield return new WaitForEndOfFrame();
                    var meshData = layer.Meshes[i];
                    var materialsData = m_chunkData.MeshMaterials[i];
                    var transform = m_chunkData.MeshTransforms[i];

                    var mesh = AddressablesLoader.GetMesh(meshData.MeshReference.RuntimeKey);
                    Material[] materials = new Material[materialsData.MaterialReferences.Length];
                    for(int j = 0; j < materials.Length; j++)
                    {
                        materials[j] = AddressablesLoader.GetMaterial(materialsData.MaterialReferences[j].RuntimeKey);
                    }

                    Renderable renderer = GameObjectPool.GetRenderer(mesh, materials, transform.Position, transform.Rotation, transform.Scale);
                    objectContainer.Renderers.Add(renderer);
                }

                for (int i = 0; i < m_chunkData.BoxColliders.Length; i++)
                {
                    yield return new WaitForEndOfFrame();

                    BoxColliderData data = m_chunkData.BoxColliders[i];
                    BoxCollideable collideable = GameObjectPool.GetBoxCollider(data.Position, data.Rotation, data.Scale, data.Center, data.Size);

                    objectContainer.BoxColliders.Add(collideable);
                }

                for (int i = 0; i < m_chunkData.SphereColliders.Length; i++)
                {
                    yield return new WaitForEndOfFrame();

                    SphereColliderData data = m_chunkData.SphereColliders[i];
                    SphereCollideable collideable = GameObjectPool.GetSphereCollider(data.Position, data.Rotation, data.Scale, data.Center, data.Radius);
                    objectContainer.SphereColliders.Add(collideable);
                }

                var colliderLayer = m_chunkData.MeshColliderLayers[lodLevel];
                for(int i = 0; i < colliderLayer.Meshes.Length; i++)
                {
                    yield return new WaitForEndOfFrame();
                    MeshData data = colliderLayer.Meshes[i];
                    TransformData transform = m_chunkData.MeshColliderTransforms[i];

                    Mesh mesh = AddressablesLoader.GetMesh(data.MeshReference.RuntimeKey);
                    MeshCollideable collideable = GameObjectPool.GetMeshCollider(transform.Position, transform.Rotation, transform.Scale, mesh);
                    objectContainer.MeshColliders.Add(collideable);
                }
            }

            SetLoadState(lodLevel, LoadState.Loaded);
            yield return null;
        }

        public void SetTerrain(Terrain terrain)
        {
            m_terrain = terrain;
        }

        public IEnumerator UnloadChunk(int lodLevel)
        {
            SetLoadState(lodLevel, LoadState.Unloading);

            LoadedObjects objectContainer = m_loadedLODObjects[lodLevel];

            if (m_chunkData != null)
            {
                for (int i = 0; i < objectContainer.Renderers.Count; i++)
                {
                    GameObjectPool.ReturnRenderer(objectContainer.Renderers[i]);
                }

                for (int i = 0; i < objectContainer.BoxColliders.Count; i++)
                {
                    GameObjectPool.ReturnBoxCollideable(objectContainer.BoxColliders[i]);
                }
                
                for (int i = 0; i < objectContainer.SphereColliders.Count; i++)
                {
                    GameObjectPool.ReturnSphereCollideable(objectContainer.SphereColliders[i]);
                }

                for (int i = 0; i < objectContainer.MeshColliders.Count; i++)
                {
                    GameObjectPool.ReturnMeshCollider(objectContainer.MeshColliders[i]);
                }
            }

            if (m_terrain != null)
                m_terrain.gameObject.SetActive(false);

            objectContainer.Clear();

            SetLoadState(lodLevel, LoadState.Loaded);

            yield return null;
        }
    }
}

