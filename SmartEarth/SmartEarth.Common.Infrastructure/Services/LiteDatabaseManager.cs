using LiteDB;
using Prism.Logging;
using Prism.Mvvm;
using SmartEarth.Common.Infrastructure.Models;
using SmartEarth.Common.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEarth.Common.Infrastructure.Services
{
    public class LiteDatabaseManager : BindableBase, IDatabaseManager
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        #endregion

        #region Bindables
        public ObservableCollection<SmartEarthTask> ScheduledTasks { get; } = new ObservableCollection<SmartEarthTask>();
        public ObservableCollection<SmartEarthTask> CompletedTasks { get; } = new ObservableCollection<SmartEarthTask>();
        public ObservableCollection<ColorBox> Colors { get; } = new ObservableCollection<ColorBox>();
        #endregion

        #region Internals 
        LiteDatabase Database { get; } = new LiteDatabase(Core.LITE_DATABASE_PATH);
        #endregion

        public bool Initialized { get; set; }
        #endregion

        #region Constructors
        public LiteDatabaseManager(ILoggerFacade logger)
        {
            Logger = logger;
            Initialize();
        }
        #endregion

        #region Methods

        void Initialize()
        {
            if (Initialized) return;
            UpdateCompletedTasks();
            UpdateScheduledTasks();
            Initialized = true;
        }

        #region IDatabaseManager Implementation
        public bool RemoveScheduledTask(SmartEarthTask task)
        {
            var collection = Database.GetCollection<SmartEarthTask>(Core.SCHEDULED_TASK_DATABASE_DOCUMENT);
            if (collection == null) return false;

            var count = collection.Delete(t => t.Signature == task.Signature) > 0;
            UpdateScheduledTasks();
            return count;
        }

        public bool SaveScheduledTask(SmartEarthTask task)
        {
            var collection = Database.GetCollection<SmartEarthTask>(Core.SCHEDULED_TASK_DATABASE_DOCUMENT);
            if (collection == null) return false;
            
            var result = collection.Upsert(task);
            UpdateScheduledTasks();
            return result;
        }

        public bool SaveCompletedTask(SmartEarthTask task)
        {
            var collection = Database.GetCollection<SmartEarthTask>(Core.COMPLETED_TASK_DATABASE_DOCUMENT);
            if (collection == null) return false;

            var result = collection.Upsert(task);
            UpdateCompletedTasks();
            return result;
        }
        #endregion

        #region Update Engines
        void UpdateScheduledTasks()
        {
            var collection = Database.GetCollection<SmartEarthTask>(Core.SCHEDULED_TASK_DATABASE_DOCUMENT);
            var tasks = collection.FindAll();
            if (tasks == null) return;
            ScheduledTasks.Clear();
            ScheduledTasks.AddRange(tasks);
            RaisePropertyChanged("ScheduledTasks");
        }

        void UpdateCompletedTasks()
        {
            var collection = Database.GetCollection<SmartEarthTask>(Core.COMPLETED_TASK_DATABASE_DOCUMENT);
            var tasks = collection.FindAll();
            if (tasks == null) return;
            CompletedTasks.Clear();
            CompletedTasks.AddRange(tasks);
            RaisePropertyChanged("CompletedTasks");
        }
        #endregion

        #endregion
    }
}
