using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.AssetManagement
{
    public class AssetContainer<T> : IAssetContainer<T> where T : UnityEngine.Object
    {
        private Dictionary<string, T> m_assets = new Dictionary<string, T>();

        public AssetContainer()
        {

        }

        public void Add(string name, T asset)
        {
            m_assets.Add(name, asset);
        }

        public T Get(string name)
        {
            if (!m_assets.ContainsKey(name))
            {
                Debug.LogError("Could not find asset with key: " + name + " of type: " + typeof(T).Name);
                return default(T);
            }
            return m_assets[name];
        }

        public void Remove(string name)
        {
            m_assets.Remove(name);
        }

        public T[] GetAllAssets()
        {
            T[] array = new T[m_assets.Count];
            m_assets.Values.CopyTo(array, 0);
            return array;
        }

        public bool Contains(string name)
        {
            return m_assets.ContainsKey(name);
        }
    }
}


