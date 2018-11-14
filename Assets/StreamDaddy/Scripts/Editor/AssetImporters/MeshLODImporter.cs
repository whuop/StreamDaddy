using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StreamDaddy.Editor.AssetImporters
{
    public class MeshLODImporter : AssetPostprocessor
    {
        void OnPreprocessModel()
        {
            ModelImporter modelImporter = assetImporter as ModelImporter;

            if (modelImporter != null)
            {
                if (modelImporter.assetPath.Contains("GeneratedLODS"))
                {
                    modelImporter.importAnimation = false;
                    modelImporter.importMaterials = false;
                }
            }
        }
    }
}

