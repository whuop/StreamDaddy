﻿using StreamDaddy.AssetManagement;
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

    public class ChunkLOD
    {
        private LoadState m_loadState = LoadState.Unloaded;
        public LoadState State { get { return m_loadState; } }

        private List<Renderable> m_renderers = new List<Renderable>();
        private List<MeshCollideable> m_meshColliders = new List<MeshCollideable>();
        private List<BoxCollideable> m_boxColliders = new List<BoxCollideable>();
        private List<SphereCollideable> m_sphereColliders = new List<SphereCollideable>();

        private Terrain m_terrain;

        private AssetChunkData m_chunkData;
        private int m_lodLevel;
        public int LodLevel { get { return m_lodLevel; } }

        //  Delegates
        public delegate void OnFinishedDelegate(int lodLevel);
        
        public ChunkLOD(AssetChunkData chunkData, int lodLevel)
        {
            m_chunkData = chunkData;
            m_lodLevel = lodLevel;
        }

        public void SetTerrain(Terrain terrain)
        {
            m_terrain = terrain;
        }

        public IEnumerator Load(OnFinishedDelegate onFinished)
        {
            m_loadState = LoadState.Loading;

            if (m_terrain != null)
                m_terrain.gameObject.SetActive(true);

            if (m_chunkData != null)
            {
                //  Fetch the meshes for the LOD level to load
                var layer = m_chunkData.MeshLayers[m_lodLevel];
                for (int i = 0; i < layer.Meshes.Length; i++)
                {
                    yield return new WaitForEndOfFrame();
                    var meshData = layer.Meshes[i];
                    var materialsData = m_chunkData.MeshMaterials[i];
                    var transform = m_chunkData.MeshTransforms[i];

                    var mesh = AddressablesLoader.GetMesh(meshData.MeshReference.RuntimeKey);
                    Material[] materials = new Material[materialsData.MaterialReferences.Length];
                    for (int j = 0; j < materials.Length; j++)
                    {
                        materials[j] = AddressablesLoader.GetMaterial(materialsData.MaterialReferences[j].RuntimeKey);
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

                var colliderLayer = m_chunkData.MeshColliderLayers[m_lodLevel];
                for (int i = 0; i < colliderLayer.Meshes.Length; i++)
                {
                    yield return new WaitForEndOfFrame();
                    MeshData data = colliderLayer.Meshes[i];
                    TransformData transform = m_chunkData.MeshColliderTransforms[i];

                    Mesh mesh = AddressablesLoader.GetMesh(data.MeshReference.RuntimeKey);
                    MeshCollideable collideable = GameObjectPool.GetMeshCollider(transform.Position, transform.Rotation, transform.Scale, mesh);
                    m_meshColliders.Add(collideable);
                }
            }

            m_loadState = LoadState.Loaded;

            //  Call the on finished callback
            onFinished?.Invoke(m_lodLevel);

            yield return null;
        }

        public IEnumerator Unload(OnFinishedDelegate onFinished)
        {
            m_loadState = LoadState.Unloading;

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

            m_loadState = LoadState.Unloaded;

            //  Call the onfinished callback
            onFinished?.Invoke(m_lodLevel);

            yield return null;
        }
    }
    
    public class Chunk
    {
        public enum JobType
        {
            Load,
            Unload
        };

        public class LodJob
        {
            public int LodLevel;
            public JobType JobType;
        }

        /// <summary>
        /// Terrain associated with this chunk.
        /// Each chunk can have 1 terrain piece each.
        /// </summary>
        private Terrain m_terrain;

        private ChunkID m_chunkID;
        public ChunkID ID { get { return m_chunkID; } }

        private int m_lodLevels = 4;
        private ChunkLOD[] m_chunkLODs;
        
        private MonoBehaviour m_coroutineStarter = null;
        
        private ChunkLOD m_currentLoadedLOD = null;
        private ChunkLOD m_LODBeingLoaded = null;
        private Queue<ChunkLOD> m_lodJobQueue = new Queue<ChunkLOD>();

        public Chunk(AssetChunkData data, MonoBehaviour coroutineStarter)
        {
            m_chunkID = new ChunkID(data.ChunkID);
            m_coroutineStarter = coroutineStarter;

            m_chunkLODs = new ChunkLOD[m_lodLevels];
            //  Initialize Chunk LODS
            for(int i = 0; i < m_lodLevels; i++)
            {
                m_chunkLODs[i] = new ChunkLOD(data, i);
            }
        }

        public Chunk(ChunkID id)
        {
            m_chunkID = id;
        }

        public void SetTerrain(Terrain terrain)
        {
            m_terrain = terrain;
        }

        private void OnFinishedChunkWork(int lodLevel)
        {
            var lod = m_chunkLODs[lodLevel];

            switch(lod.State)
            {
                case LoadState.Loaded:

                    var unloadLod = m_currentLoadedLOD;
                    m_LODBeingLoaded = null;
                    m_currentLoadedLOD = lod;

                    if (unloadLod != null)
                        m_lodJobQueue.Enqueue(unloadLod);

                    break;
                case LoadState.Unloaded:

                    if (m_lodJobQueue.Count == 0)
                    {
                        m_currentLoadedLOD = null;
                    }

                    break;
                case LoadState.Loading:
                case LoadState.Unloading:
                    Debug.LogError("Chunk should never be in Loading or Unloading state when OnFinished is reached!");
                    break;
            }

            CheckLODJobQueue();
        }

        private void CheckLODJobQueue()
        {
            if (m_lodJobQueue.Count == 0)
                return;
            var nextJob = m_lodJobQueue.Peek();

            if (nextJob == null)
            {
                Debug.LogError(string.Format("NextJob is null! Chunk {0}", m_chunkID.ToString()));
            }

            //  If it is in between loaded or unloaded state, then do nothing. let the OnFinishedChunkWork callback handle calling this
            // again when it is done.
            if (nextJob.State == LoadState.Loading || nextJob.State == LoadState.Unloading)
            {
                Debug.LogError(string.Format("Chunk {0} is in state {1}", m_chunkID.ToString(), nextJob.State.ToString()));
                return;
            }
                

            //  remove the job from the queue
            m_lodJobQueue.Dequeue();

           switch(nextJob.State)
            {
                case LoadState.Loaded:
                    Debug.LogError(string.Format("Unloading chunk {0}", m_chunkID.ToString()));
                    m_coroutineStarter.StartCoroutine(nextJob.Unload(OnFinishedChunkWork));
                    break;
                case LoadState.Unloaded:
                    Debug.LogError(string.Format("Loading chunk {0}", m_chunkID.ToString()));
                    m_LODBeingLoaded = nextJob;
                    m_coroutineStarter.StartCoroutine(nextJob.Load(OnFinishedChunkWork));
                    break;
            }
        }

        public void LoadChunk(int lodLevel)
        {
            //  If there aren't any chunk LODs, then just early return
            if (m_chunkLODs == null)
                return;

            var lod = m_chunkLODs[lodLevel];
            //  If the LOD has already begun loading or has finished loading then do nothing
            if (lod.State == LoadState.Loaded || lod.State == LoadState.Loading)
            {
                Debug.LogError("Is already loaded or in loading!");
                return;
            }
            
            m_lodJobQueue.Enqueue(lod);
            CheckLODJobQueue();
        }

        /// <summary>
        /// Unloads the currently loaded LOD level for this chunk.
        /// If there isn't one loaded, nothing happens.
        /// </summary>
        public void UnloadChunk()
        {
            if (m_currentLoadedLOD != null)
                m_lodJobQueue.Enqueue(m_currentLoadedLOD);
            else if (m_LODBeingLoaded != null)
                m_lodJobQueue.Enqueue(m_LODBeingLoaded);
            CheckLODJobQueue();
        }
    }
}

