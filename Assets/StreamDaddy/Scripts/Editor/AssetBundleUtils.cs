using StreamDaddy.AssetManagement;
using StreamDaddy.Editor.Utils;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace StreamDaddy.Editor
{
    public class AssetBundleUtils : MonoBehaviour
    {
        /*public static PrefabChunkData CreatePrefabChunkData(string scriptableObjectName, Vector3[] positions, Vector3[] rotations, Vector3[] scales, string[] prefabNames)
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
        }*/

        public static AssetChunkData CreateChunkLayoutData(string worldName, string scriptableObjectName, MeshData[] meshes, BoxColliderData[] boxColliders, SphereColliderData[] sphereColliders, MeshColliderData[] meshColliders, Vector3Int chunkID)
        {
            string path = EditorPaths.GetWorldChunkLayoutPath(worldName);
            PathUtils.EnsurePathExists(path);
            
            AssetChunkData asset = ScriptableObject.CreateInstance<AssetChunkData>();
            asset.Meshes = meshes;
            asset.BoxColliders = boxColliders;
            asset.SphereColliders = sphereColliders;
            asset.MeshColliders = meshColliders;
            asset.ChunkID = chunkID;
            
            AssetDatabase.CreateAsset(asset, path + scriptableObjectName + ".asset");
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return asset;
        }
    }
}

