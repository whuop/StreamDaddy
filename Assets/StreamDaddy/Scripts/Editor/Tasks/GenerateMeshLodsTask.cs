﻿using StreamDaddy.Editor.Chunking;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Landfall.Editor;

namespace StreamDaddy.Editor.Tasks
{
    public class GenerateMeshLodsTask : Task
    {
        private static Dictionary<int, int> LOD_LEVELS = new Dictionary<int, int>
        {
            { 0, 100 }, // 100%
            { 1, 75 }, // 75%
            { 2, 50 }, // 50%
            { 3, 25 } // 25%
        };

        public enum LodFormat : int
        {
            FBX_2013_Binary     = 0,
            FBX_2013_ASCII      = 1,
            DAE                 = 2,
            FBX_6_0_Binary      = 3,
            FBX_6_0_ASCII       = 4,
            OBJ                 = 5,
            DXF                 = 6
        }

        private static Dictionary<LodFormat, string> LOD_FORMAT = new Dictionary<LodFormat, string>
        {
            { LodFormat.FBX_2013_Binary, ".fbx" },  // FBX_2013_binary
            { LodFormat.FBX_2013_ASCII, ".fbx" },   // FBX_2013_ASCII
            { LodFormat.DAE, ".dae" },              // DAE
            { LodFormat.FBX_6_0_Binary, ".fbx" },   // FBX_6.0_Binary
            { LodFormat.FBX_6_0_ASCII, ".fbx" },    // FBX_6.0_ASCII
            { LodFormat.OBJ, ".obj" },              // OBJ
            { LodFormat.DXF, ".dxf" }               //  DXF
        };

        public static string GetLodFormat(LodFormat format)
        {
            return LOD_FORMAT[format];
        }

        public GenerateMeshLodsTask() : base("Generate Mesh LODs")
        {

        }

        public bool Execute(string worldName, LodFormat lodFormat, List<EditorChunk> chunks)
        {
            HashSet<int> processedIds = new HashSet<int>();

            try
            {
                foreach (var chunk in chunks)
                {
                    foreach (var filter in chunk.MeshFilters)
                    {
                        if (processedIds.Contains(filter.sharedMesh.GetInstanceID()))
                            continue;

                        string baseAssetPath = AssetDatabase.GetAssetPath(filter.sharedMesh);
                        int instanceID = AssetDatabase.LoadAssetAtPath(baseAssetPath, typeof(Mesh)).GetInstanceID();

                        string outputPath = "GeneratedLODS/" + baseAssetPath.Replace("Assets/", "");
                        
                        //  remove the file extension from the output.
                        outputPath = RemoveFileExtension(outputPath);

                        //  Create the directory tree where all the lods and individual mesh assets will be saved.
                        string outputDirectory = "Assets/" + outputPath + "/";
                        PathUtils.EnsurePathExists(outputDirectory);

                        //  For some reason mle.exe is better at creating less verts when creating a 100% lod first and 
                        //  using that to create the other lods
                        var lod0 = ExtractMeshesFromFilter(filter);//GenerateLOD(filter.sharedMesh, lodFormat, 0, outputPath);
                        var lod1 = GenerateLOD(filter.sharedMesh, lodFormat, 1, outputPath);
                        var lod2 = GenerateLOD(filter.sharedMesh, lodFormat, 2, outputPath);
                        var lod3 = GenerateLOD(filter.sharedMesh, lodFormat, 3, outputPath);
                        
                        CreateAssetsFromMeshes(lod0, outputDirectory, "_LOD0");
                        CreateAssetsFromMeshes(lod1, outputDirectory, "_LOD1");
                        CreateAssetsFromMeshes(lod2, outputDirectory, "_LOD2");
                        CreateAssetsFromMeshes(lod3, outputDirectory, "_LOD3");

                        //CreateAssetFromMesh(lod0, outputPath + "")

                        /*AssetDatabase.StartAssetEditing();
                        //  Remove the LOD 0, it's just temporary trash
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(mesh.GetInstanceID()));
                        AssetDatabase.StopAssetEditing();
                        AssetDatabase.Refresh();
                        */
                        processedIds.Add(instanceID);
                    }
                }
            }
            catch(Exception e)
            {
                LogError(e.Message);
                return false;
            }

            return true;
        }

        private Mesh[] ExtractMeshesFromFilter(MeshFilter filter)
        {
            string path = AssetDatabase.GetAssetPath(filter.sharedMesh);
            var modelObjects = AssetDatabase.LoadAllAssetsAtPath(path);
            List<Mesh> allMeshes = new List<Mesh>();
            foreach (var obj in modelObjects)
            {
                if (obj.GetType() == typeof(Mesh))
                {
                    Mesh subMesh = (Mesh)obj;
                    allMeshes.Add(subMesh);
                }
            }
            return allMeshes.ToArray();
        }

        public Mesh[] GenerateLOD(Mesh mesh, LodFormat lodFormat, int lodLevel, string outputPath)
        {
            string path = AssetDatabase.GetAssetPath(mesh);
            string meshName = mesh.name;
            string lodFormatName = GetLodFormat(lodFormat);
            string inputMeshFileFormat = "." + PathUtils.ExtractFileFormatFromPath(path);

            LogInfo(string.Format("Mesh Path is {0}", path));
            LogInfo(string.Format("Mesh Name is {0}", meshName));
            LogInfo(string.Format("Mesh format is {0}", inputMeshFileFormat));

            string inputPath = path.Replace("Assets/", "");
            inputPath = RemoveFileExtension(inputPath);
            
            // Add the LOD level to the output path, so that it doesn't override any of the other LOD levels for this LOD.
            outputPath += "_LOD" + lodLevel;
            
            string globalInputPath = Application.dataPath + "/" + inputPath + inputMeshFileFormat;
            string globalOutputPath = Application.dataPath + "/" + outputPath + lodFormatName;

            string workingDir = Directory.GetCurrentDirectory();
            LogInfo(string.Format("WorkingDir: {0}", workingDir));

            string mlePath = Application.dataPath + "/StreamDaddy/Editor/mle.exe";

            LogInfo(string.Format("Input Path is {0}", inputPath));
            LogInfo(string.Format("Global Input Path is {0}", globalInputPath));
            LogInfo(string.Format("Output Path is {0}", outputPath));
            LogInfo(string.Format("Global Output Path is {0}", globalOutputPath));

            LogInfo(string.Format("MLE Path: {0}", mlePath));

            string args = "-b y -d n -s n -t " + LOD_LEVELS[lodLevel] + "% -i \"" + globalInputPath + "\" -o \"" + globalOutputPath + "\"" + " -f " + (int)lodFormat;

            args = args.Replace("/", @"\");
            LogInfo(string.Format("mle arguments: {0}", args));

            AssetDatabase.StartAssetEditing();

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            startInfo.FileName = mlePath;
            startInfo.Arguments = args;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;

            process.StartInfo = startInfo;
            process.Start();

            process.StandardInput.Flush();
            process.StandardInput.Close();
            process.WaitForExit();

            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            if (!string.IsNullOrEmpty(output))
                LogInfo(output);
            if (!string.IsNullOrEmpty(error))
                LogError(error);

            //  Return the mesh that was generated
            string localOutputPath = "Assets/" + globalOutputPath.Split(new[] { "Assets/" }, StringSplitOptions.None)[1];
            LogInfo(string.Format("Local Output Path {0}", localOutputPath));

            var modelObjects = AssetDatabase.LoadAllAssetsAtPath(localOutputPath);

            List<Mesh> allMeshes = new List<Mesh>();
            foreach(var obj in modelObjects)
            {
                if (obj.GetType() == typeof(Mesh))
                {
                    Mesh subMesh = (Mesh)obj;
                    allMeshes.Add(subMesh);
                }
            }
            return allMeshes.ToArray();
        }

        private void CreateAssetsFromMeshes(Mesh[] meshes, string outputPath, string postFix)
        {
            for(int i = 0; i < meshes.Length; i++)
            {
                Mesh mesh = meshes[i];
                string finalOutputPath = outputPath + mesh.name + postFix + ".asset";

                SaveMeshAsAsset(mesh, finalOutputPath);
            }
        }

        private Mesh SaveMeshAsAsset(Mesh mesh, string outputPath)
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
            
            AssetDatabase.CreateAsset(newMesh, outputPath);
            return newMesh;
        }
        
        public static string RemoveFileExtension(string path)
        {
            string outPath = path;
            foreach(var format in LOD_FORMAT.Values)
            {
                outPath = outPath.Replace(format.ToLower(), "");
                outPath = outPath.Replace(format.ToUpper(), "");
            }
            return outPath;
        }
    }

}

