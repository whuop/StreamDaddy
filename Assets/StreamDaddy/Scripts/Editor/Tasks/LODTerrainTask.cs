using StreamDaddy.Editor.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StreamDaddy.Editor.Tasks
{
    public class LODTerrainTask : Task
    {
        public class TerrainMeshResult
        {
            public Mesh Mesh;
            public Vector3 Position;
        }

        public class Result
        {
            public List<TerrainMeshResult> LOD1 = new List<TerrainMeshResult>();
            public List<TerrainMeshResult> LOD2 = new List<TerrainMeshResult>();
            public List<TerrainMeshResult> LOD3 = new List<TerrainMeshResult>();
        }

        public LODTerrainTask() : base("LOD Terrain")
        {

        }

        public bool Execute(string worldName, Terrain sourceTerrain, List<Terrain> terrains, Material terrainMaterial, ref Result taskResult)
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

            taskResult = new Result();
                
            AssetDatabase.StartAssetEditing();

            //  Start off by exporting the splat map.
            Texture2D splatmap = TerrainToMesh.Editor.TerrainToMesh.ExportSplatMap(sourceTerrain);

            //  Save splatmap texture
            string splatPath = EditorPaths.GetTerrainMeshSplatPath(worldName) + "splatmap" + ".asset";
            AssetDatabaseUtils.CreateOrReplaceAsset<Texture2D>(splatmap, splatPath);

            //  Set the splatmap texture on the terrain mesh material
            terrainMaterial.SetTexture("_Control", AssetDatabase.LoadAssetAtPath<Texture2D>(splatPath));
            
            //  Create the different LODS
            //  First LOD starts at 50%
            float splitAmount = 0.5f;
            //  Create LOD 1
            for(int lodLevel = 1; lodLevel < 4; lodLevel++)
            {
                for (int i = 0; i < terrains.Count; i++)
                {
                    Terrain terrain = terrains[i];

                    Debug.LogError("Terrain Size: " + terrain.terrainData.size.x + "/" + terrain.terrainData.size.z);
                    
                    int samplesX = Mathf.RoundToInt((float)terrain.terrainData.heightmapWidth * splitAmount);
                    int samplesY = Mathf.RoundToInt((float)terrain.terrainData.heightmapHeight * splitAmount);
                    
                    var mesh = TerrainToMesh.Editor.TerrainToMesh.CreateTerrainMeshWithResolution(sourceTerrain, terrain, samplesX, samplesY);
                    string newMeshName = mesh.name + "_LOD" + lodLevel;
                    mesh.name = newMeshName;
                    //  Save mesh
                    string meshPath = EditorPaths.GetTerrainMeshPath(worldName) + mesh.name + ".asset";
                    AssetDatabaseUtils.CreateOrReplaceAsset<Mesh>(mesh, meshPath);

                    switch(lodLevel)
                    {
                        case 1:
                            taskResult.LOD1.Add(new TerrainMeshResult { Mesh = mesh, Position = terrain.transform.position });
                            break;
                        case 2:
                            taskResult.LOD2.Add(new TerrainMeshResult { Mesh = mesh, Position = terrain.transform.position });
                            break;
                        case 3:
                            taskResult.LOD3.Add(new TerrainMeshResult { Mesh = mesh, Position = terrain.transform.position });
                            break;
                    }

                    //CreateTerrainMeshGameObject(mesh, terrainMaterial, terrain.transform.position);
                }
                //  Decrease amount of samples for each loop
                splitAmount *= 0.5f;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.StopAssetEditing();

            AssetDatabase.Refresh();


            return true;
        }

        private void CreateTerrainMeshGameObject(Mesh terrainMesh, Material terrainMaterial, Vector3 position, string postfix = "")
        {
            GameObject terrainGO = new GameObject(terrainMesh.name + postfix, typeof(MeshRenderer), typeof(MeshFilter));

            var renderer = terrainGO.GetComponent<MeshRenderer>();
            var filter = terrainGO.GetComponent<MeshFilter>();

            filter.sharedMesh = terrainMesh;
            renderer.sharedMaterial = terrainMaterial;

            terrainGO.transform.position = position;
        }
    }
}


