using StreamDaddy.Editor.Chunking;
using UnityEditor;
using UnityEngine;

namespace StreamDaddy.Editor.Tasks
{
    public class ChunkWorldTask : Task
    {
        public ChunkWorldTask() : base("Chunk World")
        {

        }

        public bool Execute(EditorChunkManager chunkManager, SplitTerrainTask.Result splitTerrainResult)
        {
            var allMeshes = GameObject.FindObjectsOfType<MeshFilter>();
            var allBoxColliders = GameObject.FindObjectsOfType<BoxCollider>();
            var allSphereColliders = GameObject.FindObjectsOfType<SphereCollider>();
            var allMeshColliders = GameObject.FindObjectsOfType<MeshCollider>();

            int totalCount = allMeshes.Length + allBoxColliders.Length + allSphereColliders.Length + allMeshColliders.Length;

            int i = 0;
            foreach(var filter in allMeshes)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Sorting Meshes into Chunks", i.ToString() + "/" + totalCount.ToString(), i / totalCount))
                    return false;
                var renderer = filter.GetComponent<MeshRenderer>();
                if (renderer == null)
                    continue;
                if (renderer.enabled == false)
                    continue;

                chunkManager.AddMeshFilter(filter, filter.gameObject.transform.position);
                i++;
            }

            foreach(var box in allBoxColliders)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Sorting BoxColliders into Chunks", i.ToString() + "/" + totalCount.ToString(), i / totalCount))
                    return false;

                chunkManager.AddCollider(box, box.gameObject.transform.position);
                i++;
            }

            foreach(var sphere in allSphereColliders)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Sorting SphereColliders into Chunks", i.ToString() + "/" + totalCount.ToString(), i / totalCount))
                    return false;
                chunkManager.AddCollider(sphere, sphere.gameObject.transform.position);
                i++;
            }

            foreach(var meshCol in allMeshColliders)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Sorting MeshColliders into Chunks", i.ToString() + "/" + totalCount.ToString(), i / totalCount))
                    return false;
                chunkManager.AddCollider(meshCol, meshCol.gameObject.transform.position);
                i++;
            }
            
            foreach (var terrain in splitTerrainResult.TerrainSplits)
            {
                chunkManager.SetTerrain(terrain, terrain.transform.position);
            }
            
            LogInfo(string.Format("Added {0} GameObjects to Chunks", i));

            EditorUtility.ClearProgressBar();

            return true;
        }
    }

}

