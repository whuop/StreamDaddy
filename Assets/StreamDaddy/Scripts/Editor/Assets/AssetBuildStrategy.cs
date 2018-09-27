using StreamDaddy.AssetManagement;
using StreamDaddy.Editor.Chunking;
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

        private void Clear()
        {
            m_uniqueMeshes.Clear();
            m_uniqueMaterials.Clear();
        }

        public void BuildChunkAssets(string worldName, EditorChunk chunk, List<string> assetBundles)
        {
            Clear();
            //  First off, fetch all MeshRenderers, these have the data we want in them, or on their game objects.
            List<MeshRenderer> allRenderers = new List<MeshRenderer>();
            
            //  Change this to use mroe than one assebundle later if needed.
            if (!assetBundles.Contains(worldName + "_chunkassets"))
            {
                assetBundles.Add(worldName + "_chunkassets");
            }

            var gameObjects = chunk.GetAllChildren();

            foreach (var go in gameObjects)
            {
                var renderer = go.GetComponent<MeshRenderer>();
                if (renderer != null)
                    allRenderers.Add(renderer);
            }
            
            foreach (var renderer in allRenderers)
            {
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter.sharedMesh == null)
                    continue;
                if (m_uniqueMeshes.ContainsKey(meshFilter.sharedMesh.name))
                {
                    //  Skipping mesh, has already been processed
                }
                else
                {
                    m_uniqueMeshes.Add(meshFilter.sharedMesh.name, meshFilter.sharedMesh);
                    //  Do mesh Asset Bundle assignment here
                    int instanceID = meshFilter.sharedMesh.GetInstanceID();
                    string assetPath = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);

                    if (assetPath == "Library/unity default resources")
                    {
                        Debug.Log("Skipped unity default asset: " + meshFilter.gameObject.name);
                        continue;
                    }

                    Debug.Log("AssetPath: " + assetPath);
                    AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant(worldName + "_chunkassets", "");
                }
                
                foreach (var material in renderer.sharedMaterials)
                {
                    if (m_uniqueMaterials.ContainsKey(material.name))
                    {
                        //  Skipping material, has already been processed
                    }
                    else
                    {
                        m_uniqueMaterials.Add(material.name, material);
                        // Do material Asset Bundle assigment here
                        int instanceID = material.GetInstanceID();
                        string assetPath = AssetDatabase.GetAssetPath(instanceID);
                        AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant(worldName + "_chunkassets", "");
                    }
                }
            }
        }

        public void BuildChunkLayout(string worldName, EditorChunk chunk)
        {
            Clear();
            List<MeshRenderer> allRenderers = new List<MeshRenderer>();
            var gameObjects = chunk.GetAllChildren();

            List<Vector3> positions = new List<Vector3>();
            List<Vector3> rotations = new List<Vector3>();
            List<Vector3> scales = new List<Vector3>();
            List<string> meshes = new List<string>();
            List<List<string>> materials = new List<List<string>>();

            foreach (var go in gameObjects)
            {
                var renderer = go.GetComponent<MeshRenderer>();
                if (renderer != null)
                    allRenderers.Add(renderer);
            }

            foreach (var renderer in allRenderers)
            {
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter.sharedMesh == null)
                    continue;

                List<string> goMaterials = new List<string>();
                foreach (var material in renderer.sharedMaterials)
                {
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

            AssetChunkData chunkData = AssetBundleUtils.CreateChunkLayoutData(worldName, "chunklayout_" + chunk.ChunkID.X + "_" + chunk.ChunkID.Y + " " + chunk.ChunkID.Z, positions.ToArray(), rotations.ToArray(), scales.ToArray(), meshes.ToArray(), assetMaterials, chunk.ChunkID.ID);
            string chunkDataPath = AssetDatabase.GetAssetPath(chunkData.GetInstanceID());
            AssetImporter.GetAtPath(chunkDataPath).SetAssetBundleNameAndVariant(worldName + "_chunklayout", "");
        }
    }
}


