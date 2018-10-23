using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class SplitTerrain : EditorWindow
{
    public void Split(int chunkSizeX, int chunkSizeZ)
    {
        Terrain origTerrain = FindObjectOfType<Terrain>();

        int numSplitsX = Mathf.CeilToInt(origTerrain.terrainData.size.x / chunkSizeX);
        int numSplitsZ = Mathf.CeilToInt(origTerrain.terrainData.size.z / chunkSizeZ);

        int heightResolution = origTerrain.terrainData.heightmapResolution / numSplitsX;
        int detailResolution = origTerrain.terrainData.detailResolution / numSplitsX;
        int splatResolution = origTerrain.terrainData.alphamapResolution / numSplitsX;

        if (origTerrain == null)
        {
            Debug.LogWarning("No terrain found on transform");
            return;
        }

        Assert.IsTrue(numSplitsX >= 1, "NumSplitsX is less than 0, must be at least 1!");
        Assert.IsTrue(numSplitsZ >= 1, "NumSplitsZ is less than 0, must be at least 1!");

        for (int x = 0; x < numSplitsX; x++)
        {
            for (int z = 0; z < numSplitsZ; z++)
            {
                EditorUtility.DisplayProgressBar("Splitting Terrain", "Copying heightmap, detail, splat, and trees", (float)((x * numSplitsZ) + z) / (numSplitsX * numSplitsZ));
                float xMin = origTerrain.terrainData.size.x / numSplitsX * x;
                float xMax = origTerrain.terrainData.size.x / numSplitsX * (x + 1);
                float zMin = origTerrain.terrainData.size.z / numSplitsZ * z;
                float zMax = origTerrain.terrainData.size.z / numSplitsZ * (z + 1);
                copyTerrain(origTerrain, string.Format("{0}{1}_{2}", origTerrain.name, x, z), xMin, xMax, zMin, zMax, heightResolution, detailResolution, splatResolution);
            }
        }
        EditorUtility.ClearProgressBar();

        for (int x = 0; x < numSplitsX; x++)
        {
            for (int z = 0; z < numSplitsZ; z++)
            {
                GameObject center = GameObject.Find(string.Format("{0}{1}_{2}", origTerrain.name, x, z));
                GameObject right = GameObject.Find(string.Format("{0}{1}_{2}", origTerrain.name, x + 1, z));
                GameObject bot = GameObject.Find(string.Format("{0}{1}_{2}", origTerrain.name, x, z - 1));
                GameObject top = GameObject.Find(string.Format("{0}{1}_{2}", origTerrain.name, x, z + 1));
                GameObject left = GameObject.Find(string.Format("{0}{1}_{2}", origTerrain.name, x - 1, z));

                if (center == null)
                    continue;

                Terrain centerT = center.GetComponent<Terrain>();
                if (centerT == null)
                    continue;

                Terrain rightT = null;
                Terrain bottomT = null;
                Terrain leftT = null;
                Terrain topT = null;

                if (right != null)
                    rightT = right.GetComponent<Terrain>();
                if (bot != null)
                    bottomT = bot.GetComponent<Terrain>();
                if (top != null)
                    topT = top.GetComponent<Terrain>();
                if (left != null)
                    leftT = left.GetComponent<Terrain>();
                


                StitchRight(centerT, rightT, heightResolution);
                //StitchTop(centerT, topT, heightResolution); // This one does not work correctly, check up on it.

                //StitchLeft(centerT, leftT, heightResolution);
                //StitchBottom(centerT, bottomT, heightResolution);
                //StitchTopRight(centerT, topT, rightT, heightResolution);
                //StitchTopAndLeft(centerT, topT, leftT, heightResolution);
                //StitchBottomAndRight(centerT, rightT, bottomT, heightResolution);

                //centerT.SetNeighbors(leftT, topT, rightT, bottomT);
            }
        }
    }

    void copyTerrain(Terrain origTerrain, string newName, float xMin, float xMax, float zMin, float zMax, int heightmapResolution, int detailResolution, int alphamapResolution)
    {
        if (heightmapResolution < 33 || heightmapResolution > 4097)
        {
            Debug.Log("Invalid heightmap resolution");
            heightmapResolution = 33;
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

        if (AssetDatabase.FindAssets(newName).Length != 0)
        {
            Debug.Log("Asset with name " + newName + " already exists");
            return;
        }

        TerrainData td = new TerrainData();
        GameObject gameObject = Terrain.CreateTerrainGameObject(td);
        Terrain newTerrain = gameObject.GetComponent<Terrain>();

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        // Must do this before Splat
        AssetDatabase.CreateAsset(td, "Assets/Resources/" + newName + ".asset");

        // Copy over all vars
        newTerrain.bakeLightProbesForTrees = origTerrain.bakeLightProbesForTrees;
        newTerrain.basemapDistance = origTerrain.basemapDistance;
        newTerrain.castShadows = origTerrain.castShadows;
        newTerrain.collectDetailPatches = origTerrain.collectDetailPatches;
        newTerrain.detailObjectDensity = origTerrain.detailObjectDensity;
        newTerrain.detailObjectDistance = origTerrain.detailObjectDistance;
        newTerrain.drawHeightmap = origTerrain.drawHeightmap;
        newTerrain.drawTreesAndFoliage = origTerrain.drawTreesAndFoliage;
        newTerrain.editorRenderFlags = origTerrain.editorRenderFlags;
        newTerrain.heightmapMaximumLOD = origTerrain.heightmapMaximumLOD;
        newTerrain.heightmapPixelError = origTerrain.heightmapPixelError;
        newTerrain.legacyShininess = origTerrain.legacyShininess;
        newTerrain.legacySpecular = origTerrain.legacySpecular;
        newTerrain.lightmapIndex = origTerrain.lightmapIndex;
        newTerrain.lightmapScaleOffset = origTerrain.lightmapScaleOffset;
        newTerrain.materialTemplate = origTerrain.materialTemplate;
        newTerrain.materialType = origTerrain.materialType;
        newTerrain.realtimeLightmapIndex = origTerrain.realtimeLightmapIndex;
        newTerrain.realtimeLightmapScaleOffset = origTerrain.realtimeLightmapScaleOffset;
        newTerrain.reflectionProbeUsage = origTerrain.reflectionProbeUsage;
        newTerrain.treeBillboardDistance = origTerrain.treeBillboardDistance;
        newTerrain.treeCrossFadeLength = origTerrain.treeCrossFadeLength;
        newTerrain.treeDistance = origTerrain.treeDistance;
        newTerrain.treeMaximumFullLODCount = origTerrain.treeMaximumFullLODCount;

        td.splatPrototypes = origTerrain.terrainData.splatPrototypes;
        td.treePrototypes = origTerrain.terrainData.treePrototypes;
        td.detailPrototypes = origTerrain.terrainData.detailPrototypes;

        // Get percent of original
        float xMinNorm = xMin / origTerrain.terrainData.size.x;
        float xMaxNorm = xMax / origTerrain.terrainData.size.x;
        float zMinNorm = zMin / origTerrain.terrainData.size.z;
        float zMaxNorm = zMax / origTerrain.terrainData.size.z;
        float dimRatio1, dimRatio2;

        // Height
        td.heightmapResolution = heightmapResolution;
        float[,] newHeights = new float[heightmapResolution, heightmapResolution];
        dimRatio1 = (xMax - xMin) / heightmapResolution;
        dimRatio2 = (zMax - zMin) / heightmapResolution;
        for (int i = 0; i < heightmapResolution; i++)
        {
            for (int j = 0; j < heightmapResolution; j++)
            {
                // Divide by size.y because height is stored as percentage
                // Note this is [j, i] and not [i, j] (Why?!)
                newHeights[j, i] = origTerrain.SampleHeight(new Vector3(xMin + (i * dimRatio1), 0, zMin + (j * dimRatio2))) / origTerrain.terrainData.size.y;
            }
        }
        td.SetHeightsDelayLOD(0, 0, newHeights);

        // Detail
        td.SetDetailResolution(detailResolution, 8); // Default? Haven't messed with resolutionPerPatch
        for (int layer = 0; layer < origTerrain.terrainData.detailPrototypes.Length; layer++)
        {
            int[,] detailLayer = origTerrain.terrainData.GetDetailLayer(
                    Mathf.FloorToInt(xMinNorm * origTerrain.terrainData.detailWidth),
                    Mathf.FloorToInt(zMinNorm * origTerrain.terrainData.detailHeight),
                    Mathf.FloorToInt((xMaxNorm - xMinNorm) * origTerrain.terrainData.detailWidth),
                    Mathf.FloorToInt((zMaxNorm - zMinNorm) * origTerrain.terrainData.detailHeight),
                    layer);
            int[,] newDetailLayer = new int[detailResolution, detailResolution];
            dimRatio1 = (float)detailLayer.GetLength(0) / detailResolution;
            dimRatio2 = (float)detailLayer.GetLength(1) / detailResolution;
            for (int i = 0; i < newDetailLayer.GetLength(0); i++)
            {
                for (int j = 0; j < newDetailLayer.GetLength(1); j++)
                {
                    newDetailLayer[i, j] = detailLayer[Mathf.FloorToInt(i * dimRatio1), Mathf.FloorToInt(j * dimRatio2)];
                }
            }
            td.SetDetailLayer(0, 0, layer, newDetailLayer);
        }

        // Splat
        td.alphamapResolution = alphamapResolution;
        float[,,] alphamaps = origTerrain.terrainData.GetAlphamaps(
            Mathf.FloorToInt(xMinNorm * origTerrain.terrainData.alphamapWidth),
            Mathf.FloorToInt(zMinNorm * origTerrain.terrainData.alphamapHeight),
            Mathf.FloorToInt((xMaxNorm - xMinNorm) * origTerrain.terrainData.alphamapWidth),
            Mathf.FloorToInt((zMaxNorm - zMinNorm) * origTerrain.terrainData.alphamapHeight));
        // Last dim is always origTerrain.terrainData.splatPrototypes.Length so don't ratio
        float[,,] newAlphamaps = new float[alphamapResolution, alphamapResolution, alphamaps.GetLength(2)];
        dimRatio1 = (float)alphamaps.GetLength(0) / alphamapResolution;
        dimRatio2 = (float)alphamaps.GetLength(1) / alphamapResolution;
        for (int i = 0; i < newAlphamaps.GetLength(0); i++)
        {
            for (int j = 0; j < newAlphamaps.GetLength(1); j++)
            {
                for (int k = 0; k < newAlphamaps.GetLength(2); k++)
                {
                    newAlphamaps[i, j, k] = alphamaps[Mathf.FloorToInt(i * dimRatio1), Mathf.FloorToInt(j * dimRatio2), k];
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

        AssetDatabase.SaveAssets();
    }

    void StitchTop(Terrain cur, Terrain top, int heightResolution)
    {
        float[,] newHeights = new float[heightResolution, heightResolution];
        float[,] topHeights = new float[heightResolution, heightResolution];

        if (top == null)
            return;

        for (int i = 0; i < heightResolution; i++)
        {
            if (top != null)
                newHeights[heightResolution - 1, i] = topHeights[0, i];
        }

        cur.terrainData.SetHeights(0, 0, newHeights);
    }

    void StitchRight(Terrain cur, Terrain right, int heightResolution)
    {
        float[,] newHeights = new float[heightResolution, heightResolution];
        float[,] rightHeights = new float[heightResolution, heightResolution];

        if (right == null)
            return;
        
        newHeights = cur.terrainData.GetHeights(0, 0, heightResolution, heightResolution);
        rightHeights = right.terrainData.GetHeights(0, 0, heightResolution, heightResolution);

        for (int i = 0; i < heightResolution; i++)
        {
            if (right != null)
                newHeights[i, heightResolution - 1] = rightHeights[i, 0];
        }
        cur.terrainData.SetHeights(0, 0, newHeights);
    }

    void StitchLeft(Terrain cur, Terrain left, int heightResolution)
    {
        float[,] newHeights = new float[heightResolution, heightResolution];
        float[,] leftHeights = new float[heightResolution, heightResolution];

        if (left == null)
            return;
        
        newHeights = cur.terrainData.GetHeights(0, 0, heightResolution, heightResolution);

        for (int i = 0; i < heightResolution; i++)
        {
            if (left != null)
                newHeights[i, 0] = leftHeights[i, heightResolution - 1];
        }
        cur.terrainData.SetHeights(0, 0, newHeights);
    }

    void StitchBottom(Terrain cur, Terrain bottom, int heightResolution)
    {
        float[,] newHeights = new float[heightResolution, heightResolution];
        float[,] bottomHeights = new float[heightResolution, heightResolution];

        if (bottom == null)
            return;
        
        newHeights = cur.terrainData.GetHeights(0, 0, heightResolution, heightResolution);

        for (int i = 0; i < heightResolution; i++)
        {
            if (bottom != null)
                newHeights[0, i] = bottomHeights[heightResolution - 1, i];
        }
        cur.terrainData.SetHeights(0, 0, newHeights);
    }

    void StitchTopRight(Terrain cur, Terrain top, Terrain right, int heightResolution)
    {
        float[,] newHeights = new float[heightResolution, heightResolution];
        float[,] rightHeights = new float[heightResolution, heightResolution];
        float[,] topHeights = new float[heightResolution, heightResolution];

        if (right != null)
        {
            rightHeights = right.terrainData.GetHeights(0, 0, heightResolution, heightResolution);
        }

        if (top != null)
        {
            topHeights = top.terrainData.GetHeights(0, 0, heightResolution, heightResolution);
        }

        if (top != null || right != null)
        {
            newHeights = cur.terrainData.GetHeights(0, 0, heightResolution, heightResolution);

            for (int i = 0; i < heightResolution; i++)
            {
                if (top != null)
                    newHeights[heightResolution - 1, i] = topHeights[0, i];

                if (right != null)
                    newHeights[i, heightResolution - 1] = rightHeights[i, 0];
            }
            cur.terrainData.SetHeights(0, 0, newHeights);
        }
    }

    void StitchBottomAndRight(Terrain cur, Terrain right, Terrain bottom, int heightResolution)
    {
        float[,] newHeights = new float[heightResolution, heightResolution];
        float[,] rightHeights = new float[heightResolution, heightResolution];
        float[,] bottomHeights = new float[heightResolution, heightResolution];

        if (right != null)
        {
            rightHeights = right.terrainData.GetHeights(0, 0, heightResolution, heightResolution);
        }
        if (bottom != null)
        {
            bottomHeights = bottom.terrainData.GetHeights(0, 0, heightResolution, heightResolution);
        }

        if (right != null || bottom != null)
        {
            newHeights = cur.terrainData.GetHeights(0, 0, heightResolution, heightResolution);

            for (int i = 0; i < heightResolution; i++)
            {
                if (right != null)
                    newHeights[i, heightResolution - 1] = rightHeights[i, 0];

                if (bottom != null)
                    newHeights[0, i] = bottomHeights[heightResolution - 1, i];
            }
            cur.terrainData.SetHeights(0, 0, newHeights);
        }
    }

    void StitchTopAndLeft(Terrain cur, Terrain top, Terrain left, int heightResolution)
    {
        float[,] newHeights = new float[heightResolution, heightResolution];
        float[,] topHeights = new float[heightResolution, heightResolution];
        float[,] leftHeights = new float[heightResolution, heightResolution];
        
        if (top != null)
        {
            topHeights = top.terrainData.GetHeights(0, 0, heightResolution, heightResolution);
        }
        if (left != null)
        {
            leftHeights = left.terrainData.GetHeights(0, 0, heightResolution, heightResolution);
        }

        if (top != null || left != null)
        {
            newHeights = cur.terrainData.GetHeights(0, 0, heightResolution, heightResolution);

            for (int i = 0; i < heightResolution; i++)
            {
                if (top != null)
                    newHeights[heightResolution - 1, i] = topHeights[0, i];

                if (left != null)
                    newHeights[i, 0] = leftHeights[i, heightResolution - 1];
            }
            cur.terrainData.SetHeights(0, 0, newHeights);
        }
    }
}