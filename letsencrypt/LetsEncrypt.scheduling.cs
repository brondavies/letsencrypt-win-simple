using letsencrypt.Support;
using Microsoft.Win32.TaskScheduler;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace letsencrypt
{
    public partial class LetsEncrypt
    {
        public static void ScheduleRenewal(Target target, Options options, ITaskService taskService)
        {
            Settings settings = new Settings(options.ConfigPath);

            EnsureTaskScheduler(settings, options, taskService);

            Log.Information(R.Addingrenewalfortarget, target);
            var renewals = settings.Renewals;

            if (renewals != null)
            {
                foreach (var existing in from r in renewals.ToArray() where r.Binding.Host == target.Host select r)
                {
                    Log.Information(R.Removingexistingscheduledrenewal, existing);
                    renewals.Remove(existing);
                }
            }

            var result = new ScheduledRenewal
            {
                Binding = target,
                CentralSsl = options.CentralSslStore,
                San = options.San.ToString(),
                Date = DateTime.UtcNow.AddDays(options.RenewalPeriod),
                KeepExisting = options.KeepExisting.ToString(),
                Script = options.Script,
                ScriptParameters = options.ScriptParameters,
                Warmup = options.Warmup
            };
            renewals.Add(result);
            settings.Save();

            Log.Information(R.Renewalscheduledresult, result);
        }

        public static string CleanFileName(string fileName)
        {
            return
                Path.GetInvalidFileNameChars()
                    .Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }

        internal static void EnsureTaskScheduler(Settings settings, Options options, ITaskService taskService)
        {
            var taskName = CLIENT_NAME + " " + CleanFileName(options.BaseUri);

            if (settings.ScheduledTaskName == taskName)
            {
                if (!PromptYesNo(options, string.Format("\n" + R.DoyouwanttoreplacetheexistingtaskName, taskName), false))
                {
                    return;
                }
                Log.Information(R.DeletingexistingtaskNamefromWindowsTaskScheduler, taskName);
                taskService.DeleteTask(taskName);
            }
                
            Log.Information(R.CreatingtaskNamewithWindowsTaskScheduler, taskName);

            // Create a new task definition and assign properties
            var task = taskService.NewTask();
            task.SetDescription(R.CheckforrenewalofACMEcertificates);

            var now = DateTime.Now;
            var runtime = new DateTime(now.Year, now.Month, now.Day, 9, 0, 0);
            task.AddTrigger(daysInterval: 1, startBoundary: runtime);

            var currentExe = Assembly.GetExecutingAssembly().Location;

            // Create an action that will launch the app with the renew parameters whenever the trigger fires
            string commandLine = $"--renew --baseuri \"{options.BaseUri}\"";
            if (!string.IsNullOrWhiteSpace(options.CertOutPath))
            {
                commandLine += $" --certoutpath \"{options.CertOutPath}\"";
            }
            task.AddAction(executable: currentExe, arguments: commandLine, startLocation: Path.GetDirectoryName(currentExe));

            if (!options.Silent && !options.UseDefaultTaskUser && PromptYesNo(options, "\n" + R.Doyouwanttospecifytheuserthetaskwillrunas, false))
            {
                // Ask for the login and password to allow the task to run
                var username = PromptForText(options, R.Entertheusername);
                var password = PromptForText(options, R.Entertheuserspassword);
                taskService.RegisterTaskDefinition(taskName, task, username, password.ToString());
            }
            else
            {
                Log.Information(R.Creatingtasktorunascurrentuseronlywhentheuserisloggedon);
                taskService.RegisterTaskDefinition(taskName, task);
            }
            settings.ScheduledTaskName = taskName;
        }
    }
}
