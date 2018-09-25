using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Editor.Configs.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple =false, Inherited =true)]
    public class ConfigPathAttribute : Attribute
    {
        private string m_folderPath;
        public string FolderPath { get { return m_folderPath; } }
        private string m_fileNameAndExtension;
        public string FileNameAndExtension { get { return m_fileNameAndExtension; } }
        private string m_assetPath;
        public string AssetPath { get { return m_assetPath; } }

        public ConfigPathAttribute(string folderPath, string fileNameAndExtension)
        {
            m_folderPath = folderPath;
            m_fileNameAndExtension = fileNameAndExtension;
            m_assetPath = folderPath + fileNameAndExtension;
        }
    }
}


