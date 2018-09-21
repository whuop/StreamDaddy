using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Editor.Assets
{
    public interface IAssetBuildStrategy
    {
        void BuildAssets(GameObject[] gameObjects);
    }
}