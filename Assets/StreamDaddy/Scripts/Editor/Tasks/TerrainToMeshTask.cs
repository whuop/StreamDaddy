using StreamDaddy.Editor.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StreamDaddy.Editor.Tasks
{
    public class TerrainToMeshTask : Task
    {
        public TerrainToMeshTask() : base("TerrainToMesh")
        {

        }

        public bool Execute(string worldName, List<Terrain> terrains, Material terrainMaterial)
        {
            if (terrains.Count == 0)
            {
                Debug.LogError(string.Format("TerrainToMesh terrain count is 0, must be given more than 0 Terrains to process."));
                return false;
            }

            if (terrainMaterial == null)
            {
                Debug.LogError(string.Format("TerrainToMesh mesh material is null. Needs non null material!"));
                return false;
            }

            if (string.IsNullOrEmpty(worldName))
            {
                Debug.LogError("TerrainToMesh world name can't be null or empty!");
                return false;
            }
                

            AssetDatabase.StartAssetEditing();

            for(int i = 0; i < terrains.Count; i++)
            {
                Terrain terrain = terrains[i];

                var result = StreamDaddy.TerrainToMesh.Editor.TerrainToMesh.CreateMeshFromTerrain(terrain, terrainMaterial);

                //  Save mesh
                string meshPath = EditorPaths.GetTerrainMeshPath(worldName) + result.Mesh.name + ".asset";
                AssetDatabaseUtils.CreateOrReplaceAsset<Mesh>(result.Mesh, meshPath);

                //  Save control splat texture
                string splatPath = EditorPaths.GetTerrainMeshSplatPath(worldName) + result.Mesh.name + ".asset";
                AssetDatabaseUtils.CreateOrReplaceAsset<Texture2D>(result.Splat, splatPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.StopAssetEditing();
            

            return true;
        }
    }
}


