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
        [MenuItem("AssetBuilder/Clear All AssetBundles")]
        static void ClearAllAssetBundles()
        {
            Debug.Log("Started clearing");
            List<GameObject> prefabs = FindAssetsByType<GameObject>();

            foreach (var go in prefabs)
            {
                var prefab = (GameObject)PrefabUtility.GetPrefabParent(go);
                if (prefab == null)
                    continue;
                int instanceID = prefab.GetInstanceID();
                
                string assetPath = AssetDatabase.GetAssetPath(instanceID);
                AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant("", "");
                Debug.Log("Cleared " + prefab.name);
            }

            Dictionary<string, Mesh> uniqueMeshes = new Dictionary<string, Mesh>();
            Dictionary<string, Material> uniqueMaterials = new Dictionary<string, Material>();

            MeshRenderer[] allRenderers = GameObject.FindObjectsOfType<MeshRenderer>();
            foreach (var renderer in allRenderers)
            {
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter.sharedMesh == null)
                    continue;
                if (uniqueMeshes.ContainsKey(meshFilter.sharedMesh.name))
                {
                    Debug.LogError("Skipped mesh " + meshFilter.sharedMesh.name + ". Is duplicate!");
                }
                else
                {
                    uniqueMeshes.Add(meshFilter.sharedMesh.name, meshFilter.sharedMesh);
                    //  Do mesh Asset Bundle assignment here
                    int instanceID = meshFilter.sharedMesh.GetInstanceID();
                    string assetPath = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);//AssetDatabase.GetAssetPath(instanceID);
                                                                                         //if (assetPath == string.Empty)
                                                                                         //    assetPath = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);

                    if (assetPath == "Library/unity default resources")
                    {
                        Debug.Log("Skipped unity default asset: " + meshFilter.gameObject.name);
                        continue;
                    }

                    Debug.Log("AssetPath: " + assetPath);
                    AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant("", "");
                }

                List<string> goMaterials = new List<string>();
                foreach (var material in renderer.sharedMaterials)
                {
                    if (uniqueMaterials.ContainsKey(material.name))
                    {
                        //Debug.LogError("Skipped material " + material.name + ". Is duplicate!");
                    }
                    else
                    {
                        uniqueMaterials.Add(material.name, material);
                        // Do material Asset Bundle assigment here
                        int instanceID = material.GetInstanceID();
                        string assetPath = AssetDatabase.GetAssetPath(instanceID);
                        AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant("", "");
                    }

                    goMaterials.Add(material.name);
                }
            }
        }

        public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T).ToString().Replace("UnityEngine.","")));
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            return assets;
        }

        [MenuItem("AssetBuilder/Build Assets")]
        static void PrefabBasedBuildAssets()
        {
            Dictionary<string, GameObject> uniquePrefabs = new Dictionary<string, GameObject>();
            GameObject[] allGos = GameObject.FindObjectsOfType<GameObject>();

            List<Vector3> positions = new List<Vector3>();
            List<Vector3> rotations = new List<Vector3>();
            List<Vector3> scales = new List<Vector3>();
            List<string> prefabNames = new List<string>();

            List<GameObject> addedRoots = new List<GameObject>();

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
                }
                else
                {
                    uniquePrefabs.Add(prefab.name, prefab);
                    int instanceID = prefab.GetInstanceID();

                    string assetPath = AssetDatabase.GetAssetPath(instanceID);
                    AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant("test", "");
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
                Debug.Log("Prefab NAme: " + prefabNames[prefabNames.Count - 1]);
            }

            AssetsTransforms transforms = AssetBundleUtils.CreateAssetsTransforms("MetaData", positions.ToArray(), rotations.ToArray(), scales.ToArray(), prefabNames.ToArray());
            
            string transformsPath = AssetDatabase.GetAssetPath(transforms.GetInstanceID());
            AssetImporter.GetAtPath(transformsPath).SetAssetBundleNameAndVariant("test", "");

            string bundlePath = Application.streamingAssetsPath;
            BuildPipeline.BuildAssetBundles(bundlePath, BuildAssetBundleOptions.ChunkBasedCompression |
                                                        BuildAssetBundleOptions.DisableLoadAssetByFileName |
                                                        BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension |
                                                        BuildAssetBundleOptions.DisableWriteTypeTree, BuildTarget.StandaloneWindows64);
        }

        [MenuItem("AssetBuilder/Build Assets, Assets based")]
        static void AssetBasedBuildAssets()
        {
            
            Dictionary<string, Mesh> uniqueMeshes = new Dictionary<string, Mesh>();
            Dictionary<string, Material> uniqueMaterials = new Dictionary<string, Material>();

            MeshRenderer[] allRenderers = GameObject.FindObjectsOfType<MeshRenderer>();

            List<Vector3> positions = new List<Vector3>();
            List<Vector3> rotations = new List<Vector3>();
            List<Vector3> scales = new List<Vector3>();
            List<string> meshes = new List<string>();
            List<List<string>> materials = new List<List<string>>();
            
            foreach(var renderer in allRenderers)
            {
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter.sharedMesh == null)
                    continue;
                if (uniqueMeshes.ContainsKey(meshFilter.sharedMesh.name))
                {
                    Debug.LogError("Skipped mesh " + meshFilter.sharedMesh.name + ". Is duplicate!");
                }
                else
                {
                    uniqueMeshes.Add(meshFilter.sharedMesh.name, meshFilter.sharedMesh);
                    //  Do mesh Asset Bundle assignment here
                    int instanceID = meshFilter.sharedMesh.GetInstanceID();
                    string assetPath = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);//AssetDatabase.GetAssetPath(instanceID);
                                                                                         //if (assetPath == string.Empty)
                                                                                         //    assetPath = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);

                    if (assetPath == "Library/unity default resources")
                    {
                        Debug.Log("Skipped unity default asset: " + meshFilter.gameObject.name);
                        continue;
                    }

                    Debug.Log("AssetPath: " + assetPath);
                    AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant("AssetBased", "");
                }

                List<string> goMaterials = new List<string>();
                foreach (var material in renderer.sharedMaterials)
                {
                    if (uniqueMaterials.ContainsKey(material.name))
                    {
                        //Debug.LogError("Skipped material " + material.name + ". Is duplicate!");
                    }
                    else
                    {
                        uniqueMaterials.Add(material.name, material);
                        // Do material Asset Bundle assigment here
                        int instanceID = material.GetInstanceID();
                        string assetPath = AssetDatabase.GetAssetPath(instanceID);
                        AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant("AssetBased", "");
                    }

                    goMaterials.Add(material.name);
                }
                materials.Add(goMaterials);
                
                GameObject go = renderer.gameObject;
                positions.Add(go.transform.position);
                rotations.Add(go.transform.rotation.eulerAngles);
                scales.Add(go.transform.lossyScale);
                meshes.Add(meshFilter.sharedMesh.name);
            }

            string[][] assetMaterials = new string[materials.Count][];
            for(int i = 0; i < materials.Count; i++)
            {
                assetMaterials[i] = new string[materials[i].Count];
                for(int j = 0; j < materials[i].Count; j++)
                {
                    assetMaterials[i][j] = materials[i][j];
                }
            }

            RenderableAsset transforms = AssetBundleUtils.CreateRenderableAssets("MetaDataAssetBased", positions.ToArray(), rotations.ToArray(), scales.ToArray(), meshes.ToArray(), assetMaterials);

            string transformsPath = AssetDatabase.GetAssetPath(transforms.GetInstanceID());
            AssetImporter.GetAtPath(transformsPath).SetAssetBundleNameAndVariant("assetbased", "");

            string bundlePath = Application.streamingAssetsPath;
            BuildPipeline.BuildAssetBundles(bundlePath, BuildAssetBundleOptions.ChunkBasedCompression | 
                                                        BuildAssetBundleOptions.DisableLoadAssetByFileName | 
                                                        BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension | 
                                                        BuildAssetBundleOptions.DisableWriteTypeTree, 
                                                        BuildTarget.StandaloneWindows64);

            /*foreach(var kvp in uniqueMeshes)
            {
                int instanceID = kvp.Value.GetInstanceID();
                string assetPath = AssetDatabase.GetAssetPath(instanceID);
                AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant("", "");
            }

            foreach(var kvp in uniqueMaterials)
            {
                int instanceID = kvp.Value.GetInstanceID();
                string assetPath = AssetDatabase.GetAssetPath(instanceID);
                AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant("", "");
            }*/

        }

        [MenuItem("AssetBuilder/Build all assets")]
        static void BuildAllAssetBundles()
        {
            string bundlePath = Application.streamingAssetsPath;
            BuildPipeline.BuildAssetBundles(bundlePath, BuildAssetBundleOptions.ChunkBasedCompression |
                                                        BuildAssetBundleOptions.DisableLoadAssetByFileName |
                                                        BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension |
                                                        BuildAssetBundleOptions.DisableWriteTypeTree,
                                                        BuildTarget.StandaloneWindows64);
        }
    }
}

