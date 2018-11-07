using StreamDaddy.Editor.Utils;
using StreamDaddy.Streaming;
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
            if (EditorUtility.DisplayCancelableProgressBar("Building Stream Scene", worldName, 0.5f))
                return false;
            //  Create the scene from which the world should be streamed from during gameplay.
            var streamScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            //streamScene.name = worldName + "_streamed";
            for (int i = 0; i < terrains.Count; i++)
            {
                var terrainGO = terrains[i].gameObject;

                EditorSceneManager.MoveGameObjectToScene(terrainGO, streamScene);

                //  Deactivate the terrain 
                terrainGO.SetActive(false);
            }

            string scenePath = EditorPaths.GetWorldScenePath(worldName) + worldName + "_streamed.unity";
            
            string prefabsPath = EditorPaths.GetPrefabsPAth();

            string worldStreamerPath = prefabsPath + "WorldStreamer.prefab";
            GameObject worldStreamerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(worldStreamerPath);

            string worldStreamPath = EditorPaths.GetWorldStreamsFolder() + worldName + ".asset";

            WorldStream stream = AssetDatabase.LoadAssetAtPath<WorldStream>(worldStreamPath);

            GameObject worldStreamerInstance = (GameObject)PrefabUtility.InstantiatePrefab(worldStreamerPrefab, streamScene);
            
            //  Set all world streamer values needed to stream the world.
            WorldStreamer streamer = worldStreamerInstance.GetComponent<WorldStreamer>();
            streamer.WorldStream = stream;
            streamer.WorldTerrains = terrains;

            AssetDatabase.StartAssetEditing();
            EditorSceneManager.SaveScene(streamScene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();

            return true;
        }
    }

}

