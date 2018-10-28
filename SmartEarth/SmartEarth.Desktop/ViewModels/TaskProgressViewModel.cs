using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Logging;
using Prism.Mvvm;
using SmartEarth.Common.Infrastructure.Models;
using SmartEarth.Common.Infrastructure.Resources.Interfaces;
using SmartEarth.Common.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SmartEarth.Desktop.ViewModels
{
    public class TaskProgressViewModel : BindableBase, IInteractionRequestAware
    {
        #region Properties

        #region Statics
        const double WindowWidth = 400;
        const double WindowHeight = 200;
        #endregion

        #region IInteractionRequestAware Implementation
        INotification _notification;
        public INotification Notification { get => _notification; set => SetProperty(ref _notification, value); }
        public Action FinishInteraction { get; set; }
        #endregion

        #region Services
        ILoggerFacade Logger { get; }
        IScheduler Scheduler { get; }
        ITaskManager TaskManager { get; }
        #endregion

        #region Bindables
        public double Left => Window.Left;
        public double Top => Window.Top;
        public double Width { get => Window.Width; set => Window.Width = value; }
        public double Height { get => Window.Height; set => Window.Height = value; }
        public Window Window { get; set; }
        public SmartEarthTask Task => TaskManager.CurrentTask;
        #endregion

        #region Commands
        public ICommand RescheduleCommand { get; }
        #endregion

        #endregion

        #region Constructors
        public TaskProgressViewModel(ILoggerFacade logger, IScheduler scheduler, ITaskManager taskManager)
        {
            Logger = logger;
            Scheduler = scheduler;
            TaskManager = taskManager;

            RescheduleCommand = new DelegateCommand(OnReschedule);
        }
        #endregion

        #region Methods

        #region Command Handlers
        void OnReschedule()
        {

        }
        #endregion

        public void LoadWindow(Window window)
        {
            RoutedEventHandler loaded = null;
            if (!window.IsLoaded)
            {
                window.Loaded += loaded = (s, e) =>
                {
                    window.Loaded -= loaded;
                    LoadWindow(window);
                };
                return;
            }

            var workArea = System.Windows.SystemParameters.WorkArea;
            Window = window;
            Window.Width = WindowWidth;
            Window.Height = WindowHeight;
            Window.Left = workArea.Right - WindowWidth - 10;
            Window.Top = workArea.Bottom - WindowHeight - 10;
            Window.ResizeMode = ResizeMode.NoResize;
            Window.Owner = null;
            

            Window.StateChanged += (s, e) =>
            {
                Window.WindowState = WindowState.Normal;
                Window.Visibility = Visibility.Visible;
            };
        }

        #endregion
    }
}
