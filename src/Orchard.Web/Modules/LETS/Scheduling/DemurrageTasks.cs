using System;
using System.Linq;
using LETS.Services;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;

namespace LETS.Scheduling
{
    public class DemurrageTasks : IScheduledTaskHandler

    {
        private readonly ITransactionService _transactionService;
        private readonly IScheduledTaskManager _taskManager;

        private const string TaskType = "Demurrage";
        
        public ILogger Logger { get; set; }
        

        public DemurrageTasks(ITransactionService transactionService, IScheduledTaskManager taskManager)
        {
            _transactionService = transactionService;
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

        public void Process(ScheduledTaskContext context)
        {
            if (context.Task.TaskType == TaskType)
            {
                try
                {
                    Logger.Error("processing the demurrage task {0}", context.Task.ScheduledUtc);
                    _transactionService.ProcessDemurrage();
                }
                catch (Exception e)
                {
                    Logger.Error(e, e.Message);
                }
                finally
                {
                    Logger.Error("finally, attempt to schedule new task");
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
                    Logger.Error("created new task from {0}", from);
                }
                else
                {
                    Logger.Error("Attempted to schedule a task but tasks already scheduled: {0}", tasks.Count);                    
                }
            }
            else
            {
                Logger.Error("attempt to schedule a task for a time already passed");
            }
        }
    }
}

