using StreamDaddy.AssetManagement;
using StreamDaddy.Editor.Chunking;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StreamDaddy.Editor.Assets
{
    public class AssetBuildStrategy : IAssetBuildStrategy
    {
        private Dictionary<string, Mesh> m_uniqueMeshes = new Dictionary<string, Mesh>();
        private Dictionary<string, Material> m_uniqueMaterials = new Dictionary<string, Material>();

        private HashSet<int> m_processedInstanceIDs = new HashSet<int>();

        private void Clear()
        {
            m_uniqueMeshes.Clear();
            m_uniqueMaterials.Clear();
            m_processedInstanceIDs.Clear();
        }

        public void BuildChunkAssets(string worldName, EditorChunk chunk, List<string> assetBundles)
        {
            Clear();
            //  First off, fetch all MeshRenderers, these have the data we want in them, or on their game objects.
            List<MeshRenderer> allRenderers = new List<MeshRenderer>();
            List<BoxCollider> allBoxColliders = new List<BoxCollider>();
            List<SphereCollider> allSphereColliders = new List<SphereCollider>();
            List<MeshCollider> allMeshColliders = new List<MeshCollider>();
            
            //  Change this to use moree than one assebundle later if needed.
            if (!assetBundles.Contains(worldName + "_chunkassets"))
            {
                assetBundles.Add(worldName + "_chunkassets");
            }

            var gameObjects = chunk.GetAllChildren();

            foreach (var go in gameObjects)
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
                    }
                    else
                    {
                        Debug.Log("AssetPath: " + assetPath);
                        AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant(worldName + "_chunkassets", "");
                    }
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="worldName"></param>
        /// <param name="chunk"></param>
        /// <returns>Chunk asset name</returns>
        public string BuildChunkLayout(string worldName, EditorChunk chunk)
        {
            Clear();


            List<MeshData> meshData = new List<MeshData>();
            List<BoxColliderData> boxColliderData = new List<BoxColliderData>();
            List<SphereColliderData> sphereColliderData = new List<SphereColliderData>();
            List<MeshColliderData> meshColliderData = new List<MeshColliderData>();
            
            var gameObjects = chunk.GetAllChildren();
            
            foreach (var go in gameObjects)
            {
                if (m_processedInstanceIDs.Contains(go.GetInstanceID()))
                {
                    continue;
                }

                m_processedInstanceIDs.Add(go.GetInstanceID());

                var renderer = go.GetComponent<MeshRenderer>();
                var boxCollider = go.GetComponent<BoxCollider>();
                var sphereCollider = go.GetComponent<SphereCollider>();
                var meshCollider = go.GetComponent<MeshCollider>();
                
                if (renderer != null)
                {
                    MeshData md = CreateMeshData(renderer);
                    if (md != null)
                    {
                        meshData.Add(md);
                    }
                }

                if (boxCollider != null)
                {
                    BoxColliderData bd = CreateBoxColliderData(boxCollider);
                    if (bd != null)
                    {
                        boxColliderData.Add(bd);
                    }
                }

                if (sphereCollider != null)
                {
                    SphereColliderData sd = CreateSphereColliderData(sphereCollider);
                    if (sd != null)
                    {
                        sphereColliderData.Add(sd);
                    }
                }

                if (meshCollider != null)
                {
                    MeshColliderData mcd = CreateMeshColliderData(meshCollider);
                    if (mcd != null)
                    {
                        meshColliderData.Add(mcd);
                    }
                }
            }

            string chunkAssetName = "chunklayout_" + chunk.ChunkID.X + "_" + chunk.ChunkID.Y + " " + chunk.ChunkID.Z;

            AssetChunkData chunkData = AssetBundleUtils.CreateChunkLayoutData(worldName, chunkAssetName,
                meshData.ToArray(),
                boxColliderData.ToArray(), 
                sphereColliderData.ToArray(), 
                meshColliderData.ToArray(), 
                chunk.ChunkID.ID);

            string chunkDataPath = AssetDatabase.GetAssetPath(chunkData.GetInstanceID());
            AssetImporter.GetAtPath(chunkDataPath).SetAssetBundleNameAndVariant(worldName + "_chunklayout", "");
            return chunkAssetName;
        }

        private MeshData CreateMeshData(MeshRenderer renderer)
        {
            var meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter.sharedMesh == null)
                return null;

            MeshData data = new MeshData();

            List<string> meshMaterials = new List<string>();
            foreach (var material in renderer.sharedMaterials)
            {
                meshMaterials.Add(material.name);
            }

            data.MeshName = meshFilter.sharedMesh.name;
            data.Position = meshFilter.transform.position;
            data.Rotation = meshFilter.transform.rotation.eulerAngles;
            data.Scale = meshFilter.transform.lossyScale;
            data.MaterialNames = meshMaterials.ToArray();

            return data;
        }

        private BoxColliderData CreateBoxColliderData(BoxCollider boxCollider)
        {
            BoxColliderData data = new BoxColliderData();

            data.Center = boxCollider.center;
            data.Size = boxCollider.size;
            data.Position = boxCollider.transform.position;
            data.Rotation = boxCollider.transform.rotation.eulerAngles;
            data.Scale = boxCollider.transform.lossyScale;
            
            return data;
        }

        private SphereColliderData CreateSphereColliderData(SphereCollider sphereCollider)
        {
            SphereColliderData data = new SphereColliderData();

            data.Center = sphereCollider.center;
            data.Radius = sphereCollider.radius;
            data.Position = sphereCollider.transform.position;
            data.Rotation = sphereCollider.transform.rotation.eulerAngles;
            data.Scale = sphereCollider.transform.lossyScale;

            return data;
        }

        private MeshColliderData CreateMeshColliderData(MeshCollider meshCollider)
        {
            MeshColliderData data = new MeshColliderData();

            data.MeshName = meshCollider.sharedMesh.name;
            data.Position = meshCollider.transform.position;
            data.Rotation = meshCollider.transform.rotation.eulerAngles;
            data.Scale = meshCollider.transform.lossyScale;

            return data;
        }
    }
}


