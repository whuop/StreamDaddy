using StreamDaddy.Editor.Utils;
using StreamDaddy.Streaming;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace StreamDaddy.Editor.Tasks
{
    public class BuildWorldStreamTask : Task
    {
        public BuildWorldStreamTask() : base("Build World Stream") { }

        public bool Execute(string worldName, string chunkLayoutBundle, Vector3Int chunkSize, List<AssetReference> chunkLayoutReferences, List<string> assetBundles)
        {
            if (string.IsNullOrEmpty(chunkLayoutBundle))
            {
                LogError("Chunk Layout Bundle is null or empty. Task failed!");
                return false;
            }

            if (chunkLayoutReferences == null || chunkLayoutReferences.Count == 0)
            {
                LogError("ChunkLayoutNames is null or has a count of 0. Task failed!");
                return false;
            }

            if (assetBundles == null || assetBundles.Count == 0)
            {
                LogError("AssetBundles is null or has a count of 0. Task failed!");
                return false;
            }

            if (EditorUtility.DisplayCancelableProgressBar("Building WorldStream", worldName, 0.5f))
                return false;

            WorldStream world = ScriptableObject.CreateInstance<WorldStream>();
            world.WorldName = worldName;
            world.AssetBundles = assetBundles.ToArray();
            world.ChunkLayoutBundle = chunkLayoutBundle;

            world.ChunkLayoutReferences = chunkLayoutReferences;
            //world.ChunkNames = chunkLayoutNames.ToArray();
            world.ChunkSize = chunkSize;

            string path = EditorPaths.GetWorldStreamsFolder() + worldName + ".asset";

            AssetDatabase.StartAssetEditing();

            AssetDatabaseUtils.CreateOrReplaceAsset(world, path);
            EditorUtility.SetDirty(world);

            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();

            return true;
        }
    }
}


