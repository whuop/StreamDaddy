using StreamDaddy.Editor.Chunking;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StreamDaddy.Editor.Tasks
{
    public class GenerateMeshLodsTask : Task
    {
        private static Dictionary<int, float> LOD_LEVELS = new Dictionary<int, float>
        {
            { 0, 1.0f }, // 100%
            { 1, 0.75f }, // 75%
            { 2, 0.5f }, // 50%
            { 3, 0.25f } // 25%
        };

        public GenerateMeshLodsTask() : base("Generate Mesh LODs")
        {

        }

        public bool Execute(string worldName, List<EditorChunk> chunks)
        {
            
            return true;
        }

        public void GenerateLOD(Mesh mesh, int lodLevel)
        {
            string path = AssetDatabase.GetAssetPath(mesh);

            LogInfo(string.Format("Mesh Path is {0}", path));

            string inputPath = path.Replace(".FBX", "").Replace(".fbx", "").Replace("Assets/", "");

            //  Create the output path. Has to have parent folders in it to account for meshes with the same name
            string outputPath = "GeneratedLODS/" + path.Replace("Assets/", "").Replace(".FBX", "").Replace(".fbx", "") + "_LOD" + lodLevel;

            string globalInputPath = Application.dataPath + "/" + inputPath + ".fbx";
            string globalOutputPath = Application.dataPath + "/" + outputPath + ".fbx";

            string mlePath = Application.dataPath + "StreamDaddy/Editor/mle.exe";

            LogInfo(string.Format("Input Path is {0}", inputPath));
            LogInfo(string.Format("Global Input Path is {0}", globalInputPath));
            LogInfo(string.Format("Output Path is {0}", outputPath));
            LogInfo(string.Format("Global Output Path is {0}", globalOutputPath));

            LogInfo(string.Format("MLE Path: {0}", mlePath));

            string args = "-t " + LOD_LEVELS[lodLevel] + @"% -i " + globalInputPath + " -o " + globalOutputPath;

            args = args.Replace("/", @"\");
            LogInfo(string.Format("mle arguments: {0}", args));

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

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            if (!string.IsNullOrEmpty(output))
                LogInfo(output);
            if (!string.IsNullOrEmpty(error))
                LogError(error);
            /*string combinedOutPath = path.Replace(".fbx", "");
            combinedOutPath = combinedOutPath.Replace(".FBX", "") + "_LOD" + m_LODLayer[i].ToString() + ".fbx";
            string args = "-t " + lodLevel + "% -i \"" + combinedInPath + "\" -o \"" + combinedOutPath + "\"";
            combinedArgs = combinedArgs.Replace("/", @"\");

            //  Create the process that mle.exe is launched from.
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            startInfo.FileName = "mle.exe";

            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;
            process.Start();
            //process.StandardInput.WriteLine(combinedArgs);
            process.StandardInput.Flush();
            process.StandardInput.Close();
            process.WaitForExit();

            string output = process.StandardOutput.ReadToEnd();
            string err = process.StandardError.ReadToEnd();
            UnityEngine.Debug.Log(output);
            UnityEngine.Debug.LogError(err);
            if (err.Contains("unsupported"))
            {
                UnityEngine.Debug.Log(args);
            }*/
        }
    }

}

