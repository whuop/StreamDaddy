using StreamDaddy.Editor.Chunking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Editor.Assets
{
    public interface IAssetBuildStrategy
    {
        void BuildChunkAssets(EditorChunk chunk);
        void BuildChunkLayout(EditorChunk chunk);
    }
}