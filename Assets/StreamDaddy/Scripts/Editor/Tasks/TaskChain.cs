using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StreamDaddy.Editor.Tasks
{
    public class TaskChain
    {
        private List<Task> m_tasks = new List<Task>();

        public TaskChain()
        {

        }

        public void AddTask(Task task)
        {
            m_tasks.Add(task);
        }

        public void RemoveTask(Task task)
        {
            m_tasks.Remove(task);
        }

        public void Execute(Dictionary<string, object> arguments)
        {
            for(int i = 0; i < m_tasks.Count; i++)
            {

            }
        }
    }
}


