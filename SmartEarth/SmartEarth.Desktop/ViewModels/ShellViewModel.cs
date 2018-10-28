using Prism.Commands;
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
using System.Windows.Media.Imaging;

namespace SmartEarth.Desktop.ViewModels
{
    public class ShellViewModel : BindableBase
    {
        #region Properties

        #region Constants
        const double DEFAULT_SHELL_THICKNESS = 5;
        #endregion

        #region Services
        ILoggerFacade Logger { get; }
        ITaskManager TaskManager { get; }
        IWindowService WindowService { get; }
        #endregion

        #region Windows
        IWindow ProgressWindow { get; set; }
        #endregion

        #region Bindables
        public BitmapSource MaximizeRestoreIcon
        {
            get
            {
                if (ShellStateIsNormal) return (BitmapSource)Application.Current.Resources["MaximizeIcon"]; 
                return (BitmapSource)Application.Current.Resources["RestoreIcon"];
            }
        }
        #endregion

        #region Commands
        public ICommand MoveCommand { get; }
        public ICommand MaximizeCommand { get; }
        public ICommand MinimizeCommand { get; }
        public ICommand CloseCommand { get; }
        #endregion

        #region Internals
        bool ShellStateIsNormal => Shell != null && Shell.WindowState == WindowState.Normal;
        IShell Shell { get; set; }
        #endregion

        #endregion

        #region Constructors
        public ShellViewModel(ILoggerFacade logger, ITaskManager taskManager, IWindowService windowService, IShell shell)
        {
            Logger = logger;
            Shell = shell;
            TaskManager = taskManager;
            WindowService = windowService;

            Shell.StateChanged += (s, e) =>
            {
                RaisePropertyChanged(nameof(MaximizeRestoreIcon));
            };

            TaskManager.TaskStarted += async (s, e) =>
            {
                ProgressWindow = await WindowService.CreateWindow<Views.TaskProgressView>(new WindowCreationSettings()
                {
                    IsModal = false, Style = (Style)Application.Current.Resources["BorderlessTopmostWindow"], Topmost = true, Owner = (Window)Shell
                });
            };

            TaskManager.TaskCompleted += (s, e) => WindowService.DestroyWindow(ProgressWindow);

            MoveCommand = new DelegateCommand(OnMoveWindow);
            MinimizeCommand = new DelegateCommand(OnMinimizeShell);
            MaximizeCommand = new DelegateCommand(OnMaximizeShell);
            CloseCommand = new DelegateCommand(OnCloseShell);
        }
        #endregion

        #region Methods

        #region Command Handlers
        void OnMoveWindow()
        {
            Point point = Mouse.GetPosition((Window)Shell);

            if (Shell.WindowState == WindowState.Maximized)
            {
                double top = point.Y;
                double width = Shell.ActualWidth;
                double height = Shell.ActualHeight;
                double left = point.X;


                Shell.WindowState = WindowState.Normal;
                Shell.Top = top - 10;
                Shell.Left = left - (Shell.ActualWidth/2.0);
            }
            else Shell.WindowState = WindowState.Normal;

            //Shell.Left = left;
            //Shell.Width = width;
            //Shell.Height = height;

            try { Shell.DragMove(); }
            catch { }
        }

        void OnMinimizeShell() => Shell.WindowState = WindowState.Minimized;
        void OnRestoreShell() => Shell.WindowState = WindowState.Normal;
        void OnMaximizeShell()
        {
            if (Shell.WindowState == WindowState.Normal)
                Shell.WindowState = WindowState.Maximized;
            else Shell.WindowState = WindowState.Normal;
        }
        void OnCloseShell() => Shell.Close();
        #endregion

        #endregion
    }
}
