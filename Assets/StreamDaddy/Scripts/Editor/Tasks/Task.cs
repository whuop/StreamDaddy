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

        /// <summary>
        /// An executeable task.
        /// Takes arguments as a dictionary which is the data that should be used in execution.
        /// Returns true if successfull, returns false otherwise.
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public abstract bool Execute(Dictionary<string, object> arguments);
    }
}