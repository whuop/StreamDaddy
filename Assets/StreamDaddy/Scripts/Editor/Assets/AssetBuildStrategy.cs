﻿using StreamDaddy.AssetManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StreamDaddy.Editor.Assets
{
    public class AssetBuildStrategy : IAssetBuildStrategy
    {
        private Dictionary<string, Mesh> m_uniqueMeshes = new Dictionary<string, Mesh>();
        private Dictionary<string, Material> m_uniqueMaterials = new Dictionary<string, Material>();

        public void BuildAssets(GameObject[] gameObjects)
        {
            //  First off, fetch all MeshRenderers, these have the data we want in them, or on their game objects.
            List<MeshRenderer> allRenderers = new List<MeshRenderer>();
            foreach(var go in gameObjects)
            {
                var renderer = go.GetComponent<MeshRenderer>();
                if (renderer != null)
                    allRenderers.Add(renderer);
            }

            List<Vector3> positions = new List<Vector3>();
            List<Vector3> rotations = new List<Vector3>();
            List<Vector3> scales = new List<Vector3>();
            List<string> meshes = new List<string>();
            List<List<string>> materials = new List<List<string>>();

            foreach (var renderer in allRenderers)
            {
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter.sharedMesh == null)
                    continue;
                if (m_uniqueMeshes.ContainsKey(meshFilter.sharedMesh.name))
                {
                    Debug.LogError("Skipped mesh " + meshFilter.sharedMesh.name + ". Is duplicate!");
                }
                else
                {
                    m_uniqueMeshes.Add(meshFilter.sharedMesh.name, meshFilter.sharedMesh);
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
                    AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant("assetbased", "");
                }

                List<string> goMaterials = new List<string>();
                foreach (var material in renderer.sharedMaterials)
                {
                    if (m_uniqueMaterials.ContainsKey(material.name))
                    {
                        //Debug.LogError("Skipped material " + material.name + ". Is duplicate!");
                    }
                    else
                    {
                        m_uniqueMaterials.Add(material.name, material);
                        // Do material Asset Bundle assigment here
                        int instanceID = material.GetInstanceID();
                        string assetPath = AssetDatabase.GetAssetPath(instanceID);
                        AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant("assetbased", "");
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
            for (int i = 0; i < materials.Count; i++)
            {
                assetMaterials[i] = new string[materials[i].Count];
                for (int j = 0; j < materials[i].Count; j++)
                {
                    assetMaterials[i][j] = materials[i][j];
                }
            }

            AssetChunkData chunkData = AssetBundleUtils.CreateRenderableAssets("assetbased", positions.ToArray(), rotations.ToArray(), scales.ToArray(), meshes.ToArray(), assetMaterials);
            
            string transformsPath = AssetDatabase.GetAssetPath(chunkData.GetInstanceID());
            AssetImporter.GetAtPath(transformsPath).SetAssetBundleNameAndVariant("assetbased", "");

            string bundlePath = Application.streamingAssetsPath;
            BuildPipeline.BuildAssetBundles(bundlePath, BuildAssetBundleOptions.ChunkBasedCompression |
                                                        BuildAssetBundleOptions.DisableLoadAssetByFileName |
                                                        BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension |
                                                        BuildAssetBundleOptions.DisableWriteTypeTree,
                                                        BuildTarget.StandaloneWindows64);
        }
    }
}

