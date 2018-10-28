using LiteDB;
using Prism.Mvvm;
using Quartz;
using SmartEarth.Common.Infrastructure.Models.Interfaces;
using SmartEarth.Common.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace SmartEarth.Common.Infrastructure.Models
{
    [XmlInclude(typeof(GEAutomationTask))]
    public abstract class SmartEarthTask : BindableBase, IJob, ITask, ICloneable
    {
        #region Properties

        #region Statics
        const int MaxDescription = 50;
        #endregion

        #region Bindables
        string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (value == null) value = string.Empty;
                SetProperty(ref _name, value.Trim());
                RaisePropertyChanged(nameof(IsValid));
            }
        }

        string _heading = (string)Application.Current.Resources["NEW_TASK"];
        public string Heading { get => _heading; set => SetProperty(ref _heading, value); }

        ColorBox _color = new ColorBox();
        public ColorBox Color { get => _color; set => SetProperty(ref _color, value); }

        [XmlIgnore]
        [LiteDB.BsonIgnore]
        public string MiniDescription
        {
            get
            {
                try { return Description.Substring(0, MaxDescription) + "...."; }
                catch { return string.Empty; }
            }
        }

        string _description = string.Empty;
        public string Description
        {
            get => _description;
            set
            {
                if (value == null) value = string.Empty;
                SetProperty(ref _description, value.Trim());
                RaisePropertyChanged(nameof(IsValid));
            }
        }

        TaskStatus _status;
        public TaskStatus Status { get => _status; protected set => SetProperty(ref _status, value); }

        double _progress = .0;
        public double Progress { get => _progress; set => SetProperty(ref _progress, value); }

        public ObservableCollection<Log> Logs { get; set; } = new ObservableCollection<Log>();

        string _taskInformation = string.Empty;
        public string TaskInformation { get => _taskInformation; set => SetProperty(ref _taskInformation, value); }

        DateTime _due = DateTime.MinValue;
        public DateTime Due { get => _due; set => SetProperty(ref _due, value); }

        RepeatInterval _interval = new RepeatInterval();
        public RepeatInterval Interval { get => _interval; set => SetProperty(ref _interval, value); }

        #region Flags
        [XmlIgnore]
        [LiteDB.BsonIgnore]
        public bool NameIsValid => Name.IndexOfAny(Path.GetInvalidPathChars()) < 0 && !string.IsNullOrWhiteSpace(Name);
        [XmlIgnore]
        [LiteDB.BsonIgnore]
        public bool IsValid => NameIsValid && !string.IsNullOrWhiteSpace(Description);
        [XmlIgnore]
        [LiteDB.BsonIgnore]
        public bool DescriptionHidden => Description.Length > MaxDescription;
        #endregion

        #endregion

        #region Internals
        [BsonId]
        public Guid Signature { get; set; } = Guid.NewGuid();

        public CancellationTokenSource CancellationSource { get; protected set; } = new CancellationTokenSource();

        [XmlIgnore]
        [LiteDB.BsonIgnore]
        protected virtual Dictionary<string, object> Data => new Dictionary<string, object>()
        {
            { nameof(Signature), Signature },
            { nameof(Name), Name },
            { nameof(Description), Description },
            { nameof(Status), Status },
            { nameof(Due), Due },
            { nameof(Color), Color },
            { nameof(Interval), Interval },
        };

        [XmlIgnore]
        [LiteDB.BsonIgnore]
        public abstract Configuration Configuration { get; protected set; }
        #endregion

        #endregion

        #region Constructors
        public SmartEarthTask() { }
        #endregion

        #region Methods

        #region IJob Implementation
        public virtual Task Execute(IJobExecutionContext context)
        {
            LoadDetails(context.MergedJobDataMap, true);
            return Task.CompletedTask;
        }
        #endregion

        #region ITask Implementaion
        public abstract Task Start(Configuration configuration);
        public abstract Task Pause();
        public abstract Task Resume();
        public abstract Task Stop();
        #endregion

        #region IClonable Implementation
        public virtual object Clone() => Duplicate();
        #endregion

        public virtual IJobDetail CompileDetail(Services.Interfaces.IScheduler scheduler)
        {
            var data = new JobDataMap();

            foreach (var key in Data.Keys) data.Add(key, Data[key]);
            data.Add(nameof(scheduler), scheduler);
            return JobBuilder.Create<GEAutomationTask>().UsingJobData(data).Build();
        }

        protected virtual void LoadDetails(JobDataMap data, bool implement = false)
        {
            Services.Interfaces.IScheduler scheduler = null;
            try
            {
                Signature = (Guid)data[nameof(Signature)];
                Status = (TaskStatus)data[nameof(Status)];
                Due = (DateTime)data[nameof(Due)];
                scheduler = (Services.Interfaces.IScheduler)data[nameof(scheduler)];
            }
            catch { return; }

            if (implement)
                scheduler.ImplementTask(this);
        }

        public abstract SmartEarthTask Duplicate();
        #endregion
    }
}
