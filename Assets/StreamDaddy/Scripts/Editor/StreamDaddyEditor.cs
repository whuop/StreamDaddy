using StreamDaddy.Editor.Chunking;
using StreamDaddy.Editor.Configs;
using StreamDaddy.Editor.Configs.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using StreamDaddy.Editor.TerrainTools;
using StreamDaddy.TerrainToMesh.Editor;
using StreamDaddy.Editor.Tasks;
using StreamDaddy.Streaming;

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

            m_chunkManager = new EditorChunkManager();

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
                new ChunkWorldTask().Execute(m_chunkManager, m_chunkSizeProp.vector3IntValue);
                GUI.changed = true;
            }
            
            if (GUILayout.Button("Export Chunk Layouts"))
            {
                new BuildChunkLayoutTask().Execute(m_worldNameProp.stringValue, m_chunkManager.Chunks);
            }

            if (GUILayout.Button("Export Assets"))
            {
                new ExportChunkAssetsTask().Execute(m_worldNameProp.stringValue, m_chunkManager.Chunks);
            }

            if (GUILayout.Button("Export World Stream"))
            {
                List<string> chunkLayoutNames = new List<string>();
                foreach(var chunk in m_chunkManager.Chunks)
                {
                    chunkLayoutNames.Add("chunklayout_" + chunk.ChunkID.X + "_" + chunk.ChunkID.Y + " " + chunk.ChunkID.Z);
                }

                List<string> assetBundles = new List<string>();
                assetBundles.Add(m_worldNameProp.stringValue + "_chunkassets");
                new BuildWorldStreamTask().Execute(m_worldNameProp.stringValue, m_worldNameProp + "_chunkassets", m_chunkSizeProp.vector3IntValue, chunkLayoutNames, assetBundles);
            }
        }

        private void SplitTerrain()
        {
            TerrainSplitter.SplitIntoChunks(m_chunkSizeProp.vector3IntValue.x, m_chunkSizeProp.vector3IntValue.z, m_terrainToSplit, "Assets/StreamDaddy/Worlds/" + m_worldNameProp.stringValue + "/Terrains/");
        }

        private void ChunkWorld()
        {
            var allMeshes = GameObject.FindObjectsOfType<MeshFilter>();
            var allBoxColliders = GameObject.FindObjectsOfType<BoxCollider>();
            var allSpherecolliders = GameObject.FindObjectsOfType<SphereCollider>();
            var allMeshColliders = GameObject.FindObjectsOfType<MeshCollider>();

            foreach(var mesh in allMeshes)
            {
                m_chunkManager.AddGameObject(mesh.gameObject);
            }

            foreach(var box in allBoxColliders)
            {
                m_chunkManager.AddGameObject(box.gameObject);
            }

            foreach(var sphere in allSpherecolliders)
            {
                m_chunkManager.AddGameObject(sphere.gameObject);
            }

            foreach(var meshCol in allMeshColliders)
            {
                m_chunkManager.AddGameObject(meshCol.gameObject);
            }

            /*GameObject[] allGos = GameObject.FindObjectsOfType<GameObject>();
            foreach(var go in allGos)
            {
                if (!go.activeInHierarchy)
                    continue;
                m_chunkManager.AddGameObject(go);
            }*/
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


