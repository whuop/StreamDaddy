using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.AssetManagement
{
    public class AssetsTransforms : ScriptableObject
    {
        public Vector3[] Positions;
        public Vector3[] Rotations;
        public Vector3[] Scales;
        public string[] PrefabNames;
    }
}


