using StreamDaddy.Editor.Utils;
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

        public bool Execute(string worldName, Terrain sourceTerrain, List<Terrain> terrains, Material terrainMaterial)
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

            //  Start off by exporting the splat map.
            Texture2D splatmap = TerrainToMesh.Editor.TerrainToMesh.ExportSplatMap(sourceTerrain);

            //  Save splatmap texture
            string splatPath = EditorPaths.GetTerrainMeshSplatPath(worldName) + "splatmap" + ".asset";
            AssetDatabaseUtils.CreateOrReplaceAsset<Texture2D>(splatmap, splatPath);

            //  Set the splatmap texture on the terrain mesh material
            terrainMaterial.SetTexture("_Control", AssetDatabase.LoadAssetAtPath<Texture2D>(splatPath));

            for (int i = 0; i < terrains.Count; i++)
            {
                Terrain terrain = terrains[i];

                var result = StreamDaddy.TerrainToMesh.Editor.TerrainToMesh.CreateMeshFromTerrain(terrain, sourceTerrain, terrainMaterial);

                //  Save mesh
                string meshPath = EditorPaths.GetTerrainMeshPath(worldName) + result.Mesh.name + ".asset";
                AssetDatabaseUtils.CreateOrReplaceAsset<Mesh>(result.Mesh, meshPath);

                CreateTerrainMeshGameObject(result.Mesh, terrainMaterial, terrain.transform.position);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.StopAssetEditing();

            AssetDatabase.Refresh();


            return true;
        }

        private void CreateTerrainMeshGameObject(Mesh terrainMesh, Material terrainMaterial, Vector3 position)
        {
            GameObject terrainGO = new GameObject(terrainMesh.name, typeof(MeshRenderer), typeof(MeshFilter));

            var renderer = terrainGO.GetComponent<MeshRenderer>();
            var filter = terrainGO.GetComponent<MeshFilter>();

            filter.sharedMesh = terrainMesh;
            renderer.sharedMaterial = terrainMaterial;

            terrainGO.transform.position = position;
        }
    }
}


