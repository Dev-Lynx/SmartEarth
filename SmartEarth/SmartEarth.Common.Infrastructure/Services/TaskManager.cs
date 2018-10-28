using Prism.Logging;
using Prism.Mvvm;
using SmartEarth.Common.Infrastructure.Models;
using SmartEarth.Common.Infrastructure.Models.Interfaces;
using SmartEarth.Common.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmartEarth.Common.Infrastructure.Services
{
    public class TaskManager : BindableBase, ITaskManager
    {
        #region Properties
        public bool IsBusy { get; private set; }

        SmartEarthTask _currentTask;
        public SmartEarthTask CurrentTask { get => _currentTask; set => SetProperty(ref _currentTask, value); }
        ILoggerFacade Logger { get; }
        IDatabaseManager DatabaseManager { get; }
        IConfigurationManager ConfigurationManager { get; }
        Queue<ITask> Tasks { get; } = new Queue<ITask>();
        public event EventHandler NewTaskRegistered;

        public event EventHandler TaskStarted;
        public event EventHandler TaskCompleted;
        #endregion

        #region Constructors
        public TaskManager(ILoggerFacade logger, IDatabaseManager databaseManager, IConfigurationManager configurationManager)
        {
            Logger = logger;
            DatabaseManager = databaseManager;
            ConfigurationManager = configurationManager;

            NewTaskRegistered += (s, e) =>
            {
                if (IsBusy) return;
                AccessQueue();
            };

            TaskCompleted += (s, e) => AccessQueue();
        }
        #endregion

        #region Methods

        #region ITaskManager Method Implementation
        public void RegisterTask(ITask task)
        {
            Tasks.Enqueue(task);
            NewTaskRegistered?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        async void AccessQueue()
        {
            if (Tasks.Count <= 0) return;
            
            var task = Tasks.Peek();
            var configuration = new Configuration(ConfigurationManager.Configuration);
            

            if (task is SmartEarthTask)
                CurrentTask = (SmartEarthTask)task;

            TaskStarted?.Invoke(this, EventArgs.Empty);
            await task.Start(configuration);


            if (task.Status == TaskStatus.Completed && task is SmartEarthTask)
                DatabaseManager.SaveCompletedTask((SmartEarthTask)task);


            // TO-DO: Handle Task Failures here...

            Tasks.Dequeue();
            TaskCompleted?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
