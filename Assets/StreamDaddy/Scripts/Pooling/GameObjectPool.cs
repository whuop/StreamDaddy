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

    public class BoxCollideable
    {
        public GameObject GameObject;
        public BoxCollider BoxCollider;
    }

    public class SphereCollideable
    {
        public GameObject GameObject;
        public SphereCollider SphereCollider;
    }

    public class MeshCollideable
    {
        public GameObject GameObject;
        public MeshCollider MeshCollider;
    }

    public class GameObjectPool
    {
        private static Queue<Renderable> m_renderables = new Queue<Renderable>();
        private static Queue<BoxCollideable> m_boxColliders = new Queue<BoxCollideable>();
        private static Queue<SphereCollideable> m_sphereColliders = new Queue<SphereCollideable>();
        private static Queue<MeshCollideable> m_meshColliders = new Queue<MeshCollideable>();

        public static void PreWarm(int rendererCount, int boxColliderCount, int sphereColliderCount, int meshColliderCount)
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

            while(m_boxColliders.Count < boxColliderCount)
            {
                GameObject go = new GameObject("PooledBoxCollider_" + m_boxColliders.Count, typeof(BoxCollider));
                go.SetActive(false);

                BoxCollideable collideable = new BoxCollideable()
                {
                    GameObject = go,
                    BoxCollider = go.GetComponent<BoxCollider>()
                };

                m_boxColliders.Enqueue(collideable);
            }

            while (m_sphereColliders.Count < sphereColliderCount)
            {
                GameObject go = new GameObject("PooledSphereCollider_" + m_sphereColliders.Count, typeof(SphereCollider));
                go.SetActive(false);

                SphereCollideable collideable = new SphereCollideable()
                {
                    GameObject = go,
                    SphereCollider = go.GetComponent<SphereCollider>()
                };

                m_sphereColliders.Enqueue(collideable);
            }

            while (m_meshColliders.Count < meshColliderCount)
            {
                GameObject go = new GameObject("PooledMeshCollider_" + m_meshColliders.Count, typeof(MeshCollider));
                go.SetActive(false);

                MeshCollideable collideable = new MeshCollideable()
                {
                    GameObject = go,
                    MeshCollider = go.GetComponent<MeshCollider>()
                };

                m_meshColliders.Enqueue(collideable);
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

        public static BoxCollideable GetBoxCollider(Vector3 position, Vector3 rotation, Vector3 scale, Vector3 center, Vector3 size)
        {
            var collideable = m_boxColliders.Dequeue();

            collideable.GameObject.transform.position = position;
            collideable.GameObject.transform.rotation = Quaternion.Euler(rotation);
            collideable.GameObject.transform.localScale = scale;
            
            collideable.BoxCollider.center = center;
            collideable.BoxCollider.size = size;
            
            collideable.GameObject.SetActive(true);
            return collideable;
        }

        public static void ReturnBoxCollideable(BoxCollideable collideable)
        {
            m_boxColliders.Enqueue(collideable);
            collideable.GameObject.SetActive(false);
        }

        public static SphereCollideable GetSphereCollider(Vector3 position, Vector3 rotation, Vector3 scale, Vector3 center, float radius)
        {
            var collideable = m_sphereColliders.Dequeue();

            collideable.GameObject.transform.position = position;
            collideable.GameObject.transform.rotation = Quaternion.Euler(rotation);
            collideable.GameObject.transform.localScale = scale;

            collideable.SphereCollider.center = center;
            collideable.SphereCollider.radius = radius;

            collideable.GameObject.SetActive(true);
            return collideable;
        }

        public static void ReturnSphereCollideable(SphereCollideable collideable)
        {
            m_sphereColliders.Enqueue(collideable);
            collideable.GameObject.SetActive(false);
        }

        public static MeshCollideable GetMeshCollider(Vector3 position, Vector3 rotation, Vector3 scale, Mesh mesh)
        {
            var collideable = m_meshColliders.Dequeue();
            collideable.MeshCollider.sharedMesh = mesh;

            collideable.GameObject.transform.position = position;
            collideable.GameObject.transform.rotation = Quaternion.Euler(rotation);
            collideable.GameObject.transform.localScale = scale;
            
            collideable.GameObject.SetActive(true);
            return collideable;
        }

        public static void ReturnMeshCollider(MeshCollideable collideable)
        {
            m_meshColliders.Enqueue(collideable);
            collideable.GameObject.SetActive(false);
        }
    }
}


