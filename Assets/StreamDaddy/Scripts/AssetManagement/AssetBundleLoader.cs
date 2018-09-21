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
        public ShaderVariantCollection m_warmTheseUp;

        private static int m_sceneToLoad = 1;

        // Use this for initialization
        void Start()
        {
            
            m_warmTheseUp.WarmUp();
            switch(m_loadType)
            {
                case LoadType.Asset:
                    GameObjectPool.PreWarm(1000);
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
            assetBundleRequest.completed += AssetBundleRequest_completed;
            yield return assetBundleRequest;

            
            
        }

        private void AssetBundleRequest_completed(AsyncOperation obj)
        {
            var assetBundleRequest = (AssetBundleRequest)obj;
            var allAssets = assetBundleRequest.allAssets;

            //  Retrieve assets, in this case Meshes and Materials
            AssetChunkData chunkData = null;
            Type gameObjectType = typeof(GameObject);
            Type meshType = typeof(Mesh);
            Type materialType = typeof(Material);
            Type chunkDataType = typeof(AssetChunkData);

            Dictionary<string, Mesh> loadedMeshes = new Dictionary<string, Mesh>();
            Dictionary<string, Material> loadedMaterials = new Dictionary<string, Material>();

            foreach (var asset in allAssets)
            {
                Type type = asset.GetType();
                if (type.IsAssignableFrom(gameObjectType))
                {
                    var go = (GameObject)asset;
                }
                else if (type.IsAssignableFrom(meshType))
                {
                    var mesh = (Mesh)asset;
                    loadedMeshes.Add(mesh.name, mesh);
                }
                else if (type.IsAssignableFrom(materialType))
                {
                    var material = (Material)asset;
                    loadedMaterials.Add(material.name, material);
                }
                else if (type.IsAssignableFrom(chunkDataType))
                {
                    chunkData = (AssetChunkData)asset;
                }
                else
                {
                    Debug.Log("Loading unknown asset type: " + asset.GetType());
                }
            }

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            List<Material> goMaterials = new List<Material>();
            for (int i = 0; i < chunkData.Positions.Length; i++)
            {
                var positions = chunkData.Positions;
                var rotations = chunkData.Rotations;
                var scales = chunkData.Scales;
                var meshes = chunkData.MeshNames;
                var materials = chunkData.Materials;
                Mesh mesh = loadedMeshes[meshes[i]];

                for (int j = 0; j < materials[i].MaterialNames.Length; j++)
                {
                    goMaterials.Add(loadedMaterials[materials[i].MaterialNames[j]]);
                }

                var go = GameObjectPool.GetRenderer(mesh, goMaterials.ToArray(), positions[i], rotations[i], scales[i]);
                goMaterials.Clear();
            }

            watch.Stop();
            //stopWatch.Stop();
            //Debug.Log("AssetBased spawn time: " + watch.ElapsedMilliseconds + " ticks: " + watch.ElapsedTicks);
            //Debug.Log("AssetBased Loading Time: " + stopWatch.ElapsedMilliseconds);
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
            PrefabChunkData chunkData = null;
            var allAssets = assetBundleRequest.allAssets;
            Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
            foreach (var asset in allAssets)
            {
                if (asset.name == "MetaData")
                {
                    chunkData = (PrefabChunkData)asset;
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

            for (int i = 0; i < chunkData.Positions.Length; i++)
            {
                Vector3 pos = chunkData.Positions[i];
                Vector3 rot = chunkData.Rotations[i];
                Vector3 scale = chunkData.Scales[i];
                GameObject prefab = prefabs[chunkData.PrefabNames[i]];
                GameObject instance = GameObject.Instantiate(prefab, chunkData.Positions[i], Quaternion.Euler(rot));
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


