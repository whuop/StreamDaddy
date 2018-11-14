using StreamDaddy.Editor.Chunking;
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
            { 1, 65 }, // 75%
            { 2, 30 }, // 50%
            { 3, 12 } // 25%
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

                        //  For some reason mle.exe is better at creating less verts when creating a 100% lod first and 
                        //  using that to create the other lods
                        GenerateLOD(filter.sharedMesh, 0);
                        GenerateLOD(filter.sharedMesh, 1);
                        GenerateLOD(filter.sharedMesh, 2);
                        GenerateLOD(filter.sharedMesh, 3);
                        processedIds.Add(filter.sharedMesh.GetInstanceID());
                    }

                    foreach (var collider in chunk.Colliders)
                    {
                        MeshCollider col = collider as MeshCollider;
                        if (col != null)
                        {
                            if (processedIds.Contains(col.sharedMesh.GetInstanceID()))
                                continue;

                            //  For some reason mle.exe is better at creating less verts when creating a 100% lod first and 
                            //  using that to create the other lods
                            GenerateLOD(col.sharedMesh, 0);
                            GenerateLOD(col.sharedMesh, 1);
                            GenerateLOD(col.sharedMesh, 2);
                            GenerateLOD(col.sharedMesh, 3);
                            processedIds.Add(col.sharedMesh.GetInstanceID());
                        }
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

        public Mesh GenerateLOD(Mesh mesh, int lodLevel)
        {
            string path = AssetDatabase.GetAssetPath(mesh);

            string meshName = mesh.name;
            //  If this is the LOD0 that is passed in, then remove that from the name.
            meshName = meshName.Replace("_LOD0", "");
            path = path.Replace("_LOD0", "");

            LogInfo(string.Format("Mesh Path is {0}", path));
            LogInfo(string.Format("Mesh Name is {0}", meshName));

            string inputPath = path.Replace("Assets/", "");
            inputPath = RemoveFileExtension(inputPath);
            //  Create the output path. Has to have parent folders in it to account for meshes with the same name
            string outputPath = "GeneratedLODS/" + path.Replace("Assets/", "");
            outputPath = RemoveFileExtension(outputPath);
            outputPath += "_LOD" + lodLevel;

            //  Create the output path
            string outDirectory = Application.dataPath + "/GeneratedLODS/" + inputPath.Replace(meshName, "");
            outDirectory = RemoveFileExtension(outDirectory);
            System.IO.Directory.CreateDirectory(outDirectory);

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

