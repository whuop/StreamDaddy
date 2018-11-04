using StreamDaddy.Editor.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace StreamDaddy.Editor.TerrainTools
{
    public static class TerrainSplitter
    {
        public static List<Terrain> SplitIntoChunks(int chunkSizeX, int chunkSizeZ, Terrain origTerrain, string terrainSavePath)
        {
            //  Create folder structure
            PathUtils.EnsurePathExists(terrainSavePath);

            int numSplitsX = Mathf.CeilToInt(origTerrain.terrainData.size.x / chunkSizeX);
            int numSplitsZ = Mathf.CeilToInt(origTerrain.terrainData.size.z / chunkSizeZ);

            int heightResolution = origTerrain.terrainData.heightmapResolution / numSplitsX;
            int splatResolution = origTerrain.terrainData.alphamapResolution / numSplitsX;
            int detailResolution = origTerrain.terrainData.detailResolution / numSplitsX;

            //  Increase height map resolution by 1 to account for edge extrude
            heightResolution += 1;

            if (origTerrain == null)
            {
                Debug.LogWarning("No terrain found on transform");
                return null;
            }

            Assert.IsTrue(numSplitsX >= 1, "NumSplitsX is less than 0, must be at least 1!");
            Assert.IsTrue(numSplitsZ >= 1, "NumSplitsZ is less than 0, must be at least 1!");

            List<Terrain> terrains = new List<Terrain>();

            for (int x = 0; x < numSplitsX; x++)
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
            }
            EditorUtility.ClearProgressBar();

            for (int x = 0; x < numSplitsX; x++)
            {
                for (int z = 0; z < numSplitsZ; z++)
                {
                    GameObject center = GameObject.Find(string.Format("{0}{1}_{2}", origTerrain.name, x, z));
                    GameObject right = GameObject.Find(string.Format("{0}{1}_{2}", origTerrain.name, x + 1, z));
                    GameObject top = GameObject.Find(string.Format("{0}{1}_{2}", origTerrain.name, x, z + 1));
                    GameObject topRight = GameObject.Find(string.Format("{0}{1}_{2}", origTerrain.name, x + 1, z + 1));
                    GameObject left = GameObject.Find(string.Format("{0}{1}_{2}", origTerrain.name, x - 1, z));
                    GameObject bottom = GameObject.Find(string.Format("{0}{1}_{2}", origTerrain.name, x, z - 1));

                    if (center == null)
                        continue;

                    Terrain centerT = center.GetComponent<Terrain>();
                    if (centerT == null)
                        continue;

                    Terrain rightT = null;
                    Terrain topT = null;
                    Terrain topRightT = null;
                    Terrain leftT = null;
                    Terrain bottomT = null;

                    if (right != null)
                        rightT = right.GetComponent<Terrain>();
                    if (top != null)
                        topT = top.GetComponent<Terrain>();
                    if (topRight != null)
                        topRightT = topRight.GetComponent<Terrain>();
                    if (left != null)
                        leftT = left.GetComponent<Terrain>();
                    if (bottom != null)
                        bottomT = bottom.GetComponent<Terrain>();

                    StitchTopAndRight(centerT, topT, rightT, topRightT, heightResolution);
                    centerT.SetNeighbors(leftT, topT, rightT, bottomT);
                }
            }

            return terrains;
        }

        static void CopyTerrain(Terrain origTerrain, List<Terrain> splits, string newName, string savePath, float xMin, float xMax, float zMin, float zMax, int heightmapResolution, int detailResolution, int alphamapResolution, int chunkX, int chunkZ)
        {
            if (heightmapResolution < 33 || heightmapResolution > 4097)
            {
                Debug.Log("Invalid heightmap resolution");
                return;
            }
            if (detailResolution < 0 || detailResolution > 4048)
            {
                Debug.Log("Invalid detailResolution " + detailResolution);
                return;
            }
            if (alphamapResolution < 16 || alphamapResolution > 2048)
            {
                Debug.Log("Invalid alphamapResolution " + alphamapResolution);
                return;
            }

            if (xMin < 0 || xMin > xMax || xMax > origTerrain.terrainData.size.x)
            {
                Debug.Log("Invalid xMin or xMax");
                return;
            }
            if (zMin < 0 || zMin > zMax || zMax > origTerrain.terrainData.size.z)
            {
                Debug.Log("Invalid zMin or zMax");
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
            td.heightmapResolution = heightmapResolution;
            float[,] newHeights = new float[heightmapResolution, heightmapResolution];

            var origHeights = origTerrain.terrainData.GetHeights(0, 0, origTerrain.terrainData.heightmapWidth, origTerrain.terrainData.heightmapHeight);

            for (int i = 0; i < heightmapResolution - 1; i++)
            {
                for (int j = 0; j < heightmapResolution - 1; j++)
                {
                    newHeights[j, i] = origHeights[chunkZ * (heightmapResolution - 1) + j, chunkX * (heightmapResolution - 1) + i];
                }
            }
            td.SetHeightsDelayLOD(0, 0, newHeights);

            // Detail
            td.SetDetailResolution(detailResolution, 8); // Default? Haven't messed with resolutionPerPatch
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
            }

            // Splat
            td.alphamapResolution = alphamapResolution;
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

            // Tree
            for (int i = 0; i < origTerrain.terrainData.treeInstanceCount; i++)
            {
                TreeInstance ti = origTerrain.terrainData.treeInstances[i];
                if (ti.position.x < xMinNorm || ti.position.x >= xMaxNorm)
                    continue;
                if (ti.position.z < zMinNorm || ti.position.z >= zMaxNorm)
                    continue;
                ti.position = new Vector3(((ti.position.x * origTerrain.terrainData.size.x) - xMin) / (xMax - xMin), ti.position.y, ((ti.position.z * origTerrain.terrainData.size.z) - zMin) / (zMax - zMin));
                newTerrain.AddTreeInstance(ti);
            }

            gameObject.transform.position = new Vector3(origTerrain.transform.position.x + xMin, origTerrain.transform.position.y, origTerrain.transform.position.z + zMin);
            gameObject.name = newName;

            // Must happen after setting heightmapResolution
            td.size = new Vector3(xMax - xMin, origTerrain.terrainData.size.y, zMax - zMin);

            splits.Add(newTerrain);

            AssetDatabase.SaveAssets();
        }

        static void StitchTopAndRight(Terrain cur, Terrain top, Terrain right, Terrain topRight, int heightResolution)
        {
            float[,] newHeights = new float[heightResolution, heightResolution];
            float[,] topHeights = new float[heightResolution, heightResolution];
            float[,] rightHeights = new float[heightResolution, heightResolution];
            float[,] topRightHeights = new float[heightResolution, heightResolution];

            if (cur == null)
                return;

            newHeights = cur.terrainData.GetHeights(0, 0, heightResolution, heightResolution);

            if (top != null)
                topHeights = top.terrainData.GetHeights(0, 0, heightResolution, heightResolution);
            if (right != null)
                rightHeights = right.terrainData.GetHeights(0, 0, heightResolution, heightResolution);
            if (topRight != null)
                topRightHeights = topRight.terrainData.GetHeights(0, 0, heightResolution, heightResolution);

            for (int i = 0; i < heightResolution; i++)
            {
                if (right != null)
                {
                    newHeights[i, heightResolution - 1] = rightHeights[i, 0];
                }

                if (top != null)
                {
                    newHeights[heightResolution - 1, i] = topHeights[0, i];
                }
            }

            if (topRight != null)
            {
                newHeights[heightResolution - 1, heightResolution - 1] = topRightHeights[0, 0];
            }

            cur.terrainData.SetHeights(0, 0, newHeights);
        }
    }
}

