using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.AssetManagement
{
    public enum BundleState
    {
        Unloaded = 0,
        Loading = 1,
        Loaded = 2
    }

    public class BundleReference
    {
        public string BundleName = string.Empty;
        public uint RefCount = 0;
        public BundleState State = BundleState.Unloaded;
        public AssetBundle AssetBundle;
    }

    public class AssetBundleManager : MonoBehaviour
    {
        private Dictionary<string, BundleReference> m_bundleRefs = new Dictionary<string, BundleReference>();

        private AssetManager m_assetManager;

        public void Start()
        {
            m_assetManager = GetComponent<AssetManager>();
        }

        public void LoadBundle(string bundleName)
        {
            if (!m_bundleRefs.ContainsKey(bundleName))
            {
                m_bundleRefs.Add(bundleName, new BundleReference()
                {
                    BundleName = bundleName
                });
            }

            BundleReference bundle = m_bundleRefs[bundleName];
            Debug.Log("Started loading bundle: " + bundleName);

            //  If the asset bundle hasn't started to be loaded yet, then start the process.
            if (bundle.State == BundleState.Unloaded)
            {
                bundle.RefCount++;
                StartCoroutine(LoadAssetBundle(bundleName, bundle));
            }
            else if (bundle.State == BundleState.Loading)
            {
                Debug.Log("Cannot load bundle: " + bundleName + ". That bundle is being loaded");
                bundle.RefCount++;
            }
            else if (bundle.State == BundleState.Loaded)
            {
                Debug.Log("Cannot load bundle: " + bundleName + ". That bundle is already loaded");
                bundle.RefCount++;
            }
        }

        public void UnloadBundle(string bundleName)
        {
            if (!m_bundleRefs.ContainsKey(bundleName))
            {
                Debug.LogError("Cannot unload bundle " + bundleName + ". That bundle isn't loaded.");
                return;
            }

            BundleReference bundleRef = m_bundleRefs[bundleName];

            //  Decrement the ref count. If it reaches 0 its time to unload the bundle for real.
            bundleRef.RefCount--;

            if (bundleRef.RefCount == 0)
            {
                Debug.Log("Unloading bundle: " + bundleName);

                //  Remove all the loaded assets from the asset manager.
                m_assetManager.RemoveAssets(bundleRef.AssetBundle.GetAllAssetNames());

                bundleRef.State = BundleState.Unloaded;
                bundleRef.AssetBundle.Unload(true);
                bundleRef.RefCount = 0;
                bundleRef.AssetBundle = null;
            }
            else if (bundleRef.RefCount < 0)
            {
                Debug.Log("Error unloading bundle: " + bundleName + ". Invalid RefCount: " + bundleRef.RefCount);
            }
            
        }

        private IEnumerator LoadAssetBundle(string bundleName, BundleReference bundleRef)
        {
            bundleRef.State = BundleState.Loading;
            string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, bundleName);

            //  Load bundle
            var assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(filePath);
            yield return assetBundleCreateRequest;
            Debug.Log("Finished loading bundle file: " + bundleName);

            //  Asset Bundle is now loaded. Time to load all the assets in the bundle
            AssetBundle assetBundle = assetBundleCreateRequest.assetBundle;
            var assetBundleRequest = assetBundle.LoadAllAssetsAsync();
            yield return assetBundleRequest;
            Debug.Log("Finished loading bundle assets: " + bundleName);

            bundleRef.AssetBundle = assetBundle;
            //  The whole assetbundle and all assets in it have now been loaded. Time to extract all the assets from it.
            m_assetManager.AddAssets(assetBundleRequest.allAssets);
            bundleRef.State = BundleState.Loaded;
        }
    }
}


