using StreamDaddy.Editor.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StreamDaddy.Editor.TerrainTools
{
    public static class TerrainSplitter
    {
        private static float SubtractFromAndReturn(ref float subtractee, float subtractAmount)
        {
            float retVal = 0.0f;
            if (subtractAmount >= subtractee)
            {
                retVal = subtractee;
                subtractee = 0.0f;
                return retVal;
            }

            subtractee -= subtractAmount;
            retVal = subtractAmount;
            return retVal;
        }

        public static List<Terrain> SplitIntoChunks(int chunkSizeX, int chunkSizeZ, Terrain origTerrain, string terrainSavePath)
        {
            //  Create folder structure
            PathUtils.EnsurePathExists(terrainSavePath);
            
            if (origTerrain == null)
            {
                Debug.LogWarning("No terrain found on transform");
                return null;
            }
            
            List<Terrain> terrains = new List<Terrain>();

            float totalAllotedSizeX = origTerrain.terrainData.size.x;
            float totalAllotedSizeZ = origTerrain.terrainData.size.z;

            int numChunksX = Mathf.FloorToInt(totalAllotedSizeX / (float)chunkSizeX);
            int numChunksZ = Mathf.FloorToInt(totalAllotedSizeZ / (float)chunkSizeZ);

            float totalTakenX = 0.0f;
            float totalTakenZ = 0.0f;

            float xMin = 0.0f;
            float xMax = 0.0f;
            float zMin = 0.0f;
            float zMax = 0.0f;

            int x = 0;
            int z = 0;
            while(totalAllotedSizeX > 0)
            {
                float takenX = SubtractFromAndReturn(ref totalAllotedSizeX, (float)chunkSizeX);
                

                xMin = totalTakenX;
                totalTakenX += takenX;
                xMax = totalTakenX;

                
                while (totalAllotedSizeZ > 0)
                {
                    //EditorUtility.DisplayProgressBar("Splitting Terrain", "Copying heightmap, detail, splat, and trees", (float)((x * numSplitsZ) + z) / (numSplitsX * numSplitsZ));
                    float takenZ = SubtractFromAndReturn(ref totalAllotedSizeZ, (float)chunkSizeZ);

                    zMin = totalTakenZ;
                    totalTakenZ += takenZ;
                    zMax = totalTakenZ;

                    float cSizeX = (xMax - xMin);
                    float cSizeZ = (zMax - zMin);
                    
                    float chunkSizePercentX = cSizeX / origTerrain.terrainData.size.x;
                    float chunkSizePercentZ = cSizeZ /origTerrain.terrainData.size.z;

                    float largestPercent = (chunkSizePercentX > chunkSizePercentZ) ? chunkSizePercentX : chunkSizePercentZ;

                    Debug.LogError("Largest Percent: " + largestPercent);
                    Debug.LogError("ChunkSizePercent X/Z: " + chunkSizePercentX + "/" + chunkSizePercentZ);

                    int heightmapResolution = Mathf.RoundToInt((float)origTerrain.terrainData.heightmapResolution * largestPercent);
                    heightmapResolution = NearestPoT(heightmapResolution) + 1;

                    int splatResolution = Mathf.RoundToInt((float)origTerrain.terrainData.alphamapResolution * largestPercent);
                    splatResolution = NearestPoT(splatResolution) + 1;

                    int detailResolution = Mathf.RoundToInt((float)origTerrain.terrainData.detailResolution * largestPercent);
                    detailResolution = NearestPoT(detailResolution) + 1;

                    Debug.LogError("Heightmap Resolution: " + heightmapResolution);
                    Debug.LogError("Splat Resolution: " + splatResolution);
                    Debug.LogError("Detail Resolution: " + detailResolution);
                    

                    //  Switched X for Z in CopyTerrain. Not really sure why that has to be done currently, but if i dont everything is mirrored all weird, so i think it's just that the 
                    //  x and z index of the loop does not correspond to the order they are being looped.
                    CopyTerrain(origTerrain, terrains, string.Format("{0}{1}_{2}", origTerrain.name, x, z), terrainSavePath, xMin, xMax, zMin, zMax, heightmapResolution, detailResolution, splatResolution, x, z);
                    z++;
                }

                totalAllotedSizeZ = origTerrain.terrainData.size.z;
                totalTakenZ = 0.0f;
                zMin = 0.0f;
                zMax = 0.0f;
                z = 0;
                x++;
            }


            /*for (int x = 0; x < numSplitsX; x++)
            {
                for (int z = 0; z < numSplitsZ; z++)
                {
                    EditorUtility.DisplayProgressBar("Splitting Terrain", "Copying heightmap, detail, splat, and trees", (float)((x * numSplitsZ) + z) / (numSplitsX * numSplitsZ));
                    float xMin = origTerrain.terrainData.size.x / numSplitsX * x;
                    float xMax = origTerrain.terrainData.size.x / numSplitsX * (x + 1);
                    float zMin = origTerrain.terrainData.size.z / numSplitsZ * z;
                    float zMax = origTerrain.terrainData.size.z / numSplitsZ * (z + 1);
                    CopyTerrain(origTerrain, terrains, string.Format("{0}{1}_{2}", origTerrain.name, x, z), terrainSavePath, xMin, xMax, zMin, zMax, heightResolution, detailResolution, splatResolution, x, z);
                }
            }*/
            //EditorUtility.ClearProgressBar();
            

            return terrains;
        }

        static void CopyTerrain(Terrain origTerrain, List<Terrain> splits, string newName, string savePath, float xMin, float xMax, float zMin, float zMax, int heightmapResolution, int detailResolution, int alphamapResolution, int chunkX, int chunkZ)
        {
            if (heightmapResolution < 33 || heightmapResolution > 4097)
            {
                Debug.Log("Invalid heightmap resolution " + heightmapResolution);
                return;
            }
            if (detailResolution < 17 || detailResolution > 4048)
            {
                Debug.LogError("Invalid detailResolution " + detailResolution);
                return;
            }
            if (alphamapResolution < 17 || alphamapResolution > 2048)
            {
                Debug.LogError("Invalid alphamapResolution " + alphamapResolution);
                return;
            }

            if (xMin < 0 || xMin > xMax || xMax > origTerrain.terrainData.size.x)
            {
                Debug.LogError("Invalid xMin or xMax");
                return;
            }
            if (zMin < 0 || zMin > zMax || zMax > origTerrain.terrainData.size.z)
            {
                Debug.LogError("Invalid zMin or zMax");
                return;
            }

            //  Remove old terrain asset if it exists.
            string assetPath = savePath + newName + ".asset";
            if (AssetDatabase.FindAssets(newName).Length != 0)
            {
                Debug.Log("Asset with name " + newName + " already exists, deleting old one to make room for new.");
                AssetDatabase.DeleteAsset(assetPath);
            }

            //  Remove old terrain game object if it exists.
            GameObject oldT = GameObject.Find(newName);
            if (oldT != null)
            {
                Debug.Log("Terrain Game object with name " + newName + " already exists. Deleting old one to make room for new.");
                GameObject.DestroyImmediate(oldT);
            }

            TerrainData td = new TerrainData();
            GameObject gameObject = Terrain.CreateTerrainGameObject(td);
            Terrain newTerrain = gameObject.GetComponent<Terrain>();

            
            // Must do this before Splat
            //  Create the actual asset
            AssetDatabase.CreateAsset(td, assetPath);

            //  Lighting
            newTerrain.lightmapIndex = origTerrain.lightmapIndex;
            newTerrain.lightmapScaleOffset = origTerrain.lightmapScaleOffset;
            newTerrain.realtimeLightmapIndex = origTerrain.realtimeLightmapIndex;
            newTerrain.realtimeLightmapScaleOffset = origTerrain.realtimeLightmapScaleOffset;
            newTerrain.reflectionProbeUsage = origTerrain.reflectionProbeUsage;
            newTerrain.castShadows = origTerrain.castShadows;

            //  Material
            newTerrain.materialTemplate = origTerrain.materialTemplate;
            newTerrain.materialType = origTerrain.materialType;

            //  Legacy
            newTerrain.legacyShininess = origTerrain.legacyShininess;
            newTerrain.legacySpecular = origTerrain.legacySpecular;

            //  Heightmap
            newTerrain.drawHeightmap = origTerrain.drawHeightmap;
            newTerrain.heightmapMaximumLOD = origTerrain.heightmapMaximumLOD;
            newTerrain.heightmapPixelError = origTerrain.heightmapPixelError;

            //  Detail
            newTerrain.detailObjectDensity = origTerrain.detailObjectDensity;
            newTerrain.detailObjectDistance = origTerrain.detailObjectDistance;

            newTerrain.collectDetailPatches = origTerrain.collectDetailPatches;
            newTerrain.patchBoundsMultiplier = origTerrain.patchBoundsMultiplier;
            
            //  Tree
            td.treePrototypes = origTerrain.terrainData.treePrototypes;
            newTerrain.drawTreesAndFoliage = origTerrain.drawTreesAndFoliage;
            newTerrain.treeBillboardDistance = origTerrain.treeBillboardDistance;
            newTerrain.treeCrossFadeLength = origTerrain.treeCrossFadeLength;
            newTerrain.treeDistance = origTerrain.treeDistance;
            newTerrain.treeMaximumFullLODCount = origTerrain.treeMaximumFullLODCount;
            newTerrain.bakeLightProbesForTrees = origTerrain.bakeLightProbesForTrees;

            //  Misc
            newTerrain.editorRenderFlags = origTerrain.editorRenderFlags;
            newTerrain.basemapDistance = origTerrain.basemapDistance;

            //  TerrainData
            td.detailPrototypes = origTerrain.terrainData.detailPrototypes;

            //  Adjust splatmap tile position to chunk position
            var splats = origTerrain.terrainData.splatPrototypes;
            foreach (var splat in splats)
            {
                splat.tileOffset = new Vector2((heightmapResolution - 1) * chunkX, (heightmapResolution - 1) * chunkZ);
            }
            td.splatPrototypes = splats;

            //  Grass
            td.wavingGrassAmount = origTerrain.terrainData.wavingGrassAmount;
            td.wavingGrassSpeed = origTerrain.terrainData.wavingGrassSpeed;
            td.wavingGrassStrength = origTerrain.terrainData.wavingGrassStrength;
            td.wavingGrassTint = origTerrain.terrainData.wavingGrassTint;

            // Get percent of original
            float xMinNorm = xMin / origTerrain.terrainData.size.x;
            float xMaxNorm = xMax / origTerrain.terrainData.size.x;
            float zMinNorm = zMin / origTerrain.terrainData.size.z;
            float zMaxNorm = zMax / origTerrain.terrainData.size.z;

            // Height
            Vector2 newTerrainSize = new Vector2(xMax - xMin, zMax - zMin);
            CalculateSubHeightmap(td, heightmapResolution, origTerrain, chunkX, chunkZ);

            // Detail
            /*td.SetDetailResolution(detailResolution, 8); // Default? Haven't messed with resolutionPerPatch
            for (int layer = 0; layer < origTerrain.terrainData.detailPrototypes.Length; layer++)
            {
                int[,] detailLayer = origTerrain.terrainData.GetDetailLayer(0, 0, origTerrain.terrainData.detailWidth, origTerrain.terrainData.detailHeight, layer);
                int[,] newDetailLayer = new int[detailResolution, detailResolution];
                for (int x = 0; x < newDetailLayer.GetLength(0); x++)
                {
                    for (int z = 0; z < newDetailLayer.GetLength(1); z++)
                    {
                        newDetailLayer[z, x] = detailLayer[chunkZ * (detailResolution) + z, chunkX * (detailResolution) + x];
                    }
                }
                td.SetDetailLayer(0, 0, layer, newDetailLayer);
            }*/

            // Splat
            /*td.alphamapResolution = alphamapResolution;
            float[,,] alphamaps = origTerrain.terrainData.GetAlphamaps(0, 0, origTerrain.terrainData.alphamapWidth, origTerrain.terrainData.alphamapHeight);
            float[,,] newAlphamaps = new float[alphamapResolution, alphamapResolution, alphamaps.GetLength(2)];

            for (int x = 0; x < newAlphamaps.GetLength(0); x++)
            {
                for (int z = 0; z < newAlphamaps.GetLength(1); z++)
                {
                    for (int k = 0; k < newAlphamaps.GetLength(2); k++)
                    {
                        newAlphamaps[z, x, k] = alphamaps[chunkZ * (alphamapResolution) + z, chunkX * (alphamapResolution) + x, k];
                    }
                }
            }
            td.SetAlphamaps(0, 0, newAlphamaps);
            */
            // Tree
            /*for (int i = 0; i < origTerrain.terrainData.treeInstanceCount; i++)
            {
                TreeInstance ti = origTerrain.terrainData.treeInstances[i];
                if (ti.position.x < xMinNorm || ti.position.x >= xMaxNorm)
                    continue;
                if (ti.position.z < zMinNorm || ti.position.z >= zMaxNorm)
                    continue;
                ti.position = new Vector3(((ti.position.x * origTerrain.terrainData.size.x) - xMin) / (xMax - xMin), ti.position.y, ((ti.position.z * origTerrain.terrainData.size.z) - zMin) / (zMax - zMin));
                newTerrain.AddTreeInstance(ti);
            }*/

            gameObject.transform.position = new Vector3(origTerrain.transform.position.x + xMin, origTerrain.transform.position.y, origTerrain.transform.position.z + zMin);
            gameObject.name = newName;

            // Must happen after setting heightmapResolution
            td.size = new Vector3(xMax - xMin, origTerrain.terrainData.size.y, zMax - zMin);

            splits.Add(newTerrain);

            AssetDatabase.SaveAssets();
        }

        private static void CalculateSubHeightmap(TerrainData newTerrainData, int heightmapResolution, Terrain origTerrain, int chunkX, int chunkZ)
        {
            newTerrainData.heightmapResolution = heightmapResolution;
            float[,] newHeights = new float[heightmapResolution, heightmapResolution];

            float chunkSizeNormalized = ((float)heightmapResolution - 1.0f) / ((float)origTerrain.terrainData.heightmapResolution - 1.0f);
            float sampleSizeNormalized = chunkSizeNormalized / ((float)heightmapResolution - 1);

            float xOffset = chunkX * chunkSizeNormalized;
            float zOffset = chunkZ * chunkSizeNormalized;

            Color col = new Color(1, 0, 0);
            float colorInc = 1.0f / (float)heightmapResolution;
            for(int z = 0; z < heightmapResolution; z++)
            {
                col.b = 0.0f;
                for(int x = 0; x < heightmapResolution; x++)
                {
                    float posX = xOffset + (x * sampleSizeNormalized);
                    float posZ = zOffset + (z * sampleSizeNormalized);

                    float height = origTerrain.terrainData.GetInterpolatedHeight( posX, posZ);

                    newHeights[z, x] = height / origTerrain.terrainData.size.y;

                    Debug.DrawLine(new Vector3(posX * origTerrain.terrainData.size.x, height, posZ * origTerrain.terrainData.size.z), new Vector3(posX * origTerrain.terrainData.size.x, height + 1.0f, posZ * origTerrain.terrainData.size.z), col, 20.0f);
                }
            }

            newTerrainData.SetHeightsDelayLOD(0, 0, newHeights);
        }

        public static int NearestPoT(int num)
        {
            return (int)Mathf.Pow(2, Mathf.Round(Mathf.Log(num) / Mathf.Log(2)));
        }
    }
}

