using Prism.Commands;
using Prism.Logging;
using Prism.Mvvm;
using Prism.Regions;
using SmartEarth.Common.Infrastructure;
using SmartEarth.Common.Infrastructure.Extensions;
using SmartEarth.Common.Infrastructure.Models.Interfaces;
using SmartEarth.Common.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace SmartEarth.Desktop.ViewModels
{
    public class RecentViewModel : BindableBase
    {
        #region Properties

        #region Statics
        public double OriginalImageHeight { get; } = 200;
        public double AnimatedImageHeight { get; } = 300;
        #endregion

        #region Services
        ILoggerFacade Logger { get; }
        IRegionManager RegionManger { get; }
        IViewManager ViewManager { get; }
        #endregion

        #region Bindables
        public ObservableCollection<IViewable> Viewables => ViewManager.Recent;
        #endregion

        #region Commands
        public ICommand MouseEnterCommand { get; }
        public ICommand MouseLeaveCommand { get; }
        public ICommand MouseClickCommand { get; }
        public ICommand MouseScrollCommand { get; }
        #endregion

        #region Internals
        bool AnimatingImage { get; set; } = false;
        Timer ScrollTimer { get; set; }

        bool _scrolling = false;
        bool Scrolling
        {
            get => _scrolling;
            set
            {
                bool wasActive = _scrolling;
                _scrolling = value;

                if (!wasActive && _scrolling)
                {
                    ScrollTimer = new Timer(1000);

                    ElapsedEventHandler elapsed = null;
                    ScrollTimer.Elapsed += elapsed = (s, e) =>
                    {
                        ScrollTimer.Elapsed -= elapsed;
                        Scrolling = false;
                    };
                    ScrollTimer.Start();
                }
                else if (_scrolling)
                {
                    ScrollTimer.Stop();
                    ScrollTimer.Start();
                }

                if (ScrollTimer != null) ScrollTimer.Dispose();
            }
        }
        #endregion

        #endregion

        #region Constructors
        public RecentViewModel(ILoggerFacade logger, IViewManager viewManager, IRegionManager regionManager)
        {
            Logger = logger;
            ViewManager = viewManager;
            RegionManger = regionManager;

            MouseEnterCommand = new DelegateCommand<object>(OnMouseEnter);
            MouseLeaveCommand = new DelegateCommand<object>(OnMouseLeave);
            MouseClickCommand = new DelegateCommand<object>(OnClick);
            MouseScrollCommand = new DelegateCommand(OnMouseScroll);
        }
        #endregion

        #region Methods

        #region Command Handlers
        async void OnMouseEnter(object obj)
        {
            if (Scrolling) return;
            if (!(obj is FrameworkElement)) return;

            var element = (FrameworkElement)obj;

            IViewable view = null;

            if (element.DataContext is IViewable)
                view = (IViewable)element.DataContext;

            // Wait for a while to make sure the user really wants a preview
            for (int i = 0; i < 5; i++)
            {
                if (!element.IsMouseOver) return;
                await Task.Delay(100);
                if (!element.IsMouseOver) return;
            }

            if (view != null && view.Expanded) return;

            while (AnimatingImage)
                await Task.Delay(100);

            


            AnimatingImage = true;
            await (element.AnimateHeight(OriginalImageHeight, AnimatedImageHeight, 1, false));

            if (view != null) view.Expanded = true;

            AnimatingImage = false;

        }

        async void OnMouseLeave(object obj)
        {
            if (!(obj is FrameworkElement)) return;

            var element = (FrameworkElement)obj;

            IViewable view = null;

            if (element.DataContext is IViewable)
                view = (IViewable)element.DataContext;

            // Wait for a while to make sure the user really wants a preview
            for (int i = 0; i < 5; i++)
            {
                if (element.IsMouseOver) return;
                await Task.Delay(100);
                if (element.IsMouseOver) return;
            }

            if (view != null && !view.Expanded) return;

            while (AnimatingImage)
                await Task.Delay(100);

            AnimatingImage = true;
            await (element.AnimateHeight(AnimatedImageHeight, OriginalImageHeight, 1, false));

            if (view != null) view.Expanded = false;
            AnimatingImage = false;
        }

        void OnClick(object obj)
        {
            if (!(obj is IViewable)) return;
            var view = (IViewable)obj;

            NavigationParameters parameters = new NavigationParameters();
            parameters.Add("View", view);
            RegionManger.RequestNavigateToView(Core.HOME_REGION, Core.IMAGE_VIEW, parameters);
            
        }

        void OnMouseScroll() => Scrolling = true;
        #endregion

        #endregion
    }
}
