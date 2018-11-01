using StreamDaddy.AssetManagement;
using StreamDaddy.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace StreamDaddy.Editor
{
    public class AssetBundleUtils : MonoBehaviour
    {
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

