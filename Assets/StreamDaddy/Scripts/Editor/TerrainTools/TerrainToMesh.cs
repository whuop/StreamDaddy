using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class TerrainToMesh
{
    public static void CreateMeshFromTerrain(Terrain terrain, Material terrainMaterial)
    {
        TerrainData terrainData = terrain.terrainData;


        int terrainWidth = terrainData.heightmapWidth;
        int terrainHeight = terrainData.heightmapHeight;

        float[,] heights = terrainData.GetHeights(0, 0, terrainWidth, terrainHeight);
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        CreateAlphaMap(terrainData, terrainMaterial);

        //  Extract the vertices of the terrain into a 1-dimensional array.
        for (int z = 0; z < terrainHeight; z++)
        {
            for (int x = 0; x < terrainWidth; x++)
            {
                Vector3 vertex = new Vector3(z * terrainData.heightmapScale.z, heights[x, z] * terrainData.heightmapScale.y, x * terrainData.heightmapScale.x);
                vertices.Add(vertex);
            }
        }
        
        int len = vertices.Count - terrainWidth - 1;
        for(int i = 0; i < len; i++)
        {
            if ((i + 1) % terrainWidth == 0)
                continue;
            int tv0 = i;
            int tv1 = i + 1;
            int tv2 = i + terrainWidth;
            
            int vv0 = i + 1;
            int vv1 = i + terrainWidth + 1;
            int vv2 = tv2;

            indices.Add(tv0);
            indices.Add(tv1);
            indices.Add(tv2);

            indices.Add(vv0);
            indices.Add(vv1);
            indices.Add(vv2);
            
        }

        Mesh mesh = new Mesh();
        mesh.name = terrain.gameObject.name;
        mesh.SetVertices(new List<Vector3>(vertices));
        mesh.SetTriangles(indices, 0);

        GameObject go = new GameObject(terrain.gameObject.name + "_mesh", typeof(MeshRenderer), typeof(MeshFilter));

        MeshFilter filter = go.GetComponent<MeshFilter>();
        MeshRenderer renderer = go.GetComponent<MeshRenderer>();

        filter.sharedMesh = mesh;
        renderer.sharedMaterial = terrainMaterial;
    }

    private static void CreateAlphaMap(TerrainData td, Material material)
    {
        int textureWidth = td.alphamapWidth;
        int textureHeight = td.alphamapHeight;
        int numLayers = td.alphamapLayers;
        float[,,] alphaMaps = td.GetAlphamaps(0, 0, textureWidth, textureHeight);
        Texture2D[] textures = td.alphamapTextures;
        SplatPrototype[] splats = td.splatPrototypes;

        Texture2D controlTexture = new Texture2D(textureWidth, textureHeight);
        
        for(int x = 0; x < textureWidth; x++)
        {
            for(int y = 0; y < textureHeight; y++)
            {
                Color color = new Color(0, 0, 0, 0);
                for(int layer = 0; layer < numLayers; layer++)
                {
                    switch(layer)
                    {
                        case 0:
                            color.r = alphaMaps[x, y, layer];
                            break;
                        case 1:
                            color.g = alphaMaps[x, y, layer];
                            break;
                        case 2:
                            color.b = alphaMaps[x, y, layer];
                            break;
                        case 3:
                            color.a = alphaMaps[x, y, layer];
                            break;
                    }
                }
                controlTexture.SetPixel(x, y, color);
            }
        }

        AssetDatabase.CreateAsset(controlTexture, "Assets/controltexture.asset");

        material.SetTexture("_Control", controlTexture);
        for(int i = 0; i < splats.Length; i++)
        {
            if (i == 0)
            {
                material.SetTexture("_Splat0", splats[i].texture);
            }
            else if (i == 1)
            {
                material.SetTexture("_Splat1", splats[i].texture);
            }
            else if (i == 2)
            {
                material.SetTexture("_Splat2", splats[i].texture);
            }
            else if (i == 3)
            {
                material.SetTexture("_Splat3", splats[i].texture);
            }
        }
    }
}
