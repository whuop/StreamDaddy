using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.AssetManagement
{
    public class AssetBundleLoader : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {
            StartCoroutine(LoadBundle("test"));
        }

        IEnumerator LoadBundle(string bundleName)
        {
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
                    Debug.Log("Found prefab: " + asset.name);
                    if (prefabs.ContainsKey(asset.name))
                    {
                        Debug.LogError("Duplicate prefabs loaded from AssetBundle! Ignoring duplicates.");
                        continue;
                    }
                    prefabs.Add(asset.name, (GameObject)asset);
                }
                Debug.Log("Finished loading asset: " + asset.name);
            }

            Debug.Log("Prefab count: " + prefabs.Count);
            Debug.Log("Transforms count: " + transforms.Positions.Length);

            for(int i = 0; i < transforms.Positions.Length; i++)
            {
                Vector3 pos = transforms.Positions[i];
                Vector3 rot = transforms.Rotations[i];
                Vector3 scale = transforms.Scales[i];
                GameObject prefab = prefabs[transforms.PrefabNames[i]];
                GameObject instance = GameObject.Instantiate(prefab, transforms.Positions[i], Quaternion.Euler(rot));
                instance.transform.localScale = scale;
                Debug.Log("Spawned prefab");
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


