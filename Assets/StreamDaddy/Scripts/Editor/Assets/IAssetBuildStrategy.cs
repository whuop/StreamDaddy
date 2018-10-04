using StreamDaddy.Editor.Chunking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Editor.Assets
{      
    public interface IAssetBuildStrategy
    {
        void BuildChunkAssets(string worldName, EditorChunk chunk, List<string> assetBundles);
        string BuildChunkLayout(string worldName, EditorChunk chunk);
    }
}