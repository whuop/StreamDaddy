using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Pooling
{
    public class GameObjectPool
    {
        private static Queue<GameObject> m_gameObjectPool = new Queue<GameObject>();
        private static Queue<MeshFilter> m_filterPool = new Queue<MeshFilter>();
        private static Queue<MeshRenderer> m_rendererPool = new Queue<MeshRenderer>();

        public static void PreWarm(int rendererCount)
        {
            while(m_gameObjectPool.Count < rendererCount)
            {
                GameObject go = new GameObject("PooledRenderer_" + m_rendererPool.Count, typeof(MeshRenderer), typeof(MeshFilter));
                go.SetActive(false);
                m_gameObjectPool.Enqueue(go);
                m_filterPool.Enqueue(go.GetComponent<MeshFilter>());
                m_rendererPool.Enqueue(go.GetComponent<MeshRenderer>());
            }
        }

        public static GameObject GetRenderer(Mesh mesh, Material[] materials, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            var go = m_gameObjectPool.Dequeue();
            var meshFilter = m_filterPool.Dequeue();
            var meshRenderer = m_rendererPool.Dequeue();

            meshFilter.sharedMesh = mesh;
            meshRenderer.sharedMaterials = materials;
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(rotation);
            go.transform.localScale = scale;

            go.SetActive(true);
            return go;
        }

        public static void ReturnRenderer(GameObject go)
        {
            m_gameObjectPool.Enqueue(go);
            m_filterPool.Enqueue(go.GetComponent<MeshFilter>());
            m_rendererPool.Enqueue(go.GetComponent<MeshRenderer>());

            go.SetActive(false);
        }
    }
}


