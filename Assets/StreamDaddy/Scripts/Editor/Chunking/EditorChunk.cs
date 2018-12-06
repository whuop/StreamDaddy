using StreamDaddy.Chunking;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StreamDaddy.Editor.Chunking
{
    public class EditorChunk
    {
        private ChunkID m_id;
        public ChunkID ChunkID { get { return m_id; } }

        private string m_worldName;
        public string WorldName { get { return m_worldName; } }

        private List<MeshFilter> m_meshFilters = new List<MeshFilter>();
        public List<MeshFilter> MeshFilters { get { return m_meshFilters; } }

        private List<Collider> m_colliders = new List<Collider>();
        public List<Collider> Colliders { get { return m_colliders; } }

        private Terrain m_terrain;
        public Terrain Terrain { get { return m_terrain; } }

        private Mesh m_terrainMeshLOD1;
        public Mesh TerrainLOD1 { get { return m_terrainMeshLOD1; } }
        private Mesh m_terrainMeshLOD2;
        public Mesh TerrainLOD2 { get { return m_terrainMeshLOD2; } }
        private Mesh m_terrainMeshLOD3;
        public Mesh TerrainLOD3 { get { return m_terrainMeshLOD3; } }
        
        private Bounds m_boundingBox;
        
        public EditorChunk(ChunkID id, Vector3Int size, string worldName)
        {
            m_id = id;
            m_boundingBox = new Bounds(m_id.ID, size);
            m_worldName = worldName;
            
            Debug.Log("Created chunk with size: " + size.x + " " + size.y + " " + size.z);
        }

        public void AddMeshFilter(MeshFilter filter)
        {
            m_meshFilters.Add(filter);
        }

        public void AddCollider(Collider collider)
        {
            m_colliders.Add(collider);
        }

        public void SetTerrain(Terrain terrain)
        {
            m_terrain = terrain;
            //  Make sure the different LODs of this terrain are also added.
            FetchAndAssignTerrainLODs(terrain);
        }

        private void FetchAndAssignTerrainLODs(Terrain terrain)
        {
            string terrainName = terrain.name;
            string terrainLODFolder = EditorPaths.GetTerrainMeshPath(m_worldName);

            Debug.LogError("Terrain LOD Folder: " + terrainLODFolder);

            // Fetch the different LODS
            for(int i = 1; i < 4; i++)
            {
                string lodName = terrainName + "_LOD" + i;
                Debug.LogError("LOD Name: " + lodName);
                Debug.LogError("LOD Full Path: " + terrainLODFolder + lodName);

                string lodLocalPath = terrainLODFolder + lodName + ".asset";
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(lodLocalPath);

                if (mesh == null)
                    Debug.LogError("Could not find Mesh for Terrain with path: " + lodLocalPath);

                switch(i)
                {
                    case 1:
                        m_terrainMeshLOD1 = mesh;
                        break;
                    case 2:
                        m_terrainMeshLOD2 = mesh;
                        break;
                    case 3:
                        m_terrainMeshLOD3 = mesh;
                        break;
                }
            }
        }

        public bool ContainsPoint(Vector3 point)
        {
            if (m_boundingBox.Contains(point))
            {
                return true;
            }
            return false;
        }

        public void Draw()
        {
            Color tempColor = Handles.color;
            
            Vector3 center = new Vector3(
                m_boundingBox.center.x * m_boundingBox.size.x + m_boundingBox.size.x * 0.5f,
                m_boundingBox.center.y * m_boundingBox.size.y + m_boundingBox.size.y * 0.5f,
                m_boundingBox.center.z * m_boundingBox.size.z + m_boundingBox.size.z * 0.5f
                );

            Handles.Label(center, m_id.ToString());

            Handles.color = Color.green;
            Handles.DrawWireCube(center, m_boundingBox.size);
            Handles.color = tempColor;
        }
    }
}


