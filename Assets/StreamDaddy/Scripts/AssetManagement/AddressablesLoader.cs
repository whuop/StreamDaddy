using StreamDaddy.Chunking;
using StreamDaddy.Streaming;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;

namespace StreamDaddy.AssetManagement
{
    public class AddressablesLoader
    {
        private static List<UnityEngine.ResourceManagement.IAsyncOperation<AssetChunkData>> m_layoutsLoading = new List<UnityEngine.ResourceManagement.IAsyncOperation<AssetChunkData>>();
        private static List<AssetChunkData> m_chunkLayouts = new List<AssetChunkData>();

        public delegate void FinishedLoadingLayoutsDelegate(List<AssetChunkData> chunkLayouts);

        private static FinishedLoadingLayoutsDelegate m_onFinishedLoadingLayouts;
        
        /// <summary>
        /// Contains all of the meshes that have been loaded.
        /// The key is the runtime key of the asset reference.
        /// </summary>
        private static Dictionary<Hash128, Mesh> m_loadedMeshes = new Dictionary<Hash128, Mesh>();

        /// <summary>
        /// Contains all of the loaded materials.
        /// The key is the runtime key of the asset reference.
        /// </summary>
        private static Dictionary<Hash128, Material> m_loadedMaterials = new Dictionary<Hash128, Material>();

        public static void Initialize(FinishedLoadingLayoutsDelegate onFinishedLoadingLayouts)
        {
            m_onFinishedLoadingLayouts = onFinishedLoadingLayouts;
        }

        public static Mesh GetMesh(Hash128 runtimeKey)
        {
            return m_loadedMeshes[runtimeKey];
        }

        public static Material GetMaterial(Hash128 runtimeKey)
        {
            return m_loadedMaterials[runtimeKey];
        }

        public static void LoadWorldLayouts(WorldStream stream)
        {
            for(int i = 0; i < stream.ChunkLayoutReferences.Count; i++)
            {
                var layoutLoader = stream.ChunkLayoutReferences[i].LoadAsset<AssetChunkData>();
                layoutLoader.Completed += LayoutLoaderCompleted;
                m_layoutsLoading.Add(layoutLoader);
            }
        }

        public static void LoadChunkAssets(AssetChunkData chunkAssets)
        {
            //  Load meshes for renderables
            for(int i = 0; i < chunkAssets.MeshLayers.Length; i++)
            {
                var layer = chunkAssets.MeshLayers[i];
                
                //  Loop through the different LOD levels
                for(int j = 0; j < layer.Meshes.Length; j++)
                {
                    var lod = layer.Meshes[j];

                    //  If this mesh has already been loaded, then skip it.
                    if (m_loadedMeshes.ContainsKey(lod.MeshReference.RuntimeKey))
                        continue;

                    //  Load the mesh and create an empty entry to chuck it into in the loaded meshes dictionary.
                    var meshOperation = lod.MeshReference.LoadAsset<GameObject>();

                    meshOperation.Completed += MeshOperationCompleted;
                }
            }

            //  Load all materials
            for(int i = 0; i < chunkAssets.MeshMaterials.Length; i++)
            {
                var materials = chunkAssets.MeshMaterials[i];

                for(int j = 0; j < materials.MaterialReferences.Length; j++)
                {
                    var materialRef = materials.MaterialReferences[j];
                    if (m_loadedMaterials.ContainsKey(materialRef.RuntimeKey))
                        continue;

                    var materialOperation = materialRef.LoadAsset<Material>();

                    materialOperation.Completed += MaterialOperationCompleted;
                }
            }

            //  Load meshes for MeshColliders
            for(int i = 0; i < chunkAssets.MeshColliderLayers.Length; i++)
            {
                var layer = chunkAssets.MeshColliderLayers[i];

                //  Load all the different LODs of the mesh
                for(int j = 0; j < layer.Meshes.Length; j++)
                {
                    var lod = layer.Meshes[j];

                    //  If the mesh has already been loaded, then skip it.
                    if (m_loadedMeshes.ContainsKey(lod.MeshReference.RuntimeKey))
                        continue;

                    //  Load the mesh and create an empty slot to chuck it into in the loaded meshes dictionary
                    var meshOperation = lod.MeshReference.LoadAsset<GameObject>();
                    //m_loadedMeshes.Add(lod.MeshReference.RuntimeKey, null);

                    meshOperation.Completed += MeshOperationCompleted;
                }
            }
        }

        private static void MeshOperationCompleted(UnityEngine.ResourceManagement.IAsyncOperation<GameObject> obj)
        {
            Hash128 key = (Hash128)obj.Key;
            IResourceLocation location = (IResourceLocation)obj.Context;
            
            var meshfilters = obj.Result.GetComponentsInChildren<MeshFilter>();

            string meshRootAddress = location.InternalId;
            for(int i = 0; i < meshfilters.Length; i++)
            {
                var filter = meshfilters[i];
                string meshAddress = location.InternalId + "_" + filter.sharedMesh.name;
                Hash128 meshKey = Hash128.Compute(meshAddress);
                if (m_loadedMeshes.ContainsKey(meshKey))
                {
                    Debug.LogError(string.Format("Trying to add duplicate mesh with address {0} and hash {1}", meshAddress, meshKey.ToString()));
                    continue;
                }
                m_loadedMeshes.Add(meshKey, filter.sharedMesh);
            }
            
        }

        private static void MaterialOperationCompleted(UnityEngine.ResourceManagement.IAsyncOperation<Material> obj)
        {
            Hash128 key = (Hash128)obj.Key;
            if (m_loadedMaterials.ContainsKey(key))
            {
                IResourceLocation location = (IResourceLocation)obj.Context;
                Debug.LogError(string.Format("Trying to add duplicate material with address {0} and hash {1}", location.InternalId, key.ToString()));
            }
            m_loadedMaterials.Add(key, obj.Result);
        }

        private static void LayoutLoaderCompleted(UnityEngine.ResourceManagement.IAsyncOperation<AssetChunkData> obj)
        {
            //  Remove this async operation from the list of loading layouts. 
            m_layoutsLoading.Remove(obj);
            //  Add layout to the list of loaded layouts.
            m_chunkLayouts.Add(obj.Result);
            
            if (m_layoutsLoading.Count == 0)
            {
                Debug.Log("Finished loading world stream layouts");
                if (m_onFinishedLoadingLayouts != null)
                    m_onFinishedLoadingLayouts(m_chunkLayouts);
            }
        }
    }
}


