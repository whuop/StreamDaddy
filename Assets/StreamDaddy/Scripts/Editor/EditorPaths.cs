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

        private static string WORLD_STREAMS_DIRECTORY_PATH = "Streams/";

        public static string GetWorldPath(string worldName)
        {
            string path = WORLDS_DIRECTORY_PATH + worldName + "/";
            PathUtils.EnsurePathExists(Application.dataPath + "/" + path);
            return "Assets/" + path;
        }

        public static string GetWorldChunkDataPath(string worldName)
        {
            string path = GetWorldPath(worldName) + RELATIVE_CHUNK_DATA_DIRECTORY_PATH;
            PathUtils.EnsurePathExists(Application.dataPath + "/" + path);
            return path;
        }

        public static string GetWorldChunkLayoutPath(string worldName)
        {
            string path = GetWorldPath(worldName) + RELATIVE_CHUNK_LAYOUT_DIRECTORY_PATH;
            PathUtils.EnsurePathExists(Application.dataPath + "/" + path);
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
    }
}


