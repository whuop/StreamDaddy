using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.TerrainToMesh.Editor
{
    public static class TerrainToMesh
    {
        public struct TerrainToMeshResult
        {
            public Mesh Mesh;
        }

        public static TerrainToMeshResult CreateMeshFromTerrain(Terrain terrain, Terrain sourceTerrain,Material terrainMaterial)
        {
            TerrainData terrainData = terrain.terrainData;

            int terrainWidth = terrainData.heightmapWidth;
            int terrainHeight = terrainData.heightmapHeight;

            float[,] heights = terrainData.GetHeights(0, 0, terrainWidth, terrainHeight);
            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();

            CopySplatReferencesToMaterial(terrainData, terrainMaterial);

            

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

            GenerateControlUVs(terrainData, sourceTerrain, mesh);
            mesh.RecalculateNormals();

            var result = new TerrainToMeshResult();
            result.Mesh = mesh;
            
            return result;
        }

        public static Texture2D ExportSplatMap(Terrain terrain)
        {
            TerrainData td = terrain.terrainData;
            int textureWidth = td.alphamapWidth;
            int textureHeight = td.alphamapHeight;
            float[,,] alphaMaps = td.GetAlphamaps(0, 0, textureWidth, textureHeight);
            int numLayers = td.alphamapLayers;

            Texture2D splatmap = new Texture2D(textureWidth, textureHeight);
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
                    splatmap.SetPixel(x, y, color);
                }
            }
            return splatmap;
        }

        public static Mesh CreateTerrainMeshWithResolution(Terrain sourceTerrain, Terrain terrain, int samplesX, int samplesZ)
        {
            float terrainWorldWidth = terrain.terrainData.size.x;
            float terrainWorldHeight = terrain.terrainData.size.z;

            Debug.LogError("Lodding with terrain size X/Y: " + terrainWorldWidth + "/" + terrainWorldHeight);

            Debug.LogError("Lodding with samplesX: " + samplesX + " SamplesZ: " + samplesZ);

            float sampleSizeX = terrainWorldWidth / (float)samplesX;
            float sampleSizeZ = terrainWorldHeight / (float)samplesZ;
            Debug.LogError("Lodding with Sample Size X/Z:" + sampleSizeX + "/" + sampleSizeZ);

            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();

            samplesZ++;
            samplesX++;

            for (int z = 0; z < samplesZ; z++)
            {
                for(int x = 0; x < samplesX; x++)
                {
                    Vector3 pos = new Vector3(z * sampleSizeZ, 0.0f, x * sampleSizeX);
                    pos.y = terrain.terrainData.GetInterpolatedHeight(pos.x / terrainWorldWidth, pos.z / terrainWorldHeight);

                    //Debug.Log("PosY: " + pos.y);
                    vertices.Add(pos);
                }
            }

            int len = vertices.Count - samplesX - 1;
            for (int i = 0; i < len; i++)
            {
                if ((i + 1) % samplesX == 0)
                    continue;
                int tv0 = i;
                int tv1 = i + 1;
                int tv2 = i + samplesX;

                int vv0 = i + 1;
                int vv1 = i + samplesX + 1;
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

            GenerateControlUVs(sourceTerrain, terrain,mesh, samplesX, samplesZ);

            return mesh;
        }


        /// <summary>
        /// Creates the splat map for given TerrainData. 
        /// Splat map contains data in the rgba channels that says which splat texture should be rendered where.
        /// </summary>
        /// <param name="td"></param>
        /// <returns></returns>
        /*private static Texture2D CreateSplatMap(TerrainData td)
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
        }*/

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
        private static void GenerateControlUVs(TerrainData data, Terrain sourceTerrain, Mesh terrainMesh)
        {
            int sourceWidth = sourceTerrain.terrainData.heightmapWidth;
            int sourceHeight = sourceTerrain.terrainData.heightmapHeight;

            float sampleWidth = 1.0f / (float)sourceWidth;
            float sampleHeight = 1.0f / (float)sourceHeight;
            
            int terrainWidth = data.heightmapWidth;
            int terrainHeight = data.heightmapHeight;
            
            //  Get the offset of the splat from the number of the mesh, which is in the name
            string terrainNumbers = terrainMesh.name.Substring(terrainMesh.name.Length - 3, 3);

            int yOffset = int.Parse(terrainNumbers.Substring(0, 1));
            int xOffset = int.Parse(terrainNumbers.Substring(2, 1));
            
            xOffset *= terrainWidth;
            yOffset *= terrainHeight;
            
            List<Vector2> uvs = new List<Vector2>();

            for (int y = 0; y < terrainHeight; y++)
            {
                for (int x = 0; x < terrainWidth; x++)
                {
                    float u = (xOffset * sampleWidth) + (x * sampleWidth);
                    float v = (yOffset * sampleHeight) + (y * sampleHeight);
                    uvs.Add(new Vector2(u, v));
                }
            }

            terrainMesh.SetUVs(0, uvs);
        }

        private static void GenerateControlUVs(Terrain sourceTerrain, Terrain terrainChunk, Mesh terrainMesh, int samplesX, int samplesZ)
        {
            var sourceTerrainSize = sourceTerrain.terrainData.size;
            var terrainChunkSize = terrainChunk.terrainData.size;

            float numChunksX = (int)(sourceTerrainSize.x / terrainChunkSize.x);
            float numChunksZ = (int)(sourceTerrainSize.z / terrainChunkSize.z);

            Debug.Log("Chunks X/Y: " + numChunksX + "/" + numChunksZ);

            float textureAreaX = 1.0f / numChunksX;
            float textureAreaZ = 1.0f / numChunksZ;
            
            /*string terrainNumbers = terrainMesh.name.Substring(terrainMesh.name.Length - 3, 3);

            int yOffset = int.Parse(terrainNumbers.Substring(0, 1));
            int xOffset = int.Parse(terrainNumbers.Substring(2, 1));
            
            List<Vector2> uvs = new List<Vector2>();
            
            for(int y = 0; y < terrainHeight; y++)
            {
                for(int x = 0; x < terrainHeight; x++)
                {
                    float u = (xOffset * sourceSampleWidth) + (x * sourceSampleWidth * meshSampleScaleX);
                    float v = (yOffset * sourceSampleHeight) + (y * sourceSampleHeight * meshSampleScaleY);
                }
            }

            terrainMesh.SetUVs(0, uvs);*/
        }
    }

}

