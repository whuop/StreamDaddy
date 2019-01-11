using StreamDaddy.AssetManagement;
using StreamDaddy.Chunking;
using StreamDaddy.Editor.Chunking;
using StreamDaddy.Editor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Landfall.Editor;
using static StreamDaddy.Editor.Tasks.GenerateMeshLodsTask;

namespace StreamDaddy.Editor.Tasks
{
    public class BuildChunkLayoutTask : Task
    {
        public static string WORLD_NAME_ARG = "worldname";
        public static string CHUNKS_ARG = "chunks";
        
        public struct BuildChunkLayoutResult
        {
            public List<AssetReference> ChunkLayoutReferences;
            public string ChunkLayoutBundle;
            public List<string> AssetBundles;
        }
        
        public BuildChunkLayoutTask() : base("Build Chunk Layouts")
        {

        }
        
        public bool Execute(string worldName, LodFormat lodFormat, List<EditorChunk> chunks, LODTerrainTask.Result terrains, ref BuildChunkLayoutResult result)
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
            
            result.ChunkLayoutReferences = new List<AssetReference>();
            result.AssetBundles = new List<string>();

            string chunkLayoutGroupName = worldName + "ChunkLayout";
            string chunkAssetGroupName = worldName + "ChunkAssets";

            result.AssetBundles.Add(chunkAssetGroupName);
            
            //  The name of the asset bundle containing all the layouts for the chunks
            result.ChunkLayoutBundle = chunkLayoutGroupName;
            //  Create the addressables group for the chunk layout object
            var assetSettings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;

            var chunkLayoutGroup = CreateAddressablesGroup(chunkLayoutGroupName, assetSettings);
            var chunkAssetsGroup = CreateAddressablesGroup(chunkAssetGroupName, assetSettings);
            
            for (int i = 0; i < chunks.Count; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Exporting chunk layout", "Chunk " + i, i / chunks.Count))
                    return false;

                List<MeshLayerData> meshLayers = new List<MeshLayerData>();
                
                //  Add all the 4 LOD levels, 0, 1, 2 and 3.
                meshLayers.Add(new MeshLayerData());
                meshLayers.Add(new MeshLayerData());
                meshLayers.Add(new MeshLayerData());
                meshLayers.Add(new MeshLayerData());
                //  Create the Lists holding the different lod levels that are then
                // assigned to the corresponding MeshLayer
                List<MeshData> lod0 = new List<MeshData>();
                List<MeshData> lod1 = new List<MeshData>();
                List<MeshData> lod2 = new List<MeshData>();
                List<MeshData> lod3 = new List<MeshData>();
                //  Create the list holding the transforms for the lod level meshes.
                //  The different lod levels all share the same transform, so the ordering of the meshes in the 
                //  different lod levels is very important for things to be placed correctly in the world.
                List<TransformData> meshTransforms = new List<TransformData>();
                List<MaterialData> meshMaterials = new List<MaterialData>();
                
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

                    ExportMesh(mesh, lodFormat, assetSettings, chunkAssetsGroup,
                        lod0, lod1, lod2, lod3);
                    ExportTransform(filter.transform, meshTransforms);
                    ExportMaterials(renderer, assetSettings, chunkAssetsGroup, meshMaterials);
                }

                //  Add the different LODs to the different layers
                meshLayers[0].Meshes = lod0.ToArray();
                meshLayers[1].Meshes = lod1.ToArray();
                meshLayers[2].Meshes = lod2.ToArray();
                meshLayers[3].Meshes = lod3.ToArray();

                //  Create the 4 different LOD level layers for the mesh colliders, 0, 1, 2, 3
                List<MeshLayerData> meshColliderLayers = new List<MeshLayerData>();
                meshColliderLayers.Add(new MeshLayerData());
                meshColliderLayers.Add(new MeshLayerData());
                meshColliderLayers.Add(new MeshLayerData());
                meshColliderLayers.Add(new MeshLayerData());
                //  Create the list hold the transfors for the mesh collider lods
                List<TransformData> meshColliderTransforms = new List<TransformData>();
                //  Create the different lists of the lodded meshes for the MeshColliderLayers
                List<MeshData> cLod0 = new List<MeshData>();
                List<MeshData> cLod1 = new List<MeshData>();
                List<MeshData> cLod2 = new List<MeshData>();
                List<MeshData> cLod3 = new List<MeshData>();

                List<BoxColliderData> boxColliderData = new List<BoxColliderData>();
                List<SphereColliderData> sphereColliderData = new List<SphereColliderData>();

                //  Export colliders
                foreach (var collider in colliders)
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
                        Mesh mesh = meshCol.sharedMesh;

                        if (mesh == null)
                            continue;

                        ExportMesh(mesh, lodFormat, assetSettings, chunkAssetsGroup, cLod0, cLod1, lod2, lod3);
                        ExportTransform(meshCol.transform, meshColliderTransforms);
                    }
                }

                //  Add the different LODs to the different layers
                meshColliderLayers[0].Meshes = cLod0.ToArray();
                meshColliderLayers[1].Meshes = cLod1.ToArray();
                meshColliderLayers[2].Meshes = cLod2.ToArray();
                meshColliderLayers[3].Meshes = cLod3.ToArray();

                //  Create the list that holds all the terrain meshes
                List<TerrainMeshData> terrainMeshes = new List<TerrainMeshData>();
                foreach(var t in terrains.LOD1)
                {
                    string assetPath = AssetDatabase.GetAssetPath(t.Mesh.GetInstanceID());
                    string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
                    var meshReference = new AssetReference(assetGuid);
                    var terrainData = CreateTerrainMeshData(meshReference, t.Position);
                    terrainMeshes.Add(terrainData);

                    assetSettings.CreateOrMoveEntry(assetGuid, chunkAssetsGroup);
                }

                foreach (var t in terrains.LOD2)
                {
                    string assetPath = AssetDatabase.GetAssetPath(t.Mesh.GetInstanceID());
                    string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
                    var meshReference = new AssetReference(assetGuid);
                    var terrainData = CreateTerrainMeshData(meshReference, t.Position);
                    terrainMeshes.Add(terrainData);

                    assetSettings.CreateOrMoveEntry(assetGuid, chunkAssetsGroup);
                }

                foreach (var t in terrains.LOD3)
                {
                    string assetPath = AssetDatabase.GetAssetPath(t.Mesh.GetInstanceID());
                    string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
                    var meshReference = new AssetReference(assetGuid);
                    var terrainData = CreateTerrainMeshData(meshReference, t.Position);
                    terrainMeshes.Add(terrainData);

                    assetSettings.CreateOrMoveEntry(assetGuid, chunkAssetsGroup);
                }

                string chunkAssetName = "chunklayout_" + chunk.ChunkID.X + "_" + chunk.ChunkID.Y + "_" + chunk.ChunkID.Z;
                
                //  Create the scriptable object for this chunk layout
                var chunkLayout = CreateChunkLayout(meshLayers.ToArray(), meshMaterials.ToArray(), meshTransforms.ToArray(), boxColliderData.ToArray(), sphereColliderData.ToArray(), meshColliderLayers.ToArray(), meshColliderTransforms.ToArray(), terrainMeshes.ToArray(), chunk.ChunkID);

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
                
                //  Add layout to the Addressables layout group
                var assetEntry = assetSettings.CreateOrMoveEntry(guid, chunkLayoutGroup, false, true);

                //  Create a reference to the layout asset
                result.ChunkLayoutReferences.Add(new AssetReference(guid));
            }

            EditorUtility.ClearProgressBar();

            return true;
        }

        private void ExportMesh(Mesh mesh, LodFormat lodFormat, AddressableAssetSettings assetSettings, AddressableAssetGroup chunkAssetsGroup,
                                List<MeshData> lod0, List<MeshData> lod1, List<MeshData> lod2, List<MeshData> lod3)
        {
            string assetPath = AssetDatabase.GetAssetPath(mesh.GetInstanceID());
            string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            string fileFormat = "." + PathUtils.ExtractFileFormatFromPath(assetPath);

            //  Create the directory path
            assetPath = assetPath.Replace("Assets/", "Assets/GeneratedLODS/");
            assetPath = assetPath.Replace(fileFormat, "") + "/";
            //  Add file name
            assetPath = assetPath + mesh.name;
            
            string lod0Path = assetPath + "_LOD0.asset";
            string lod1Path = assetPath + "_LOD1.asset";
            string lod2Path = assetPath + "_LOD2.asset";
            string lod3Path = assetPath + "_LOD3.asset";

            Mesh mlod0 = AssetDatabase.LoadAssetAtPath<Mesh>(lod0Path);
            Mesh mlod1 = AssetDatabase.LoadAssetAtPath<Mesh>(lod1Path);
            Mesh mlod2 = AssetDatabase.LoadAssetAtPath<Mesh>(lod2Path);
            Mesh mlod3 = AssetDatabase.LoadAssetAtPath<Mesh>(lod3Path);

            if (mlod0 == null)
                LogError("Can't find LOD0 for mesh " + assetPath + " at path " + lod0Path);
            if (mlod1 == null)
                LogError("Can't find LOD1 for mesh " + assetPath + " at path " + lod1Path);
            if (mlod2 == null)
                LogError("Can't find LOD2 for mesh " + assetPath + " at path " + lod2Path);
            if (mlod3 == null)
                LogError("Can't find LOD3 for mesh " + assetPath + " at path " + lod3Path);

            string lod0Guid = AssetDatabase.AssetPathToGUID(lod0Path);
            string lod1Guid = AssetDatabase.AssetPathToGUID(lod1Path);
            string lod2Guid = AssetDatabase.AssetPathToGUID(lod2Path);
            string lod3Guid = AssetDatabase.AssetPathToGUID(lod3Path);

            var lod0Entry = assetSettings.CreateOrMoveEntry(lod0Guid, chunkAssetsGroup);
            var lod1Entry = assetSettings.CreateOrMoveEntry(lod1Guid, chunkAssetsGroup);
            var lod2Entry = assetSettings.CreateOrMoveEntry(lod2Guid, chunkAssetsGroup);
            var lod3Entry = assetSettings.CreateOrMoveEntry(lod3Guid, chunkAssetsGroup);
            
            var lod0Ref = new AssetReference(lod0Guid);
            var lod1Ref = new AssetReference(lod1Guid);
            var lod2Ref = new AssetReference(lod2Guid);
            var lod3Ref = new AssetReference(lod3Guid);

            var lod0md = CreateMeshData(lod0Ref);
            var lod1md = CreateMeshData(lod1Ref);
            var lod2md = CreateMeshData(lod2Ref);
            var lod3md = CreateMeshData(lod3Ref);

            lod0.Add(lod0md);
            lod1.Add(lod1md);
            lod2.Add(lod2md);
            lod3.Add(lod3md);
        }

        private void ExportTransform(Transform transform, List<TransformData> meshTransforms)
        {
            var td = CreateTransformData(transform.position, transform.rotation, transform.lossyScale);
            meshTransforms.Add(td);
        }

        private void ExportMaterials(MeshRenderer renderer, AddressableAssetSettings assetSettings, AddressableAssetGroup chunkAssetsGroup, List<MaterialData> meshMaterials)
        {
            var materials = renderer.sharedMaterials;
            List<AssetReference> materialReferences = new List<AssetReference>();
            //  Export all materials associated with the Mesh asset to an addressable group.
            foreach (var mat in materials)
            {
                string matPath = AssetDatabase.GetAssetPath(mat.GetInstanceID());
                string matGuid = AssetDatabase.AssetPathToGUID(matPath);
                //  Add material to Assets Addressables group
                var matEntry = assetSettings.CreateOrMoveEntry(matGuid, chunkAssetsGroup);
                materialReferences.Add(new AssetReference(matEntry.guid));

                Debug.Log(string.Format("Adding Material {0} with address {1}", mat.name, matEntry.address));
            }

            var md = CreateMaterialData(materialReferences.ToArray());
            meshMaterials.Add(md);
        }

        private AddressableAssetGroup CreateAddressablesGroup(string groupName, AddressableAssetSettings settings)
        {
            AddressableAssetGroup assetGroup;
            try
            {
                assetGroup = settings.groups.Single(g => g.Name == groupName);
            }
            catch (Exception e)
            {
                assetGroup = settings.CreateGroup(groupName, false, false, true, new List<AddressableAssetGroupSchema>());
                //assetGroup = settings.CreateGroup(groupName, false, false, true);
            }
            return assetGroup;
        }

        private TransformData CreateTransformData(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            TransformData data = new TransformData
            {
                Position = position,
                Rotation = rotation.eulerAngles,
                Scale = scale
            };
            return data;
        }

        private AssetChunkData CreateChunkLayout(MeshLayerData[] meshLayers, MaterialData[] meshMaterials, TransformData[] meshTransforms, BoxColliderData[] boxColliders, SphereColliderData[] sphereColliders, MeshLayerData[] meshColliderLayers, TransformData[] meshColliderTransforms, TerrainMeshData[] terrainMeshes, ChunkID chunkID)
        {
            AssetChunkData asset = ScriptableObject.CreateInstance<AssetChunkData>();
            asset.MeshLayers = meshLayers;
            asset.MeshMaterials = meshMaterials;
            asset.MeshTransforms = meshTransforms;
            asset.BoxColliders = boxColliders;
            asset.SphereColliders = sphereColliders;
            asset.MeshColliderLayers = meshColliderLayers;
            asset.MeshColliderTransforms = meshColliderTransforms;
            asset.TerrainMeshes = terrainMeshes;
            asset.ChunkID = chunkID.ID;

            return asset;
        }

        private void SaveChunkLayout(string worldName, string chunkAssetName, AssetChunkData chunkLayout)
        {
            string path = EditorPaths.GetWorldChunkLayoutPath(worldName);
            PathUtils.EnsurePathExists(path);
            AssetDatabaseUtils.CreateOrReplaceAsset(chunkLayout, path + chunkAssetName + ".asset");
        }

        private MeshData CreateMeshData(AssetReference meshReference)
        {            
            MeshData data = new MeshData();
            data.MeshReference = meshReference;
            
            return data;
        }

        private MaterialData CreateMaterialData(AssetReference[] materialReferences)
        {
            MaterialData data = new MaterialData();
            data.MaterialReferences = materialReferences;
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

        private TerrainMeshData CreateTerrainMeshData(AssetReference meshReference, Vector3 position)
        {
            TerrainMeshData data = new TerrainMeshData();

            data.MeshReference = meshReference;
            data.Position = position;

            return data;
        }
    }
}

