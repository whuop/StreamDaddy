using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.AssetManagement
{
    public class AssetChunkData : ScriptableObject
    {
        public Vector3[] Positions;
        public Vector3[] Rotations;
        public Vector3[] Scales;
        public string[] MeshNames;
        public MaterialArray[] Materials;
        public Vector3Int ChunkID;
    }

    [System.Serializable]
    public class MaterialArray
    {
        [SerializeField]
        public string[] MaterialNames;
    }
}


