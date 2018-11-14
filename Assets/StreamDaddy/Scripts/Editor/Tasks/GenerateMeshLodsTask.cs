﻿using StreamDaddy.Editor.Chunking;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

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

        public GenerateMeshLodsTask() : base("Generate Mesh LODs")
        {

        }

        public bool Execute(string worldName, List<EditorChunk> chunks)
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
                        string outputPath = "GeneratedLODS/" + baseAssetPath.Replace("Assets/", "");

                        //  remove the file extension from the output.
                        outputPath = RemoveFileExtension(outputPath);
                        
                        //  For some reason mle.exe is better at creating less verts when creating a 100% lod first and 
                        //  using that to create the other lods
                        var mesh = GenerateLOD(filter.sharedMesh, 0, outputPath);
                        GenerateLOD(mesh, 1, outputPath);
                        GenerateLOD(mesh, 2, outputPath);
                        GenerateLOD(mesh, 3, outputPath);

                        AssetDatabase.StartAssetEditing();
                        //  Remove the LOD 0, it's just temporary trash
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(mesh.GetInstanceID()));
                        AssetDatabase.StopAssetEditing();
                        AssetDatabase.Refresh();
                        
                        processedIds.Add(filter.sharedMesh.GetInstanceID());
                    }

                    /*foreach (var collider in chunk.Colliders)
                    {
                        MeshCollider col = collider as MeshCollider;
                        if (col != null)
                        {
                            if (processedIds.Contains(col.sharedMesh.GetInstanceID()))
                                continue;

                            string baseAssetPath = AssetDatabase.GetAssetPath(col.sharedMesh.GetInstanceID());
                            string outputPath = "GeneratedLODS/" + baseAssetPath.Replace("Assets/", "");

                            //  remove the file extension from the output.
                            outputPath = RemoveFileExtension(outputPath);

                            //  For some reason mle.exe is better at creating less verts when creating a 100% lod first and 
                            //  using that to create the other lods
                            var mesh = GenerateLOD(col.sharedMesh, 0, outputPath);
                            //GenerateLOD(mesh, 0, outputPath);
                            //GenerateLOD(mesh, 0, outputPath);
                            //GenerateLOD(mesh, 0, outputPath);



                            processedIds.Add(col.sharedMesh.GetInstanceID());
                        }
                    }*/
                }
            }
            catch(Exception e)
            {
                LogError(e.Message);
                return false;
            }

            return true;
        }

        public Mesh GenerateLOD(Mesh mesh, int lodLevel, string outputPath)
        {
            string path = AssetDatabase.GetAssetPath(mesh);
            string meshName = mesh.name;

            LogInfo(string.Format("Mesh Path is {0}", path));
            LogInfo(string.Format("Mesh Name is {0}", meshName));

            string inputPath = path.Replace("Assets/", "");
            inputPath = RemoveFileExtension(inputPath);

            //  Calculate the output directory for the LOD
            //  Create the output path
            string outDirectory = Application.dataPath + "/" + outputPath.Replace(meshName, "");
            System.IO.Directory.CreateDirectory(outDirectory);

            // Add the LOD level to the output path, so that it doesn't override any of the other LOD levels for this LOD.
            outputPath += "_LOD" + lodLevel;
            
            string globalInputPath = Application.dataPath + "/" + inputPath + ".fbx";
            string globalOutputPath = Application.dataPath + "/" + outputPath + ".fbx";

            string workingDir = Directory.GetCurrentDirectory();
            LogInfo(string.Format("WorkingDir: {0}", workingDir));

            string mlePath = Application.dataPath + "/StreamDaddy/Editor/mle.exe";

            LogInfo(string.Format("Input Path is {0}", inputPath));
            LogInfo(string.Format("Global Input Path is {0}", globalInputPath));
            LogInfo(string.Format("Output Path is {0}", outputPath));
            LogInfo(string.Format("Global Output Path is {0}", globalOutputPath));

            LogInfo(string.Format("MLE Path: {0}", mlePath));

            string args = "-b n -d n -s n -t " + LOD_LEVELS[lodLevel] + "% -i \"" + globalInputPath + "\" -o \"" + globalOutputPath + "\"";

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
            Mesh outputMesh = AssetDatabase.LoadAssetAtPath<Mesh>(localOutputPath);

            //AssetDatabase.SetLabels(outputMesh, new string[] { "dont-import-materials", "dont-import-animations" });

            return outputMesh;
        }

        private string RemoveFileExtension(string path)
        {
            string outPath = path;
            outPath = outPath.Replace(".FBX", "");
            outPath = outPath.Replace(".fbx", "");
            outPath = outPath.Replace(".OBJ", "");
            outPath = outPath.Replace(".obj", "");
            return outPath;
        }
    }

}

