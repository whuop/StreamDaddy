using StreamDaddy.AssetManagement;
using StreamDaddy.Chunking;
using StreamDaddy.Editor.Chunking;
using StreamDaddy.Editor.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
            public List<string> AssetBundles;
        }
        
        public BuildChunkLayoutTask() : base("Build Chunk Layouts")
        {

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
            result.AssetBundles = new List<string>();

            string chunkLayoutGroupName = worldName + "ChunkLayout";
            string chunkAssetGroupName = worldName + "ChunkAssets";

            result.AssetBundles.Add(chunkAssetGroupName);
            
            //  The name of the asset bundle containing all the layouts for the chunks
            result.ChunkLayoutBundle = chunkLayoutGroupName;
            //  Create the addressables group for the chunk layout object
            var assetSettings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;

            //  Create Addressables labels
            assetSettings.AddLabel(chunkLayoutGroupName);
            assetSettings.AddLabel(chunkAssetGroupName);

            AddressableAssetGroup chunkLayoutGroup;
            AddressableAssetGroup chunkAssetsGroup;
            try
            {
                chunkLayoutGroup = assetSettings.groups.Single(g => g.Name == chunkLayoutGroupName);
            }
            catch(Exception e)
            {
                chunkLayoutGroup = assetSettings.CreateGroup(chunkLayoutGroupName, false, false, true);
            }

            try
            {
                chunkAssetsGroup = assetSettings.groups.Single(g => g.Name == chunkAssetGroupName);
            }
            catch(Exception e)
            {
                chunkAssetsGroup = assetSettings.CreateGroup(chunkAssetGroupName, false, false, true);
            }
            
            for (int i = 0; i < chunks.Count; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Exporting chunk layout", "Chunk " + i, i / chunks.Count))
                    return false;
                List<MeshData> meshData = new List<MeshData>();
                List<BoxColliderData> boxColliderData = new List<BoxColliderData>();
                List<SphereColliderData> sphereColliderData = new List<SphereColliderData>();
                List<MeshColliderData> meshColliderData = new List<MeshColliderData>();

                var chunk = chunks[i];
                var meshFilters = chunk.MeshFilters;
                var colliders = chunk.Colliders;

                //  Export mesh assets to an Addressable group
                foreach(var filter in meshFilters)
                {
                    var mesh = filter.sharedMesh;
                    if (mesh == null)
                        continue;

                    var renderer = filter.gameObject.GetComponent<MeshRenderer>();
                    if (renderer == null)
                        continue;

                    var materials = renderer.sharedMaterials;
                    
                    string assetPath = AssetDatabase.GetAssetPath(mesh.GetInstanceID());
                    string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
                    var entry = assetSettings.CreateOrMoveEntry(assetGuid, chunkAssetsGroup);
                    entry.SetLabel(chunkAssetGroupName, true);

                    Debug.Log(string.Format("Adding Mesh {0} with address {1}", mesh.name, entry.address));

                    List<string> materialAddresses = new List<string>();
                    //  Export all materials associated with the Mesh asset to an addressable group.
                    foreach(var mat in materials)
                    {
                        string matPath = AssetDatabase.GetAssetPath(mat.GetInstanceID());
                        string matGuid = AssetDatabase.AssetPathToGUID(matPath);
                        var matEntry = assetSettings.CreateOrMoveEntry(matGuid, chunkAssetsGroup);
                        materialAddresses.Add(matEntry.address);
                        //  Set mesh label
                        matEntry.SetLabel(chunkAssetGroupName, true);

                        Debug.Log(string.Format("Adding Material {0} with address {1}", mat.name, matEntry.address));
                    }
                    var md = CreateMeshData(entry.address, materialAddresses.ToArray(), filter.transform.position, filter.transform.rotation.eulerAngles, filter.transform.lossyScale);
                    meshData.Add(md);
                }

                //  Export colliders
                foreach(var collider in colliders)
                {
                    if (collider.GetType().IsAssignableFrom(typeof(BoxCollider)))
                    {
                        BoxCollider box = (BoxCollider)collider;
                        var bd = CreateBoxColliderData(box);
                        boxColliderData.Add(bd);
                    }
                    else if (collider.GetType().IsAssignableFrom(typeof(SphereCollider)))
                    {
                        SphereCollider sphere = (SphereCollider)collider;
                        var sd = CreateSphereColliderData(sphere);
                        sphereColliderData.Add(sd);
                    }
                    else if (collider.GetType().IsAssignableFrom(typeof(MeshCollider)))
                    {
                        MeshCollider meshCol = (MeshCollider)collider;

                        string assetPath = AssetDatabase.GetAssetPath(meshCol.sharedMesh.GetInstanceID());
                        string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
                        var entry = assetSettings.CreateOrMoveEntry(assetGuid, chunkAssetsGroup);
                        //  Set mesh label
                        entry.SetLabel(chunkAssetGroupName, true);

                        var md = CreateMeshColliderData(entry.address, meshCol.gameObject.transform.position, meshCol.gameObject.transform.rotation.eulerAngles, meshCol.gameObject.transform.lossyScale);
                        meshColliderData.Add(md);
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

                
                //  Add the chunk layout to the asset group for this world
                string chunkDataPath = AssetDatabase.GetAssetPath(chunkLayout.GetInstanceID());
                string guid = AssetDatabase.AssetPathToGUID(chunkDataPath);

                var assetEntry = assetSettings.CreateOrMoveEntry(guid, chunkLayoutGroup, false, true);
                //  Set layout asset label
                assetEntry.SetLabel(chunkLayoutGroupName, true);
            }

            EditorUtility.ClearProgressBar();

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

        private MeshData CreateMeshData(string meshAddress, string[] materialAddresses, Vector3 position, Vector3 rotation, Vector3 scale)
        {            
            MeshData data = new MeshData();

            data.MeshAddress = meshAddress;
            data.MaterialAddresses = materialAddresses;
            data.Position = position;
            data.Rotation = rotation;
            data.Scale = scale;

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

        private MeshColliderData CreateMeshColliderData(string meshAddress, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            MeshColliderData data = new MeshColliderData();

            data.MeshAddress = meshAddress;
            data.Position = position;
            data.Rotation = rotation;
            data.Scale = scale;

            return data;
        }

    }
}

