using StreamDaddy.AssetManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StreamDaddy.Editor.Assets
{
    public class PrefabBuildStrategy : IAssetBuildStrategy
    {
        private Dictionary<string, GameObject> m_uniquePrefabs = new Dictionary<string, GameObject>();
        
        public void BuildAssets(GameObject[] gameObjects)
        {
            List<Vector3> positions = new List<Vector3>();
            List<Vector3> rotations = new List<Vector3>();
            List<Vector3> scales = new List<Vector3>();
            List<string> prefabNames = new List<string>();

            List<GameObject> addedRoots = new List<GameObject>();

            for(int i = 0; i < gameObjects.Length; i++)
            {
                //  Find the root prefab object of this game object
                var sceneRoot = (GameObject)PrefabUtility.FindPrefabRoot(gameObjects[i]);
                //  Get the prefab asset from the root prefab instance object
                var prefab = (GameObject)PrefabUtility.GetPrefabParent(sceneRoot);

                //  Skip if no prefab was found
                if (prefab == null)
                {
                    continue;
                }

                //  Check if this prefab has already been added, if so ignore adding it
                if (m_uniquePrefabs.ContainsKey(prefab.name))
                {
                    //  Do nothing, it has already been added
                }
                else // Has not been added
                {
                    m_uniquePrefabs.Add(prefab.name, prefab);
                    int instanceID = prefab.GetInstanceID();

                    string assetPath = AssetDatabase.GetAssetPath(instanceID);
                    AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant("prefabs", "");
                }
                
                if (!addedRoots.Contains(sceneRoot))
                {
                    addedRoots.Add(sceneRoot);
                }
                else
                {
                    continue;
                }

                positions.Add(sceneRoot.transform.position);
                rotations.Add(sceneRoot.transform.rotation.eulerAngles);
                scales.Add(sceneRoot.transform.lossyScale);
                prefabNames.Add(prefab.name);
            }

            PrefabChunkData chunkData = AssetBundleUtils.CreatePrefabChunkData("prefabs", positions.ToArray(),
                                                                                         rotations.ToArray(),
                                                                                         scales.ToArray(),
                                                                                         prefabNames.ToArray());

            string chunkDataPath = AssetDatabase.GetAssetPath(chunkData.GetInstanceID());
            AssetImporter.GetAtPath(chunkDataPath).SetAssetBundleNameAndVariant("prefabs", "");

            List<UnityEngine.Object> objectsToBuild = new List<Object>();

            string bundlePath = Application.streamingAssetsPath;
            var manifest = BuildPipeline.BuildAssetBundles(bundlePath, BuildAssetBundleOptions.ChunkBasedCompression |
                                                        BuildAssetBundleOptions.DisableLoadAssetByFileName |
                                                        BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension |
                                                        BuildAssetBundleOptions.DisableWriteTypeTree, BuildTarget.StandaloneWindows64);
            
        }
    }
}


