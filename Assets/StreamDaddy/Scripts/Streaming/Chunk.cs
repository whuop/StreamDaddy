using StreamDaddy.AssetManagement;
using StreamDaddy.Chunking;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Streaming
{
    public enum LoadState
    {
        Unloaded = 0,
        Unloading = 1,
        Loading = 2,
        Loaded = 3
    }
    
    public class Chunk
    {
        public enum JobType
        {
            Load,
            Unload
        };

        public class LodJob
        {
            public int LodLevel;
            public JobType JobType;
        }

        private ChunkID m_chunkID;
        public ChunkID ID { get { return m_chunkID; } }

        private int m_lodLevels = 4;
        private ChunkLOD[] m_chunkLODs;
        
        private MonoBehaviour m_coroutineStarter = null;
        
        private int m_currentlyLoadedLODLevel = -1;

        private Queue<ChunkLOD> m_jobQueue = new Queue<ChunkLOD>();

        //private Queue<ChunkLOD> m_loadQueue = new Queue<ChunkLOD>();
        //private Queue<ChunkLOD> m_unloadQueue = new Queue<ChunkLOD>();

        public Chunk(AssetChunkData data, MonoBehaviour coroutineStarter)
        {
            m_chunkID = new ChunkID(data.ChunkID);
            m_coroutineStarter = coroutineStarter;

            InitializeChunkLODs(m_lodLevels, data);
        }

        public Chunk(ChunkID id, MonoBehaviour coroutineStarter)
        {
            m_chunkID = id;
            m_coroutineStarter = coroutineStarter;
            InitializeChunkLODs(m_lodLevels, null);
        }

        private void InitializeChunkLODs(int lodLevels, AssetChunkData data)
        {
            m_chunkLODs = new ChunkLOD[lodLevels];
            //  Initialize Chunk LODS
            for (int i = 0; i < m_lodLevels; i++)
            {
                m_chunkLODs[i] = new ChunkLOD(data, i);
            }
        }

        public void SetTerrain(Terrain terrain, int lodLevel)
        {
            m_chunkLODs[lodLevel].SetTerrain(terrain);
        }

        public void SetTerrainMesh(Mesh terrainMesh, int lodLevel)
        {
            m_chunkLODs[lodLevel].SetTerrainMesh(terrainMesh);
        }

        private void OnFinishedChunkWork(int lodLevel)
        {
            var lod = m_chunkLODs[lodLevel];

            switch(lod.State)
            {
                case LoadState.Loaded:
                    //UnloadAllLODS(m_currentlyLoadedLODLevel);
                    break;
                case LoadState.Unloaded:
                    
                    break;
                case LoadState.Loading:
                case LoadState.Unloading:
                    Debug.LogError("Chunk should never be in Loading or Unloading state when OnFinished is reached!");
                    break;
            }
            
            CheckLODJobQueue();
        }

        private void UnloadAllLODS(int excludeLOD = -1)
        {
            //  If thre aren't any LOD layers, then just return.
            if (m_chunkLODs == null)
                return;

            int len = m_chunkLODs.Length;
            for(int i = 0; i < len; i++)
            {
                if (excludeLOD == i)
                    continue;

                var lod = m_chunkLODs[i];
                if (lod.State == LoadState.Loaded || lod.State == LoadState.Loading)
                {
                    if (!m_jobQueue.Contains(lod))
                        m_jobQueue.Enqueue(lod);
                }
            }
        }

        private void CheckLODJobQueue()
        {
            //  If there are no jobs in the queue, simply return.
            if (m_jobQueue.Count == 0)
            {
                return;
            }

            var nextJob = m_jobQueue.Peek();

            if (nextJob.State == LoadState.Loading || nextJob.State == LoadState.Unloading)
            {
                //  Return if one of the states is currently executing work.
                //  Allowing it to finish the work before starting the next work.
                return;
            }

            //  Remove the job from the queue, it's now allowed to run the job.
            m_jobQueue.Dequeue();

            switch(nextJob.State)
            {
                case LoadState.Loaded:
                    m_coroutineStarter.StartCoroutine(nextJob.Unload(OnFinishedChunkWork));
                    break;
                case LoadState.Unloaded:
                    m_coroutineStarter.StartCoroutine(nextJob.Load(OnFinishedChunkWork));
                    break;
            }
            

            //  If there is no Load job waiting, then go for the unload jobs.
            /*if (m_loadQueue.Count == 0)
            {
                //  If there is nothing to unload then early exit
                if (m_unloadQueue.Count == 0)
                    return;

                var nextUnload = m_unloadQueue.Peek();
                //  If the lod is currently in loading, then dont do anything, we have to wait for that to finish.
                if (nextUnload.State == LoadState.Loading)
                {
                    //Debug.LogError(string.Format("Chunk {0} is in state {1}", m_chunkID.ToString(), nextUnload.State.ToString()));
                    return;
                }

                m_unloadQueue.Dequeue();
                m_coroutineStarter.StartCoroutine(nextUnload.Unload(OnFinishedChunkWork));
                return;
            }

            var nextLoad = m_loadQueue.Peek();

            //  If it is in between loaded or unloaded state, then do nothing. let the OnFinishedChunkWork callback handle calling this
            // again when it is done.
            if (nextLoad.State == LoadState.Unloading)
            {
                Debug.LogError(string.Format("Chunk {0} is in state {1}", m_chunkID.ToString(), nextLoad.State.ToString()));
                return;
            }

            m_loadQueue.Dequeue();
            
            switch (nextLoad.State)
            {
                case LoadState.Unloaded:
                    //Debug.LogError(string.Format("Loading chunk {0}", m_chunkID.ToString()));
                    m_coroutineStarter.StartCoroutine(nextLoad.Load(OnFinishedChunkWork));
                    break;
            }*/
        }

        public void LoadChunk(int lodLevel)
        {
            //  If there aren't any chunk LODs, then just early return
            if (m_chunkLODs == null)
                return;

            var lod = m_chunkLODs[lodLevel];

            m_jobQueue.Clear();
            m_currentlyLoadedLODLevel = lodLevel;

            //  If the LOD has already begun loading or has finished loading then do nothing
            if (lod.State == LoadState.Loaded || lod.State == LoadState.Loading)
            {
                Debug.LogError("Is already loaded or in loading!");
            }
            else
            {
                if (!m_jobQueue.Contains(lod))
                {
                    m_jobQueue.Enqueue(lod);
                }
            }

            UnloadAllLODS(lod.LodLevel);
            CheckLODJobQueue();
        }

        /// <summary>
        /// Unloads the currently loaded LOD level for this chunk.
        /// If there isn't one loaded, nothing happens.
        /// </summary>
        public void UnloadChunk()
        {
            m_currentlyLoadedLODLevel = -1;
            UnloadAllLODS();
            CheckLODJobQueue();
        }
    }
}

