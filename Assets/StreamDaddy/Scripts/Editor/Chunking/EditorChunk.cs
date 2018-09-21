using StreamDaddy.Chunking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Editor.Chunking
{
    public class EditorChunk
    {
        private ChunkID m_id;
        public ChunkID ChunkID { get { return m_id; } }

        private List<GameObject> m_children = new List<GameObject>();

        public EditorChunk()
        {

        }

        public void AddChild(GameObject go)
        {
            m_children.Add(go);
        }

        public void RemoveChild(GameObject go)
        {
            m_children.Remove(go);
        }

        public GameObject[] GetAllChildren()
        {
            return m_children.ToArray();
        }
    }
}


