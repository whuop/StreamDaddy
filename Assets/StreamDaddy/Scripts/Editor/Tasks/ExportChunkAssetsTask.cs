using StreamDaddy.Editor.Chunking;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StreamDaddy.Editor.Tasks
{
    public class ExportChunkAssetsTask : Task
    {
        private Dictionary<string, Mesh> m_uniqueMeshes = new Dictionary<string, Mesh>();
        private Dictionary<string, Material> m_uniqueMaterials = new Dictionary<string, Material>();

        private HashSet<int> m_processedInstanceIDs = new HashSet<int>();

        public ExportChunkAssetsTask() : base("Export Chunk Assets")
        {

        }

        public bool Execute(string worldName, List<EditorChunk> chunks)
        {
            m_uniqueMeshes.Clear();
            m_uniqueMaterials.Clear();
            m_processedInstanceIDs.Clear();
            
            List<MeshRenderer> allRenderers = new List<MeshRenderer>();
            List<BoxCollider> allBoxColliders = new List<BoxCollider>();
            List<SphereCollider> allSphereColliders = new List<SphereCollider>();
            List<MeshCollider> allMeshColliders = new List<MeshCollider>();

            //  Add the asset bundle name to the list of all asset bundles in the project.
            string assetBundleName = worldName + "_chunkassets";
            
            for(int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                //  Fetch all the game objects in the chunk so that
                //  all the assets can be extracted.
                //var gameObjects = chunk.GetAllChildren();

                if (EditorUtility.DisplayCancelableProgressBar("Exporting Chunk Assets", "Chunk " + i, i / chunks.Count))
                    return false;

                //  Extract all assets to be built into asset bundles.
                /*foreach (var go in gameObjects)
                {
                    var renderer = go.GetComponent<MeshRenderer>();
                    var boxCollider = go.GetComponent<BoxCollider>();
                    var sphereCollider = go.GetComponent<SphereCollider>();
                    var meshCollider = go.GetComponent<MeshCollider>();

                    if (renderer != null)
                        allRenderers.Add(renderer);
                    if (boxCollider != null)
                        allBoxColliders.Add(boxCollider);
                    if (sphereCollider != null)
                        allSphereColliders.Add(sphereCollider);
                    if (meshCollider != null)
                        allMeshColliders.Add(meshCollider);
                }*/

                foreach(var renderer in allRenderers)
                {
                    var meshFilter = renderer.GetComponent<MeshFilter>();
                    //  Skip if there is no mesh filter attached, as it isn't being rendered
                    // in the scene then anyway.
                    if (meshFilter.sharedMesh == null)
                        continue;

                    // Build meshes
                    if (m_uniqueMeshes.ContainsKey(meshFilter.sharedMesh.name))
                    {
                        //  Do nothing, this mesh has already been processed.
                    }
                    else
                    {
                        m_uniqueMeshes.Add(meshFilter.sharedMesh.name, meshFilter.sharedMesh);
                        //  Do mesh asset bundle assignment
                        int instanceID = meshFilter.sharedMesh.GetInstanceID();
                        string assetPath = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);

                        if (assetPath == "Library/unity default resources")
                        {
                            Debug.Log(string.Format("[Task-{0}] Skipped asset {1} is default resource.", this.Name, meshFilter.sharedMesh.name));
                        }
                        else
                        {
                            AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
                            if (assetImporter != null)
                            {
                                Debug.Log(string.Format("[Task-{0}] Setting AssetBundle for {1} to bundle {2}", this.Name, meshFilter.sharedMesh.name, assetBundleName));
                                assetImporter.SetAssetBundleNameAndVariant(assetBundleName, "");
                            }
                            else
                            {
                                Debug.Log(string.Format("[Task-{0}] Can not find Mesh in GameObject {1}", this.Name, meshFilter.gameObject.name), meshFilter.gameObject);
                            }
                        }
                    }

                    //  Build materials
                    foreach(var material in renderer.sharedMaterials)
                    {
                        if (material == null)
                        {
                            Debug.Log(string.Format("[Task-{0}] Can not export null Material in GameObject {1}", this.Name, renderer.gameObject.name), renderer.gameObject);
                            continue;
                        }

                        if (m_uniqueMaterials.ContainsKey(material.name))
                        {
                            //  Skipping material, has already been processed.
                        }
                        else
                        {
                            m_uniqueMaterials.Add(material.name, material);
                            // Do material asset bundle assignment
                            int instanceID = material.GetInstanceID();
                            string assetPath = AssetDatabase.GetAssetPath(instanceID);
                            
                            if (assetPath == "Resources/unity_builtin_extra")
                            {
                                LogInfo(string.Format("Skipping material for mesh {0}. Is unity built in material with asset path: {1}.", meshFilter.sharedMesh.name, assetPath));
                                continue;
                            }

                            AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant(assetBundleName, "");
                        }
                    }
                }
            }

            EditorUtility.ClearProgressBar();

            string bundlePath = EditorPaths.STREAMING_DIRECTORY_PATH;
            BuildPipeline.BuildAssetBundles(bundlePath, BuildAssetBundleOptions.ChunkBasedCompression |
                                                        BuildAssetBundleOptions.DisableLoadAssetByFileName |
                                                        BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension |
                                                        BuildAssetBundleOptions.DisableWriteTypeTree,
                                                        BuildTarget.StandaloneWindows64);
            return true;
        }
    }

}

