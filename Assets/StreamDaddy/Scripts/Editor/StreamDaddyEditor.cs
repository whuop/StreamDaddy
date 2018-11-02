﻿using StreamDaddy.Editor.Chunking;
using StreamDaddy.Editor.Configs;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using StreamDaddy.Editor.Tasks;

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

        private Terrain m_terrainToSplit;
        private Material m_terrainMeshMaterial;

        private TaskChain m_taskChain;

        private BuildChunkLayoutTask.BuildChunkLayoutResult m_chunkLayoutResult;

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

            m_terrainMeshMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/StreamDaddy/Materials/MeshTerrain.mat");
            
            m_taskChain = new TaskChain();
        }

        private void OnGUI()
        {

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(m_worldNameProp);
            EditorGUILayout.PropertyField(m_chunkSizeProp);
            m_terrainToSplit = (Terrain)EditorGUILayout.ObjectField("Terrain to split", m_terrainToSplit, typeof(Terrain), true);

            if (EditorGUI.EndChangeCheck())
            {
                //  Apply changes to the serialized config, making it save changes. 
                m_serializedConfig.ApplyModifiedProperties();
            }

            if (GUILayout.Button("Split Terrain"))
            {
                new SplitTerrainTask().Execute(m_worldNameProp.stringValue, m_terrainToSplit, m_chunkSizeProp.vector3IntValue);
            }

            if (GUILayout.Button("Terrain To Mesh"))
            {
                List<Terrain> terrains = new List<Terrain>();
                terrains.Add(m_terrainToSplit);
                
                new TerrainToMeshTask().Execute(m_worldNameProp.stringValue, terrains, m_terrainMeshMaterial);
            }
            
            if (GUILayout.Button("Chunk World"))
            {
                m_chunkManager = new EditorChunkManager(m_chunkSizeProp.vector3IntValue);
                new ChunkWorldTask().Execute(m_chunkManager);
                GUI.changed = true;
            }
            
            if (GUILayout.Button("Export Chunk Layouts"))
            {
                m_chunkLayoutResult = new BuildChunkLayoutTask.BuildChunkLayoutResult();
                new BuildChunkLayoutTask().Execute(m_worldNameProp.stringValue, m_chunkManager.Chunks, ref m_chunkLayoutResult);
            }

            if (GUILayout.Button("Export Assets"))
            {
                new ExportChunkAssetsTask().Execute(m_worldNameProp.stringValue, m_chunkManager.Chunks);
            }

            if (GUILayout.Button("Export World Stream"))
            {
                List<string> assetBundles = new List<string>();
                assetBundles.Add(m_worldNameProp.stringValue + "_chunkassets");
                new BuildWorldStreamTask().Execute(m_worldNameProp.stringValue, m_chunkLayoutResult.ChunkLayoutBundle, m_chunkSizeProp.vector3IntValue, m_chunkLayoutResult.ChunkLayoutNames, assetBundles);
            }

            if (GUI.changed)
                SceneView.RepaintAll();
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (m_chunkManager != null)
                m_chunkManager.Draw();
        }
    }
}


