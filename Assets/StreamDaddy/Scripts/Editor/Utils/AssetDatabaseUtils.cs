using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StreamDaddy.Editor.Utils
{
    public static class AssetDatabaseUtils
    {
        public static T CreateOrReplaceAsset<T>(T asset, string path) where T : Object
        {
            T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (existingAsset == null)
            {
                AssetDatabase.CreateAsset(asset, path);
                existingAsset = asset;
                Debug.Log("Created asset: " + typeof(T).Name + " at path: " + path);
            }
            else
            {
                EditorUtility.CopySerialized(asset, existingAsset);
                Debug.Log("Replaced asset: " + typeof(T).Name + " at path: " + path);
            }

            return existingAsset;
        }
    }
}



