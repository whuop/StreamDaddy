using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FbxToAsset : EditorWindow
{
    private Mesh mesh;

    [MenuItem("Tools/FbxToAsset")]
    static void Init()
    {
        FbxToAsset window = (FbxToAsset)EditorWindow.GetWindow(typeof(FbxToAsset));
        window.Show();
    }

    private void OnGUI()
    {
        mesh = (Mesh)EditorGUILayout.ObjectField(mesh, typeof(Mesh), false);
        if (GUILayout.Button("Create Asset"))
        {
            MeshFromFBX(mesh, "Assets/testmesh.asset");
        }
    }

    private void MeshFromFBX(Mesh mesh, string outputPath)
    {
        Mesh newMesh = new Mesh();
        newMesh.vertices = mesh.vertices;
        newMesh.uv = mesh.uv;
        newMesh.uv2 = mesh.uv2;
        newMesh.uv3 = mesh.uv3;
        newMesh.uv4 = mesh.uv4;
        newMesh.normals = mesh.normals;
        newMesh.colors = mesh.colors;
        newMesh.tangents = mesh.tangents;
        newMesh.subMeshCount = mesh.subMeshCount;
        for (int subMesh = 0; subMesh < mesh.subMeshCount; ++subMesh)
        {
            newMesh.SetTriangles(mesh.GetTriangles(subMesh), subMesh);
        }
        
        //string meshPath = m_assetPath + "Models/" + mesh.name + "_" + hashCode + ".asset";

        // Create folder if not exist
        //if (!AssetDatabase.IsValidFolder(m_assetPath + "Models"))
        //    AssetDatabase.CreateFolder(m_assetPath.Substring(0, m_assetPath.Length - 1), "Models");

        AssetDatabase.CreateAsset(newMesh, outputPath);
    }
}
