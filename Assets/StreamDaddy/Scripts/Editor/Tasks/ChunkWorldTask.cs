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

        public bool Execute(EditorChunkManager chunkManager, Vector3Int chunkSize)
        {
            chunkManager.SetChunkSizeAndClearManager(chunkSize);

            var allMeshes = GameObject.FindObjectsOfType<MeshFilter>();
            var allBoxColliders = GameObject.FindObjectsOfType<BoxCollider>();
            var allSphereColliders = GameObject.FindObjectsOfType<SphereCollider>();
            var allMeshColliders = GameObject.FindObjectsOfType<MeshCollider>();

            foreach(var mesh in allMeshes)
            {
                chunkManager.AddGameObject(mesh.gameObject);
            }

            foreach(var box in allBoxColliders)
            {
                chunkManager.AddGameObject(box.gameObject);
            }

            foreach(var sphere in allSphereColliders)
            {
                chunkManager.AddGameObject(sphere.gameObject);
            }

            foreach(var meshCol in allMeshColliders)
            {
                chunkManager.AddGameObject(meshCol.gameObject);
            }

            return true;
        }
    }

}

