using StreamDaddy.Editor.TerrainTools;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Editor.Tasks
{
    public class SplitTerrainTask : Task
    {
        public struct SplitTerrainResult
        {
            public List<Terrain> TerrainSplits;
            public Texture2D SplatMap;
        }

        public SplitTerrainTask() : base("Split Terrain")
        {

        }

        public bool Execute(string worldName, Terrain terrain, Vector3Int chunkSize, ref SplitTerrainResult result)
        {
            if (terrain == null)
            {
                LogError("Terrain is null, task failed!");
            }

            if (string.IsNullOrEmpty(worldName))
            {
                LogError("World name is null or empty, task failed!");
            }
            
            result.TerrainSplits = TerrainSplitter.SplitIntoChunks(chunkSize.x, chunkSize.z, terrain, EditorPaths.GetSplitTerrainPath(worldName));
            return true;
        }
    }

}

