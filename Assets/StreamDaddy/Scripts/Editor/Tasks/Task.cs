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

        /// <summary>
        /// Checks to make sure that the 
        /// </summary>
        /// <param name="argument"></param>
        protected bool EnsureArgumentExists(string argument, Dictionary<string, object> arguments)
        {
            if (!arguments.ContainsKey(argument))
            {
                Debug.LogError(string.Format("[Task] Could not find argument {0}, Task failed!", argument));
                return false;
            }
            return true;
        }
    }
}