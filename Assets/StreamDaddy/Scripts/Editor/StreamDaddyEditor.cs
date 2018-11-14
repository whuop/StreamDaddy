using StreamDaddy.Editor.Chunking;
using StreamDaddy.Editor.Configs;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using StreamDaddy.Editor.Tasks;
using UnityEditor.SceneManagement;

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

        private SplitTerrainTask.SplitTerrainResult m_splitTerrainResult;
        private BuildChunkLayoutTask.BuildChunkLayoutResult m_chunkLayoutResult;

        public Mesh m_mesh;

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

            m_mesh = (Mesh)EditorGUILayout.ObjectField("Mesh to LOD", m_mesh, typeof(Mesh), true);

            if (EditorGUI.EndChangeCheck())
            {
                //  Apply changes to the serialized config, making it save changes. 
                m_serializedConfig.ApplyModifiedProperties();
            }


            if (GUILayout.Button("Generate LODs"))
            {
                new GenerateMeshLodsTask().Execute(m_worldNameProp.stringValue, m_chunkManager.Chunks);
            }

            if (GUILayout.Button("Split Terrain"))
            {
                m_splitTerrainResult = new SplitTerrainTask.SplitTerrainResult();
                new SplitTerrainTask().Execute(m_worldNameProp.stringValue, m_terrainToSplit, m_chunkSizeProp.vector3IntValue, ref m_splitTerrainResult);
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

            /*if (GUILayout.Button("Export Assets"))
            {
                new ExportChunkAssetsTask().Execute(m_worldNameProp.stringValue, m_chunkManager.Chunks);
            }*/

            if (GUILayout.Button("Export World Stream"))
            {
                new BuildWorldStreamTask().Execute(m_worldNameProp.stringValue, m_chunkLayoutResult.ChunkLayoutBundle, m_chunkSizeProp.vector3IntValue, m_chunkLayoutResult.ChunkLayoutReferences, m_chunkLayoutResult.AssetBundles);
            }

            if (GUILayout.Button("Construct Stream Scene for World"))
            {
                List<Terrain> terrainsToMove = new List<Terrain>(GameObject.FindObjectsOfType<Terrain>());
                terrainsToMove.Remove(m_terrainToSplit);

                new CreateStreamedSceneTask().Execute(m_worldNameProp.stringValue, terrainsToMove);
            }

            if (GUILayout.Button("Full Export (excluding terrain split)"))
            {
                //  Chunk World
                m_chunkManager = new EditorChunkManager(m_chunkSizeProp.vector3IntValue);
                new ChunkWorldTask().Execute(m_chunkManager);
                GUI.changed = true;

                //  Export chunk layouts
                m_chunkLayoutResult = new BuildChunkLayoutTask.BuildChunkLayoutResult();
                new BuildChunkLayoutTask().Execute(m_worldNameProp.stringValue, m_chunkManager.Chunks, ref m_chunkLayoutResult);

                //  Export Assets
                new ExportChunkAssetsTask().Execute(m_worldNameProp.stringValue, m_chunkManager.Chunks);

                //  Export WorldStream scriptable object
                List<string> assetBundles = new List<string>();
                assetBundles.Add(m_worldNameProp.stringValue + "_chunkassets");
                new BuildWorldStreamTask().Execute(m_worldNameProp.stringValue, m_chunkLayoutResult.ChunkLayoutBundle, m_chunkSizeProp.vector3IntValue, m_chunkLayoutResult.ChunkLayoutReferences, assetBundles);

                //  Construct the stream scene
                List<Terrain> terrainsToMove = new List<Terrain>(GameObject.FindObjectsOfType<Terrain>());
                terrainsToMove.Remove(m_terrainToSplit);

                new CreateStreamedSceneTask().Execute(m_worldNameProp.stringValue, terrainsToMove);
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


