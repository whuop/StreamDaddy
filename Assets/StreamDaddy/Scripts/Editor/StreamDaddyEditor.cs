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
        private EditorChunkManager m_chunkManager;
        
        private StreamDaddyConfig m_config;

        //  Serialied config variables
        private SerializedObject m_serializedConfig;
        private SerializedProperty m_chunkSizeProp;
        private SerializedProperty m_worldNameProp;

        private void OnDestroy()
        {
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        }

        private void OnEnable()
        {
            Initialize();
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
        }

        private void OnDisable()
        {
            Debug.Log("Disabled");
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
            m_worldNameProp = m_serializedConfig.FindProperty("m_worldName");

            m_chunkManager = new EditorChunkManager();
        }

        private void OnGUI()
        {

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(m_worldNameProp);
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
                m_chunkManager.BeginWorld(m_worldNameProp.stringValue);
                m_chunkManager.ExportAllChunkAssets();
            }

            if (GUILayout.Button("Export World"))
            {
                m_chunkManager.ExportAllChunkLayouts();
            }

            if (GUILayout.Button("Build AssetBundles"))
            {
                m_chunkManager.EndWorld();
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

        void OnSceneGUI(SceneView sceneView)
        {
            m_chunkManager.Draw();

            if (GUI.changed)
                SceneView.RepaintAll();
        }
    }
}


