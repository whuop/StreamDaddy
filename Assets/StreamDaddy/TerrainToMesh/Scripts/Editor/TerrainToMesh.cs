using StreamDaddy.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StreamDaddy.TerrainToMesh.Editor
{
    public static class TerrainToMesh
    {
        public struct TerrainToMeshResult
        {
            public Mesh Mesh;
            public Texture2D Splat;
        }

        public static TerrainToMeshResult CreateMeshFromTerrain(Terrain terrain, Material terrainMaterial)
        {
            TerrainData terrainData = terrain.terrainData;

            int terrainWidth = terrainData.heightmapWidth;
            int terrainHeight = terrainData.heightmapHeight;

            float[,] heights = terrainData.GetHeights(0, 0, terrainWidth, terrainHeight);
            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();

            CopySplatReferencesToMaterial(terrainData, terrainMaterial);
            Texture2D splatMap = CreateSplatMap(terrainData);

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
            for (int i = 0; i < len; i++)
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

            GenerateControlUVs(terrainData, mesh);
            mesh.RecalculateNormals();

            var result = new TerrainToMeshResult();
            result.Mesh = mesh;
            result.Splat = splatMap;

            return result;
        }

        /// <summary>
        /// Creates the splat map for given TerrainData. 
        /// Splat map contains data in the rgba channels that says which splat texture should be rendered where.
        /// </summary>
        /// <param name="td"></param>
        /// <returns></returns>
        private static Texture2D CreateSplatMap(TerrainData td)
        {
            int textureWidth = td.alphamapWidth;
            int textureHeight = td.alphamapHeight;
            float[,,] alphaMaps = td.GetAlphamaps(0, 0, textureWidth, textureHeight);
            int numLayers = td.alphamapLayers;
            Texture2D controlTexture = new Texture2D(textureWidth, textureHeight);
            for (int x = 0; x < textureWidth; x++)
            {
                for (int y = 0; y < textureHeight; y++)
                {
                    Color color = new Color(0, 0, 0, 0);
                    for (int layer = 0; layer < numLayers; layer++)
                    {
                        switch (layer)
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
            return controlTexture;
        }

        /// <summary>
        /// Copies splat texture references from source TerrainData to MeshTerrain material.
        /// </summary>
        /// <param name="td"></param>
        /// <param name="material"></param>
        private static void CopySplatReferencesToMaterial(TerrainData td, Material material)
        {
            int textureWidth = td.alphamapWidth;
            int textureHeight = td.alphamapHeight;
            Texture2D[] textures = td.alphamapTextures;
            SplatPrototype[] splats = td.splatPrototypes;

            for (int i = 0; i < splats.Length; i++)
            {
                if (i == 0)
                {
                    material.SetTexture("_Splat0", splats[i].texture);
                    material.SetTextureScale("_Splat0", splats[i].tileSize);
                }
                else if (i == 1)
                {
                    material.SetTexture("_Splat1", splats[i].texture);
                    material.SetTextureScale("_Splat1", splats[i].tileSize);
                }
                else if (i == 2)
                {
                    material.SetTexture("_Splat2", splats[i].texture);
                    material.SetTextureScale("_Splat2", splats[i].tileSize);
                }
                else if (i == 3)
                {
                    material.SetTexture("_Splat3", splats[i].texture);
                    material.SetTextureScale("_Splat3", splats[i].tileSize);
                }
            }
        }

        /// <summary>
        /// Generates UVs for the splat map.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="terrainMesh"></param>
        private static void GenerateControlUVs(TerrainData data, Mesh terrainMesh)
        {
            int terrainWidth = data.heightmapWidth;
            int terrainHeight = data.heightmapHeight;

            float sampleWidth = 1.0f / (float)terrainWidth;
            float sampleHeight = 1.0f / (float)terrainHeight;

            List<Vector2> uvs = new List<Vector2>();

            for (int y = 0; y < terrainHeight; y++)
            {
                for (int x = 0; x < terrainWidth; x++)
                {
                    uvs.Add(new Vector2(x * sampleWidth, y * sampleHeight));
                }
            }

            terrainMesh.SetUVs(0, uvs);
        }
    }

}

