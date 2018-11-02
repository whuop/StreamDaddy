﻿using StreamDaddy.Editor.Utils;
using StreamDaddy.Streaming;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StreamDaddy.Editor.Tasks
{
    public class BuildWorldStreamTask : Task
    {
        public BuildWorldStreamTask() : base("Build World Stream") { }

        public bool Execute(string worldName, string chunkLayoutBundle, Vector3Int chunkSize, List<string> chunkLayoutNames, List<string> assetBundles)
        {
            if (string.IsNullOrEmpty(chunkLayoutBundle))
            {
                LogError("Chunk Layout Bundle is null or empty. Task failed!");
                return false;
            }

            if (chunkLayoutNames == null || chunkLayoutNames.Count == 0)
            {
                LogError("ChunkLayoutNames is null or has a count of 0. Task failed!");
                return false;
            }

            if (assetBundles == null || assetBundles.Count == 0)
            {
                LogError("AssetBundles is null or has a count of 0. Task failed!");
                return false;
            }
            

            WorldStream world = ScriptableObject.CreateInstance<WorldStream>();
            world.AssetBundles = assetBundles.ToArray();
            world.ChunkLayoutBundle = chunkLayoutBundle;
            world.ChunkNames = chunkLayoutNames.ToArray();
            world.ChunkSize = chunkSize;

            string path = EditorPaths.GetWorldStreamsFolder() + worldName + ".asset";

            AssetDatabase.StartAssetEditing();

            AssetDatabaseUtils.CreateOrReplaceAsset(world, path);
            EditorUtility.SetDirty(world);

            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();


            return true;
        }
    }
}

