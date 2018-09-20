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

        public static RenderableAsset CreateRenderableAssets(string scriptableObjectName, Vector3[] positions, Vector3[] rotations, Vector3[] scales, string[] meshNames, string[][] materialNames)
        {
            RenderableAsset asset = ScriptableObject.CreateInstance<RenderableAsset>();
            asset.Positions = positions;
            asset.Rotations = rotations;
            asset.Scales = scales;
            asset.MeshNames = meshNames;

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

