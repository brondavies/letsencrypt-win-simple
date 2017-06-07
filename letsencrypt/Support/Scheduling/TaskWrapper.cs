using Microsoft.Win32.TaskScheduler;
using System;

namespace letsencrypt.Support
{
    internal class TaskWrapper : ITask
    {
        private TaskDefinition task;

        public TaskWrapper(TaskService service)
        {
            task = service.NewTask();
        }

        public void AddAction(string executable, string arguments, string startLocation)
        {
            task.Actions.Add(new ExecAction(executable, arguments, startLocation));
            task.Principal.RunLevel = TaskRunLevel.Highest; // need admin
        }

        public void AddTrigger(short daysInterval, DateTime startBoundary)
        {
            task.Triggers.Add(new DailyTrigger { DaysInterval = daysInterval, StartBoundary = startBoundary });
        }
        
        public TaskDefinition GetTask()
        {
            return task;
        }

        public void SetDescription(string description)
        {
            task.RegistrationInfo.Description = description;
        }
    }
}