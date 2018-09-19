using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using StreamDaddy.AssetManagement;

namespace StreamDaddy.Editor
{
    public class AssetBuilder : MonoBehaviour
    {
        [MenuItem("AssetBuilder/Build Assets")]
        static void BuildAssets()
        {
            Dictionary<string, GameObject> uniquePrefabs = new Dictionary<string, GameObject>();
            GameObject[] allGos = GameObject.FindObjectsOfType<GameObject>();

            List<Vector3> positions = new List<Vector3>();
            List<Vector3> rotations = new List<Vector3>();
            List<Vector3> scales = new List<Vector3>();
            List<string> prefabNames = new List<string>();

            foreach(var go in allGos)
            {
                var sceneRoot = (GameObject)PrefabUtility.FindPrefabRoot(go);
                var prefab = (GameObject)PrefabUtility.GetPrefabParent(sceneRoot);
                if (prefab == null)
                {
                    Debug.LogWarning("Skipping " + go.name + ". Does not appear to be a prefab!");
                    continue;
                }
                
                if (uniquePrefabs.ContainsKey(prefab.name))
                {
                    Debug.Log("Skipped " + prefab.name + ". Is duplicate!");
                    continue;
                }

                uniquePrefabs.Add(prefab.name, prefab);
                int instanceID = prefab.GetInstanceID();

                string assetPath = AssetDatabase.GetAssetPath(instanceID);
                AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant("test", "");

                positions.Add(sceneRoot.transform.position);
                rotations.Add(sceneRoot.transform.rotation.eulerAngles);
                scales.Add(sceneRoot.transform.lossyScale);
                prefabNames.Add(prefab.name);
                Debug.Log("Prefab NAme: " + prefabNames[prefabNames.Count - 1]);
            }

            AssetsTransforms transforms = AssetBundleUtils.CreateAssetsTransforms("MetaData", positions.ToArray(), rotations.ToArray(), scales.ToArray(), prefabNames.ToArray());
            
            string transformsPath = AssetDatabase.GetAssetPath(transforms.GetInstanceID());
            AssetImporter.GetAtPath(transformsPath).SetAssetBundleNameAndVariant("test", "");

            string bundlePath = Application.streamingAssetsPath;
            BuildPipeline.BuildAssetBundles(bundlePath, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.StandaloneWindows64);
        }

    }
}

