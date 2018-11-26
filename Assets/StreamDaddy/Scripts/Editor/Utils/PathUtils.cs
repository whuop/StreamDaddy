using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Editor.Utils
{
    public class PathUtils
    {
        public static void EnsurePathExists(string path)
        {
            bool validPath = true;
            try
            {
                System.IO.Path.GetFullPath(path);
            }
            catch(Exception e)
            {
                validPath = false;
            }

            if (!validPath)
            {
                Debug.LogError("Path: " + path + " is not a valid path!");
                return;
            }

            System.IO.Directory.CreateDirectory(path);
        }

        public static string ExtractFileFormatFromPath(string path)
        {
            string[] split = path.Split('.');
            string format = split[split.Length - 1];
            return format;
        }

        public static string RemoveLastInPath(string path)
        {
            string result = "";

            string[] splits = path.Split(new[] { "/" }, StringSplitOptions.None);

            for (int i = 0; i < splits.Length - 1; i++)
            {
                result += splits[i] + "/";
            }

            return result;
        }
    }
}


