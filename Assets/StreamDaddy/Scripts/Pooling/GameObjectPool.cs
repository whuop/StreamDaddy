using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Pooling
{
    public class Renderable
    {
        public GameObject GameObject;
        public MeshFilter Filter;
        public MeshRenderer Renderer;
    }

    public class GameObjectPool
    {
        private static Queue<Renderable> m_renderables = new Queue<Renderable>();

        public static void PreWarm(int rendererCount)
        {
            while(m_renderables.Count < rendererCount)
            {
                GameObject go = new GameObject("PooledRenderer_" + m_renderables.Count, typeof(MeshRenderer), typeof(MeshFilter));
                go.SetActive(false);

                Renderable renderable = new Renderable()
                {
                    GameObject = go,
                    Renderer = go.GetComponent<MeshRenderer>(),
                    Filter = go.GetComponent<MeshFilter>()
                };

                m_renderables.Enqueue(renderable);
            }
        }

        public static Renderable GetRenderer(Mesh mesh, Material[] materials, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            var renderable = m_renderables.Dequeue();

            renderable.Filter.sharedMesh = mesh;
            renderable.Renderer.sharedMaterials = materials;
            renderable.GameObject.transform.position = position;
            renderable.GameObject.transform.rotation = Quaternion.Euler(rotation);
            renderable.GameObject.transform.localScale = scale;

            renderable.GameObject.SetActive(true);
            return renderable;
        }

        public static void ReturnRenderer(Renderable renderable)
        {
            m_renderables.Enqueue(renderable);
            renderable.GameObject.SetActive(false);
        }
    }
}


