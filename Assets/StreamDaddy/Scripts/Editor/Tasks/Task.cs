using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Editor.Tasks
{
    public abstract class Task
    {
        private string m_name;
        public string Name { get { return m_name; } set { m_name = value; } }

        private float m_progress = 0.0f;
        public float Progress { get { return m_progress; } set { m_progress = value; } }

        public Task(string name)
        {
            m_name = name;
        }

        protected void LogInfo(string msg)
        {
            Debug.Log(string.Format("[Task-{0}] " + msg, Name));
        }

        protected void LogWarning(string msg)
        {
            Debug.LogWarning(string.Format("[Task-{0}] " + msg, Name));
        }

        protected void LogError(string msg)
        {
            Debug.LogError(string.Format("[Task-{0}] " + msg, Name));
        }
    }
}