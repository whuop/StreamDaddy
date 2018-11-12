using StreamDaddy.Chunking;
using StreamDaddy.Streaming;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.AssetManagement
{
    public class AddressablesLoader
    {
        private List<UnityEngine.ResourceManagement.IAsyncOperation<AssetChunkData>> m_layoutsLoading = new List<UnityEngine.ResourceManagement.IAsyncOperation<AssetChunkData>>();
        private List<AssetChunkData> m_chunkLayouts = new List<AssetChunkData>();

        public delegate void FinishedLoadingLayoutsDelegate(List<AssetChunkData> chunkLayouts);

        private FinishedLoadingLayoutsDelegate m_onFinishedLoadingLayouts;

        public AddressablesLoader(FinishedLoadingLayoutsDelegate onFinishedLoadingLayouts)
        {
            m_onFinishedLoadingLayouts = onFinishedLoadingLayouts;
        }

        public void LoadWorldLayouts(WorldStream stream)
        {
            for(int i = 0; i < stream.ChunkLayoutReferences.Count; i++)
            {
                var layoutLoader = stream.ChunkLayoutReferences[i].LoadAsset<AssetChunkData>();
                layoutLoader.Completed += LayoutLoaderCompleted;
                m_layoutsLoading.Add(layoutLoader);
            }
        }

        public void LoadChunkAssets(ChunkID chunk, AssetChunkData chunkAssets)
        {

        }

        private void LayoutLoaderCompleted(UnityEngine.ResourceManagement.IAsyncOperation<AssetChunkData> obj)
        {
            //  Remove this async operation from the list of loading layouts. 
            m_layoutsLoading.Remove(obj);
            //  Add layout to the list of loaded layouts.
            m_chunkLayouts.Add(obj.Result);
            
            if (m_layoutsLoading.Count == 0)
            {
                Debug.Log("Finished loading world stream layouts");
                if (m_onFinishedLoadingLayouts != null)
                    m_onFinishedLoadingLayouts(m_chunkLayouts);
            }
        }
    }
}


