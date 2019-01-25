using System;
using System.Linq;
using LETS.Services;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;

namespace LETS.Scheduling
{
    public class StatsTasks : IScheduledTaskHandler
    {
        private readonly IStatsService _statsService;
        private readonly IScheduledTaskManager _taskManager;

        public StatsTasks(IStatsService statsService, IScheduledTaskManager taskManager) {
            _statsService = statsService;
            _taskManager = taskManager;
            Logger = NullLogger.Instance;
            try
            {
                var tasks = _taskManager.GetTasks(TaskType);
                if (!tasks.Any())
                {
                    ScheduleNextTask(DateTime.UtcNow.AddDays(1), "constructor");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
            }
        }

        private const string TaskType = "Stats";

        public ILogger Logger { get; set; }

        public void Process(ScheduledTaskContext context)
        {
            if (context.Task.TaskType == TaskType)
            {
                try
                {
                    Logger.Error("processing the stats task {0}", context.Task.ScheduledUtc);
                    _statsService.SaveStats();
                }
                catch (Exception e)
                {
                    Logger.Error(e, e.Message);
                }
                finally
                {
                    Logger.Error("finally, attempt to schedule new stats task");
                    ScheduleNextTask(DateTime.UtcNow.AddDays(1), "process");
                }
            }
        }

        private void ScheduleNextTask(DateTime date, string from)
        {
            if (date >= DateTime.UtcNow)
            {
                var tasks = _taskManager.GetTasks(TaskType).Where(t => t.ScheduledUtc >= DateTime.UtcNow).ToList();
                if (tasks.Count == 0)
                {
                    _taskManager.CreateTask(TaskType, date, null);
                    Logger.Error("created new stats task from {0}", from);
                }
                else
                {
                    Logger.Error("Attempted to schedule a stats task but tasks already scheduled: {0}", tasks.Count);
                }
            }
            else
            {
                Logger.Error("attempt to schedule a stats task for a time already passed");
            }
        }
    }
}