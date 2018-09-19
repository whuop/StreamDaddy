using StreamDaddy.AssetManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StreamDaddy.Editor
{
    public class AssetBundleUtils : MonoBehaviour
    {
        private static string TEMP_PATH = "Assets/StreamDaddy/Temp/";
        private static string CHUNK_DATA_PATH = Path.Combine(TEMP_PATH, "ChunkData/");

        public static AssetsTransforms CreateAssetsTransforms(string scriptableObjectName, Vector3[] positions, Vector3[] rotations, Vector3[] scales, string[] prefabNames)
        {
            AssetsTransforms asset = ScriptableObject.CreateInstance<AssetsTransforms>();
            asset.Positions = positions;
            asset.Rotations = rotations;
            asset.Scales = scales;
            asset.PrefabNames = prefabNames;
            
            AssetDatabase.CreateAsset(asset, CHUNK_DATA_PATH + scriptableObjectName + ".asset");

            EditorUtility.SetDirty(asset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return asset;
        }
    }
}

