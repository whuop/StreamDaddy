using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.AssetManagement
{
    public class AssetChunkData : ScriptableObject
    {
        public MeshData[] Meshes;
        public BoxColliderData[] BoxColliders;
        public SphereColliderData[] SphereColliders;
        public MeshColliderData[] MeshColliders;

        public Vector3Int ChunkID;
    }

    [System.Serializable]
    public class PositionData
    {
        [SerializeField]
        public Vector3 Position;
        [SerializeField]
        public Vector3 Rotation;
        [SerializeField]
        public Vector3 Scale;
    }

    [System.Serializable]
    public class MeshData : PositionData
    {
        [SerializeField]
        public string MeshAddress;
        [SerializeField]
        public string[] MaterialAddresses;
    }

    [System.Serializable]
    public class BoxColliderData : PositionData
    {
        [SerializeField]
        public Vector3 Center;
        [SerializeField]
        public Vector3 Size;
    }

    [System.Serializable]
    public class SphereColliderData : PositionData
    {
        [SerializeField]
        public Vector3 Center;
        [SerializeField]
        public float Radius;
    }

    [System.Serializable]
    public class MeshColliderData : PositionData
    {
        [SerializeField]
        public string MeshAddress;
    }
}


