using StreamDaddy.AssetManagement;
using StreamDaddy.Pooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Streaming
{
    public class ChunkLOD
    {
        private LoadState m_loadState = LoadState.Unloaded;
        public LoadState State { get { return m_loadState; } }

        private List<Renderable> m_renderers = new List<Renderable>();
        private List<MeshCollideable> m_meshColliders = new List<MeshCollideable>();
        private List<BoxCollideable> m_boxColliders = new List<BoxCollideable>();
        private List<SphereCollideable> m_sphereColliders = new List<SphereCollideable>();

        private Terrain m_terrain;
        private Mesh m_terrainMesh;

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

        public void SetTerrainMesh(Mesh terrainMesh)
        {
            m_terrainMesh = terrainMesh;
        }

        public IEnumerator Load(OnFinishedDelegate onFinished)
        {
            m_loadState = LoadState.Loading;

            //  If this has a terrain then activate it
            if (m_terrain != null)
                m_terrain.gameObject.SetActive(true);
            //  If instead this has a terrain mesh then activate that instead.
            if (m_terrainMesh != null)
            {
                //GameObjectPool.GetRenderer(m_terrain, )
            }

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

            //  If this LOD has a terrain then deactivate that
            if (m_terrain != null)
                m_terrain.gameObject.SetActive(false);
            //  If instead it has a terrain mesh, then return that collider and renderer to the game object pool
            else if (m_terrainMesh != null)
            {

            }

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
}