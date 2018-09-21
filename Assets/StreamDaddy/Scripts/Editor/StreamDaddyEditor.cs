using StreamDaddy.Editor.Chunking;
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

        private Vector3Int m_chunkSize = new Vector3Int();
        private EditorChunkManager m_chunkManager = new EditorChunkManager();

        private bool m_isInitialized = false;

        private void Initialize()
        {

        }

        private void OnGUI()
        {
            if (!m_isInitialized)
            {
                m_isInitialized = true;
                Initialize();
            }

            Vector3Int newChunkSize = EditorGUILayout.Vector3IntField("Chunk Size", m_chunkSize);
            if (m_chunkSize != newChunkSize)
            {
                m_chunkSize = newChunkSize;
                m_chunkManager.SetChunkSizeAndClearManager(m_chunkSize);
            }


            if (GUILayout.Button("Chunk World"))
            {
                Debug.Log("Chunking");
            }
        }
    }
}


