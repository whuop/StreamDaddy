using StreamDaddy.Editor.Configs.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StreamDaddy.Editor.Configs
{
    public class ConfigBase : ScriptableObject
    {
        ConfigPathAttribute m_pathAttribute;

        protected ConfigBase()
        {
            
        }

        public static void Save<T>(T obj) where T : ConfigBase
        {
            var pathAttribute = (ConfigPathAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(ConfigPathAttribute));

            T asset = AssetDatabase.LoadAssetAtPath<T>(pathAttribute.AssetPath);
            if (asset == null)
            {
                AssetDatabase.CreateAsset(obj, pathAttribute.AssetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        public static T Load<T>() where T : ConfigBase
        {
            var pathAttribute = (ConfigPathAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(ConfigPathAttribute));

            Debug.Log("Loading StreamDaddy config at path:" + pathAttribute.AssetPath);

            T config = AssetDatabase.LoadAssetAtPath<T>(pathAttribute.AssetPath);
            if (config == null)
            {
                Debug.LogError("Could not load config at path: " + pathAttribute.AssetPath);
                return null;
            }
            return config;
        }
    }
}


