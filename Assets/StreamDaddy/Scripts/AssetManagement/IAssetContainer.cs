using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.AssetManagement
{
    public interface IAssetContainer<T> where T : UnityEngine.Object
    {
        T Get(string name);
        void Add(string name, T asset);
        void Remove(string name);
        bool Contains(string name);
        T[] GetAllAssets();
    }
}


