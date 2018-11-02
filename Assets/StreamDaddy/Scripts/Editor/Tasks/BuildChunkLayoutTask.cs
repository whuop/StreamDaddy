using StreamDaddy.AssetManagement;
using StreamDaddy.Chunking;
using StreamDaddy.Editor.Chunking;
using StreamDaddy.Editor.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StreamDaddy.Editor.Tasks
{
    public class BuildChunkLayoutTask : Task
    {
        public static string WORLD_NAME_ARG = "worldname";
        public static string CHUNKS_ARG = "chunks";
        
        public struct BuildChunkLayoutResult
        {
            public List<string> ChunkLayoutNames;
            public string ChunkLayoutBundle;
        }

        public BuildChunkLayoutTask() : base("Build Chunk Layouts")
        {

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

        public bool Execute(string worldName, List<EditorChunk> chunks, ref BuildChunkLayoutResult result)
        {
            if (chunks == null || chunks.Count == 0)
            {
                LogError("Chunks must not be null and must have a size that is larger than 0. Task failed!");
                return false;
            }

            if (string.IsNullOrEmpty(worldName))
            {
                LogError("World Name is either null or empty. Task failed!");
                return false;
            }
            
            result.ChunkLayoutNames = new List<string>();

            HashSet<int> processedInstanceIDs = new HashSet<int>();

            List<MeshData> meshData = new List<MeshData>();
            List<BoxColliderData> boxColliderData = new List<BoxColliderData>();
            List<SphereColliderData> sphereColliderData = new List<SphereColliderData>();
            List<MeshColliderData> meshColliderData = new List<MeshColliderData>();

            //  The name of the asset bundle containing all the layouts for the chunks
            result.ChunkLayoutBundle = worldName + "_chunklayout";
            
            for(int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                var gameObjects = chunk.GetAllChildren();

                for(int j = 0; j < gameObjects.Length; j++)
                {
                    var go = gameObjects[j];
                    if (processedInstanceIDs.Contains(go.GetInstanceID()))
                    {
                        continue;
                    }

                    var renderer = go.GetComponent<MeshRenderer>();
                    var boxCollider = go.GetComponent<BoxCollider>();
                    var sphereCollider = go.GetComponent<SphereCollider>();
                    var meshCollider = go.GetComponent<MeshCollider>();

                    if (renderer != null)
                    {
                        MeshData md = CreateMeshData(renderer);
                        meshData.Add(md);
                    }

                    if (boxCollider != null)
                    {
                        BoxColliderData bd = CreateBoxColliderData(boxCollider);
                        boxColliderData.Add(bd);
                    }

                    if (sphereCollider != null)
                    {
                        SphereColliderData sd = CreateSphereColliderData(sphereCollider);
                        sphereColliderData.Add(sd);
                    }

                    if (meshCollider != null)
                    {
                        MeshColliderData mcd = CreateMeshColliderData(meshCollider);
                        meshColliderData.Add(mcd);
                    }
                }

                string chunkAssetName = "chunklayout_" + chunk.ChunkID.X + "_" + chunk.ChunkID.Y + "_" + chunk.ChunkID.Z;
                result.ChunkLayoutNames.Add(chunkAssetName);

                //  Create the scriptable object for this chunk layout
                var chunkLayout = CreateChunkLayout(meshData.ToArray(), boxColliderData.ToArray(), sphereColliderData.ToArray(), meshColliderData.ToArray(), chunk.ChunkID);

                AssetDatabase.StartAssetEditing();
                SaveChunkLayout(worldName, chunkAssetName, chunkLayout);
                EditorUtility.SetDirty(chunkLayout);
                AssetDatabase.SaveAssets();
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

                string path = EditorPaths.GetWorldChunkLayoutPath(worldName) + chunkAssetName + ".asset";
                chunkLayout = AssetDatabase.LoadAssetAtPath<AssetChunkData>(path);

                //  Set the asset bundle
                string chunkDataPath = AssetDatabase.GetAssetPath(chunkLayout.GetInstanceID());
                AssetImporter.GetAtPath(chunkDataPath).SetAssetBundleNameAndVariant(result.ChunkLayoutBundle, "");
            }
            
            return true;
        }

        private AssetChunkData CreateChunkLayout(MeshData[] meshes, BoxColliderData[] boxColliders, SphereColliderData[] sphereColliders, MeshColliderData[] meshColliders, ChunkID chunkID)
        {
            AssetChunkData asset = ScriptableObject.CreateInstance<AssetChunkData>();
            asset.Meshes = meshes;
            asset.BoxColliders = boxColliders;
            asset.SphereColliders = sphereColliders;
            asset.MeshColliders = meshColliders;
            asset.ChunkID = chunkID.ID;

            return asset;
        }

        private void SaveChunkLayout(string worldName, string chunkAssetName, AssetChunkData chunkLayout)
        {
            string path = EditorPaths.GetWorldChunkLayoutPath(worldName);
            PathUtils.EnsurePathExists(path);
            AssetDatabaseUtils.CreateOrReplaceAsset(chunkLayout, path + chunkAssetName + ".asset");
        }

    }
}

