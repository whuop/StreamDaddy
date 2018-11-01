using StreamDaddy.Editor.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Editor
{
    public static class EditorPaths
    {
        public static string STREAMING_DIRECTORY_PATH = Application.streamingAssetsPath;
        private static string WORLDS_DIRECTORY_PATH = "StreamDaddy/Worlds/";
        private static string RELATIVE_CHUNK_DATA_DIRECTORY_PATH= "ChunkData/";
        private static string RELATIVE_CHUNK_LAYOUT_DIRECTORY_PATH = "ChunkLayout/";

        private static string RELATIVE_SPLIT_TERRAIN_PATH = "Terrains/";
        private static string RELATIVE_TERRAIN_MESH_DIRECTORY_PATH = "TerrainMesh/";
        private static string RELATIVE_TERRAIN_MESH_SPLAT_PATH = "TerrainMesh/Splats/";

        private static string WORLD_STREAMS_DIRECTORY_PATH = "Streams/";

        private static string APPLICATION_DATAPATH = Application.dataPath.Replace("Assets", string.Empty);

        public static string GetWorldPath(string worldName)
        {
            string path = WORLDS_DIRECTORY_PATH + worldName + "/";
            PathUtils.EnsurePathExists(Application.dataPath + "/" + path);
            return "Assets/" + path;
        }

        public static string GetWorldChunkDataPath(string worldName)
        {
            string path = GetWorldPath(worldName) + RELATIVE_CHUNK_DATA_DIRECTORY_PATH;
            PathUtils.EnsurePathExists(APPLICATION_DATAPATH + path);
            return path;
        }

        public static string GetWorldChunkLayoutPath(string worldName)
        {
            string path = GetWorldPath(worldName) + RELATIVE_CHUNK_LAYOUT_DIRECTORY_PATH;
            PathUtils.EnsurePathExists(APPLICATION_DATAPATH + path);
            return path;
        }

        public static string GetStreamingAssetsFolder()
        {
            string path = STREAMING_DIRECTORY_PATH;
            PathUtils.EnsurePathExists(Application.dataPath + "/" + path);
            return "Assets/" + path;
        }

        public static string GetWorldStreamsFolder()
        {
            string path = WORLD_STREAMS_DIRECTORY_PATH;
            PathUtils.EnsurePathExists(Application.dataPath + "/" + path);
            return "Assets/" + path;
        }

        public static string GetSplitTerrainPath(string worldName)
        {
            string path = GetWorldPath(worldName) + RELATIVE_SPLIT_TERRAIN_PATH;
            PathUtils.EnsurePathExists(APPLICATION_DATAPATH + path);
            return path;
        }

        public static string GetTerrainMeshPath(string worldName)
        {
            string path = GetWorldPath(worldName) + RELATIVE_TERRAIN_MESH_DIRECTORY_PATH;
            PathUtils.EnsurePathExists(APPLICATION_DATAPATH + path);
            return path;
        }

        public static string GetTerrainMeshSplatPath(string worldName)
        {
            string path = GetWorldPath(worldName) + RELATIVE_TERRAIN_MESH_SPLAT_PATH;
            PathUtils.EnsurePathExists(APPLICATION_DATAPATH + path);
            return path;
        }
    }
}


