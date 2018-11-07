using StreamDaddy.Editor.Chunking;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StreamDaddy.Editor.Tasks
{
    public class ChunkWorldTask : Task
    {
        public ChunkWorldTask() : base("Chunk World")
        {

        }

        public bool Execute(EditorChunkManager chunkManager)
        {
            var allMeshes = GameObject.FindObjectsOfType<MeshFilter>();
            var allBoxColliders = GameObject.FindObjectsOfType<BoxCollider>();
            var allSphereColliders = GameObject.FindObjectsOfType<SphereCollider>();
            var allMeshColliders = GameObject.FindObjectsOfType<MeshCollider>();

            int totalCount = allMeshes.Length + allBoxColliders.Length + allSphereColliders.Length + allMeshColliders.Length;

            int i = 0;
            foreach(var mesh in allMeshes)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Sorting Meshes into Chunks", i.ToString() + "/" + totalCount.ToString(), i / totalCount))
                    return false;
                var renderer = mesh.GetComponent<MeshRenderer>();
                if (renderer == null)
                    continue;
                if (renderer.enabled == false)
                    continue;
                chunkManager.AddGameObject(mesh.gameObject);
                i++;
            }

            foreach(var box in allBoxColliders)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Sorting BoxColliders into Chunks", i.ToString() + "/" + totalCount.ToString(), i / totalCount))
                    return false;
                chunkManager.AddGameObject(box.gameObject);
                i++;
            }

            foreach(var sphere in allSphereColliders)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Sorting SphereColliders into Chunks", i.ToString() + "/" + totalCount.ToString(), i / totalCount))
                    return false;
                chunkManager.AddGameObject(sphere.gameObject);
                i++;
            }

            foreach(var meshCol in allMeshColliders)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Sorting MeshColliders into Chunks", i.ToString() + "/" + totalCount.ToString(), i / totalCount))
                    return false;
                chunkManager.AddGameObject(meshCol.gameObject);
                i++;
            }

            LogInfo(string.Format("Added {0} GameObjects to Chunks", i));

            EditorUtility.ClearProgressBar();

            return true;
        }
    }

}

