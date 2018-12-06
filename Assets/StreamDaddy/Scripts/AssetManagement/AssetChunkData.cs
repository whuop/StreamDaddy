using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StreamDaddy.AssetManagement
{
    public class AssetChunkData : ScriptableObject
    {
        /// <summary>
        /// Renderable meshes 
        /// </summary>
        public MeshLayerData[] MeshLayers;
        public MaterialData[] MeshMaterials;
        public TransformData[] MeshTransforms;

        /// <summary>
        /// All the different Terrain LODs.
        /// </summary>
        public TerrainMeshData[] TerrainMeshes;

        /// <summary>
        /// Colliders
        /// </summary>
        public BoxColliderData[] BoxColliders;
        public SphereColliderData[] SphereColliders;

        /// <summary>
        /// Mesh Colliders, these are special compared to the other colliders as they
        /// have a mesh. Which means we might want to LOD them as well.
        /// </summary>
        public MeshLayerData[] MeshColliderLayers;
        public TransformData[] MeshColliderTransforms;
        
        /// <summary>
        /// Where the Chunk is located in the world.
        /// </summary>
        public Vector3Int ChunkID;
    }

    [System.Serializable]
    public class MeshLayerData
    {
        public MeshData[] Meshes;
    }

    [System.Serializable]
    public class TransformData
    {
        [SerializeField]
        public Vector3 Position;
        [SerializeField]
        public Vector3 Rotation;
        [SerializeField]
        public Vector3 Scale;
    }

    [System.Serializable]
    public class MeshData
    {
        [SerializeField]
        public AssetReference MeshReference;
    }

    [System.Serializable]
    public class MaterialData
    {
        [SerializeField]
        public AssetReference[] MaterialReferences;
    }

    [System.Serializable]
    public class BoxColliderData : TransformData
    {
        [SerializeField]
        public Vector3 Center;
        [SerializeField]
        public Vector3 Size;
    }

    [System.Serializable]
    public class SphereColliderData : TransformData
    {
        [SerializeField]
        public Vector3 Center;
        [SerializeField]
        public float Radius;
    }

    [System.Serializable]
    public class TerrainMeshData
    {
        [SerializeField]
        public AssetReference MeshReference;
        [SerializeField]
        public Vector3 Position;
    }
}


