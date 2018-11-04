﻿using StreamDaddy.Editor.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace StreamDaddy.Editor.Tasks
{
    public class CreateStreamedSceneTask : Task
    {
        public CreateStreamedSceneTask() : base("Create Streamed Scene")
        {

        }

        public bool Execute(string worldName, List<Terrain> terrains)
        {

            //  Create the scene from which the world should be streamed from during gameplay.
            var streamScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            //streamScene.name = worldName + "_streamed";
            for (int i = 0; i < terrains.Count; i++)
            {
                var terrainGO = terrains[i].gameObject;

                EditorSceneManager.MoveGameObjectToScene(terrainGO, streamScene);
            }

            string scenePath = EditorPaths.GetWorldScenePath(worldName) + worldName + "_streamed.unity";
            
            string prefabsPath = EditorPaths.GetPrefabsPAth();

            string worldStreamerPath = prefabsPath + "WorldStreamer.prefab";
            GameObject worldStreamerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(worldStreamerPath);

            GameObject worldStreamerInstance = (GameObject)PrefabUtility.InstantiatePrefab(worldStreamerPrefab, streamScene);

            AssetDatabase.StartAssetEditing();
            EditorSceneManager.SaveScene(streamScene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();

            return true;
        }
    }

}
