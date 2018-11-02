using StreamDaddy.Editor.Chunking;
using System.Collections;
using System.Collections.Generic;
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

            int i = 0;
            foreach(var mesh in allMeshes)
            {
                chunkManager.AddGameObject(mesh.gameObject);
                i++;
            }

            foreach(var box in allBoxColliders)
            {
                chunkManager.AddGameObject(box.gameObject);
                i++;
            }

            foreach(var sphere in allSphereColliders)
            {
                chunkManager.AddGameObject(sphere.gameObject);
                i++;
            }

            foreach(var meshCol in allMeshColliders)
            {
                chunkManager.AddGameObject(meshCol.gameObject);
                i++;
            }

            LogInfo(string.Format("Added {0} GameObjects to Chunks", i));

            return true;
        }
    }

}

