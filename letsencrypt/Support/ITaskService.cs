using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.TaskScheduler;

namespace letsencrypt.Support
{
    public interface ITaskService : IDisposable
    {
        void DeleteTask(string taskName);
        ITask NewTask();
        void RegisterTaskDefinition(string taskName, ITask task);
        void RegisterTaskDefinition(string taskName, ITask task, string username, string password);
    }

    public interface ITask
    {
        void SetDescription(string description);
        void AddAction(string executable, string arguments, string startLocation);
        void AddTrigger(short daysInterval, DateTime startBoundary);
    }
}
