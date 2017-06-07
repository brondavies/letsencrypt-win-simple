using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace letsencrypt.Support
{
    public class TaskServiceWrapper : ITaskService
    {
        private TaskService taskService = new TaskService();
        public void DeleteTask(string taskName)
        {
            taskService.RootFolder.DeleteTask(taskName, false);
        }

        public void Dispose()
        {
            taskService = null;
        }

        public ITask NewTask()
        {
            return new TaskWrapper(taskService);
        }

        public void RegisterTaskDefinition(string taskName, ITask task)
        {
            TaskWrapper taskWrapper = (TaskWrapper)task;
            taskService.RootFolder.RegisterTaskDefinition(taskName, taskWrapper.GetTask());
        }

        public void RegisterTaskDefinition(string taskName, ITask task, string username, string password)
        {
            TaskWrapper taskWrapper = (TaskWrapper)task;
            taskService.RootFolder.RegisterTaskDefinition(taskName, taskWrapper.GetTask(), TaskCreation.Create, username, password, TaskLogonType.Password);
        }
    }
}
