using StreamDaddy.Pooling;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.AssetManagement
{
    public class AssetBundleLoader : MonoBehaviour
    {
        public enum LoadType
        {
            Prefab,
            Asset,
            Scene
        }

        public LoadType m_loadType;

        private static int m_sceneToLoad = 1;

        // Use this for initialization
        void Start()
        {
            switch(m_loadType)
            {
                case LoadType.Asset:
                    GameObjectPool.PreWarm(16*10);
                    StartCoroutine(LoadBundleAssetBased("AssetBased"));
                    break;
                case LoadType.Prefab:
                    StartCoroutine(LoadBundle("test"));
                    break;
                case LoadType.Scene:
                    StartCoroutine(LoadScene());
                    break;
            }
        }

        IEnumerator LoadBundleAssetBased(string bundleName)
        {
            yield return new WaitForSeconds(2.0f);

            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, bundleName);

            //  Load bundle
            var assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(filePath);
            yield return assetBundleCreateRequest;

            //  Load all assets in the bundle
            AssetBundle assetBundle = assetBundleCreateRequest.assetBundle;
            var assetBundleRequest = assetBundle.LoadAllAssetsAsync();
            yield return assetBundleRequest;

            //  Retrieve assets, in this case Meshes and Materials
            RenderableAsset renderableAssets = null;
            var allAssets = assetBundleRequest.allAssets;

            Type gameObjectType = typeof(GameObject);
            Type meshType = typeof(Mesh);
            Type materialType = typeof(Material);
            Type renderableAssetType = typeof(RenderableAsset);

            Dictionary<string, Mesh> loadedMeshes = new Dictionary<string, Mesh>();
            Dictionary<string, Material> loadedMaterials = new Dictionary<string, Material>();

            foreach(var asset in allAssets)
            {
                Type type = asset.GetType();
                if (type.IsAssignableFrom(gameObjectType))
                {
                    var go = (GameObject)asset;
                    Debug.Log("Loaded Game Object:" + go.name);
                }
                else if (type.IsAssignableFrom(meshType))
                {
                    var mesh = (Mesh)asset;
                    loadedMeshes.Add(mesh.name, mesh);
                    Debug.Log("Loaded Mesh: " + mesh.name);
                }
                else if (type.IsAssignableFrom(materialType))
                {
                    var material = (Material)asset;
                    loadedMaterials.Add(material.name, material);
                }
                else if (type.IsAssignableFrom(renderableAssetType))
                {
                    renderableAssets = (RenderableAsset)asset;
                }
                else
                {
                    Debug.Log("Loading unknown asset type: " + asset.GetType());
                }
            }

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            List<Material> goMaterials = new List<Material>();
            for(int i = 0; i < renderableAssets.Positions.Length; i++)
            {
                var positions = renderableAssets.Positions;
                var rotations = renderableAssets.Rotations;
                var scales = renderableAssets.Scales;
                var meshes = renderableAssets.MeshNames;
                var materials = renderableAssets.Materials;
                Mesh mesh = loadedMeshes[meshes[i]];

                for(int j = 0; j < materials[i].MaterialNames.Length; j++)
                {
                    goMaterials.Add(loadedMaterials[materials[i].MaterialNames[j]]);
                }

                var go = GameObjectPool.GetRenderer(mesh, goMaterials.ToArray(), positions[i], rotations[i], scales[i]);
                goMaterials.Clear();
            }

            watch.Stop();
            stopWatch.Stop();
            Debug.Log("AssetBased spawn time: " + watch.ElapsedMilliseconds + " ticks: " + watch.ElapsedTicks);
            Debug.Log("AssetBased Loading Time: " + stopWatch.ElapsedMilliseconds);
        }

        IEnumerator LoadBundle(string bundleName)
        {
            yield return new WaitForSeconds(2.0f);

            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, bundleName);


            //  Load bundle
            var assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(filePath);
            yield return assetBundleCreateRequest;

            //  Load all assets in the bundle
            AssetBundle assetBundle = assetBundleCreateRequest.assetBundle;
            var assetBundleRequest = assetBundle.LoadAllAssetsAsync();
            yield return assetBundleRequest;

            //  Retrieve object, in this case Prefab ( GameObject )
            AssetsTransforms transforms = null;
            var allAssets = assetBundleRequest.allAssets;
            Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
            foreach (var asset in allAssets)
            {
                if (asset.name == "MetaData")
                {
                    transforms = (AssetsTransforms)asset;
                }
                else
                {
                    //Debug.Log("Found prefab: " + asset.name);
                    if (prefabs.ContainsKey(asset.name))
                    {
                        //Debug.LogError("Duplicate prefabs loaded from AssetBundle! Ignoring duplicates.");
                        continue;
                    }
                    prefabs.Add(asset.name, (GameObject)asset);
                }
                //Debug.Log("Finished loading asset: " + asset.name);
            }

            //Debug.Log("Prefab count: " + prefabs.Count);
            //Debug.Log("Transforms count: " + transforms.Positions.Length);

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            for (int i = 0; i < transforms.Positions.Length; i++)
            {
                Vector3 pos = transforms.Positions[i];
                Vector3 rot = transforms.Rotations[i];
                Vector3 scale = transforms.Scales[i];
                GameObject prefab = prefabs[transforms.PrefabNames[i]];
                GameObject instance = GameObject.Instantiate(prefab, transforms.Positions[i], Quaternion.Euler(rot));
                instance.transform.localScale = scale;
                //Debug.Log("Spawned prefab");
            }

            watch.Stop();
            stopWatch.Stop();
            Debug.Log("PrefabBased spawn time: " + watch.ElapsedMilliseconds + " ticks: " + watch.ElapsedTicks);
            Debug.Log("PrefabBased Loading Time: " + stopWatch.ElapsedMilliseconds);
        }

        private IEnumerator LoadScene()
        {
            yield return new WaitForSeconds(2.0f);

            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(m_sceneToLoad, UnityEngine.SceneManagement.LoadSceneMode.Additive);

            stopWatch.Stop();
            Debug.Log("SceneBased Loading Time: " + stopWatch.ElapsedMilliseconds);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


