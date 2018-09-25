using StreamDaddy.Editor.Chunking;
using StreamDaddy.Editor.Configs;
using StreamDaddy.Editor.Configs.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StreamDaddy.Editor
{
    public class StreamDaddyEditor : EditorWindow
    {
        [MenuItem("Window/StreamDaddy")]
        static void ShowWindow()
        {
            StreamDaddyEditor window = (StreamDaddyEditor)EditorWindow.GetWindow(typeof(StreamDaddyEditor));
            window.Show();
        }
        private EditorChunkManager m_chunkManager = new EditorChunkManager();

        private bool m_isInitialized = false;
        
        private StreamDaddyConfig m_config;

        //  Serialied config variables
        private SerializedObject m_serializedConfig;
        private SerializedProperty m_chunkSizeProp;

        private void OnFocus()
        {
            Debug.Log("OnFocus");
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
        }

        private void OnDestroy()
        {
            Debug.Log("OnDestroy");
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        }

        private void OnEnable()
        {
            Debug.Log("OnEnable");
        }

        private void OnDisable()
        {
            Debug.Log("OnDestroy");
        }

        private void Initialize()
        {
            m_config = ConfigBase.Load<StreamDaddyConfig>();
            if (m_config == null)
            {
                Debug.Log("Could not find config, creating new!");
                m_config = ScriptableObject.CreateInstance<StreamDaddyConfig>();
                ConfigBase.Save(m_config);
            }

            m_serializedConfig = new SerializedObject(m_config);
            m_chunkSizeProp = m_serializedConfig.FindProperty("m_chunkSize");
        }

        private void OnGUI()
        {
            if (!m_isInitialized)
            {
                m_isInitialized = true;
                Initialize();
            }

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(m_chunkSizeProp);
            
            if (EditorGUI.EndChangeCheck())
            {
                //  Apply changes to the serialized config, making it save changes. 
                m_serializedConfig.ApplyModifiedProperties();
            }
            
            if (GUILayout.Button("Chunk World"))
            {
                //  Apply new chunk size to the Chunk Manager. 
                m_chunkManager.SetChunkSizeAndClearManager(m_config.ChunkSize);
                ChunkWorld();
            }

            if (GUILayout.Button("Export Assets"))
            {
                m_chunkManager.ExportAllChunkAssets();
                m_chunkManager.BuildAllAssetBundles();
            }

            if (GUILayout.Button("Export World"))
            {
                m_chunkManager.ExportAllChunkLayouts();
                m_chunkManager.BuildAllAssetBundles();
            }
        }

        private void ChunkWorld()
        {
            GameObject[] allGos = GameObject.FindObjectsOfType<GameObject>();
            foreach(var go in allGos)
            {
                if (!go.activeInHierarchy)
                    continue;
                m_chunkManager.AddGameObject(go);
            }
            GUI.changed = true;
        }

        private void ExportWorld()
        {

        }

        void OnSceneGUI(SceneView sceneView)
        {
            m_chunkManager.Draw();

            if (GUI.changed)
                SceneView.RepaintAll();
        }
    }
}


