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
    }
}


