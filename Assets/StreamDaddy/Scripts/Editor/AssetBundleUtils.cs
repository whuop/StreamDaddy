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

        public static PrefabChunkData CreatePrefabChunkData(string scriptableObjectName, Vector3[] positions, Vector3[] rotations, Vector3[] scales, string[] prefabNames)
        {
            PrefabChunkData asset = ScriptableObject.CreateInstance<PrefabChunkData>();
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

        public static AssetChunkData CreateRenderableAssets(string scriptableObjectName, Vector3[] positions, Vector3[] rotations, Vector3[] scales, string[] meshNames, string[][] materialNames, Vector3Int chunkID)
        {
            AssetChunkData asset = ScriptableObject.CreateInstance<AssetChunkData>();
            asset.Positions = positions;
            asset.Rotations = rotations;
            asset.Scales = scales;
            asset.MeshNames = meshNames;
            asset.ChunkID = chunkID;

            MaterialArray[] materials = new MaterialArray[materialNames.Length];
            for(int i = 0; i < materials.Length; i++)
            {
                materials[i] = new MaterialArray();
                materials[i].MaterialNames = materialNames[i];
            }

            asset.Materials = materials;

            AssetDatabase.CreateAsset(asset, CHUNK_DATA_PATH + scriptableObjectName + ".asset");

            EditorUtility.SetDirty(asset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return asset;
        }
    }
}

