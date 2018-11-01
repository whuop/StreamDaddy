using StreamDaddy.Chunking;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StreamDaddy.Editor.Chunking
{
    public class EditorChunk
    {
        private ChunkID m_id;
        public ChunkID ChunkID { get { return m_id; } }

        private List<GameObject> m_children = new List<GameObject>();

        private Bounds m_boundingBox;

        public EditorChunk(ChunkID id, Vector3Int size)
        {
            m_id = id;
            m_boundingBox = new Bounds(m_id.ID, size);

            Debug.Log("Created chunk with size: " + size.x + " " + size.y + " " + size.z);
        }

        public void AddChild(GameObject go)
        {
            m_children.Add(go);
        }

        public bool HasChild(GameObject go)
        {
            return m_children.Contains(go);
        }

        public void RemoveChild(GameObject go)
        {
            m_children.Remove(go);
        }

        public GameObject[] GetAllChildren()
        {
            return m_children.ToArray();
        }

        public bool ContainsPoint(Vector3 point)
        {
            if (m_boundingBox.Contains(point))
            {
                return true;
            }
            return false;
        }

        public void Draw()
        {
            Color tempColor = Handles.color;

            Vector3 center = new Vector3(
                m_boundingBox.center.x * m_boundingBox.size.x + m_boundingBox.size.x * 0.5f,
                m_boundingBox.center.y * m_boundingBox.size.y + m_boundingBox.size.y * 0.5f,
                m_boundingBox.center.z * m_boundingBox.size.z + m_boundingBox.size.z * 0.5f
                );

            Handles.Label(center, m_id.ToString());

            Handles.color = Color.green;
            Handles.DrawWireCube(center, m_boundingBox.size);
            Handles.color = tempColor;
        }
    }
}


