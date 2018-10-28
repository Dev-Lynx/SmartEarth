using Prism.Commands;
using Prism.Logging;
using Prism.Mvvm;
using Prism.Regions;
using SmartEarth.Common.Infrastructure.Extensions;
using SmartEarth.Common.Infrastructure.Models.Interfaces;
using SmartEarth.Common.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartEarth.Desktop.ViewModels
{
    public class ImageViewModel : BindableBase, INavigationAware
    {
        #region Properties

        #region Constants
        const string HAND_CURSOR = "HandCursor";
        const string CLOSED_HAND_CURSOR = "ClosedHandCursor";
        const double ZOOM_MIN = 100;
        const double ZOOM_MAX = 1000;
        #endregion

        #region Services
        ILoggerFacade Logger { get; }
        IViewManager ViewManager { get; }
        #endregion

        #region Bindables
        public IViewable View => ViewManager.View;

        Cursor _cursor = UIHelper.LoadCursor(HAND_CURSOR);
        public Cursor CurrentCursor { get => _cursor; private set => SetProperty(ref _cursor, value); } 

        bool _zoomActive = false;
        public bool ZoomActive
        {
            get => _zoomActive;
            private set
            {
                SetProperty(ref _zoomActive, value);

                if (ZoomActiveTimer == null || !ZoomActiveTimer.Enabled)
                {
                    ZoomActiveTimer = new Timer(3000)
                    {
                        AutoReset = false
                    };
                    ZoomActiveTimer.Start();
                }

                ElapsedEventHandler elapsed = null;
                ZoomActiveTimer.Elapsed += elapsed = (s, e) =>
                {
                    ZoomActiveTimer.Elapsed -= elapsed;
                    ZoomActiveTimer.Stop();
                    ZoomActiveTimer.Dispose();
                    ZoomActiveTimer = null;
                    ZoomActive = false;
                };
            }
        }

        double _zoomScale = 100;
        public double ZoomScale
        {
            get => _zoomScale;
            private set
            {
                if (value < ZOOM_MIN) value = ZOOM_MIN;
                else if (value > ZOOM_MAX) value = ZOOM_MAX;
                SetProperty(ref _zoomScale, value);
            }
        }

        double _scrollOffsetX = .0;
        public double ScrollOffsetX
        {
            get => _scrollOffsetX;
            set
            {
                SetProperty(ref _scrollOffsetX, value);
                Logger.Debug($"Offset X {_scrollOffsetX}");
            }
        }

        double _scrollOffsetY = .0;
        public double ScrollOffsetY
        {
            get => _scrollOffsetY;
            set
            {
                SetProperty(ref _scrollOffsetY, value);
               // Logger.Debug($"Offset Y {_scrollOffsetY}");
            }
        }

        public double ViewPortHeight { get; set; }
        public double ViewPortWidth { get; set; }
        #endregion

        #region Commands
        public ICommand MouseUpCommand { get; }
        public ICommand MouseDownCommand { get; }
        public ICommand MouseMoveCommand { get; }
        public ICommand MouseWheelCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand PreviousCommand { get; }
        public ICommand ChangeViewCommand { get; }
        public ICommand ScrollCommand { get; }
        #endregion

        #region Internals
        double OldScrollX { get; set; }
        double OldScrollY { get; set; }
        Point Position { get; set; }
        FrameworkElement Element { get; set; }
        Timer ZoomActiveTimer { get; set; }
        #endregion

        #endregion

        #region Constructors
        public ImageViewModel(ILoggerFacade logger, IViewManager viewManager)
        {
            Logger = logger;
            ViewManager = viewManager;

            MouseUpCommand = new DelegateCommand<object>(OnMouseUp);
            MouseDownCommand = new DelegateCommand<object>(OnMouseDown);
            MouseMoveCommand = new DelegateCommand(OnMouseMove);
            MouseWheelCommand = new DelegateCommand<object>(OnMouseScroll);
            ScrollCommand = new DelegateCommand<object>(OnScroll);
        }
        #endregion

        #region Methods

        #region Command Handlers

        #region INavigationAware Implementation
        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            try
            {
                var view = (IViewable)navigationContext.Parameters["View"];
                ViewManager.SetView(view);
                RaisePropertyChanged(nameof(View));
            }
            catch (Exception ex)
            {
                Logger.Error("An error occured during navigation\n {0}", ex);
            }
        }        

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            if (View != null) View.Reset();
        }
        #endregion


        void OnMouseUp(object obj)
        {
            if (!(obj is FrameworkElement)) return;
            Element = (FrameworkElement)obj;

            CurrentCursor = UIHelper.LoadCursor(HAND_CURSOR);

            try { Element.ReleaseMouseCapture(); }
            catch { }

            
        }

        void OnMouseDown(object obj)
        {
            if (!(obj is FrameworkElement)) return;
            Element = (FrameworkElement)obj;

            try { Element.CaptureMouse(); }
            catch { }

            CurrentCursor = UIHelper.LoadCursor(CLOSED_HAND_CURSOR);
            Position = Mouse.GetPosition(Element);
            OldScrollX = ScrollOffsetX;
            OldScrollY = ScrollOffsetY;
        }

        void OnMouseMove()
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed) return;
            if (!Element.IsMouseCaptured) return;
            
            var position = Mouse.GetPosition(Element);

            var xDiff = (position.X - Position.X) * ZoomScale/200;
            var yDiff = (position.Y - Position.Y) * ZoomScale/200;

            Logger.Debug("Difference  {0}, {1}", xDiff, yDiff);

            var xOffset = (OldScrollX - xDiff);
            var yOffset =  OldScrollY - yDiff;


            Logger.Debug("Offset {0}, {1}", xOffset, yOffset);

            if (xOffset < 0) xOffset = 0;
            else if (xOffset > ViewPortWidth)
            {
                xOffset = ViewPortWidth;
                Logger.Debug("Higher!!!");
            }

            if (yOffset < 0) yOffset = 0;
            else if (yOffset > ViewPortHeight) yOffset = ViewPortHeight;
            

            ScrollOffsetX = xOffset;
            ScrollOffsetY = yOffset;
        }

        void OnMouseScroll(object obj)
        {
            if (!(obj is MouseWheelEventArgs)) return;

            MouseWheelEventArgs e = (MouseWheelEventArgs)obj;

            double factor = Math.Abs(e.Delta / 120.0);

            if (e.Delta < 0) ZoomScale -= 25 * factor;
            else if (e.Delta > 0) ZoomScale += 25 * factor;
            
            e.Handled = true;
            ZoomActive = true;
        }
        
        void OnScroll(object obj)
        {
            if (!(obj is ScrollChangedEventArgs)) return;

            var e = (ScrollChangedEventArgs)obj;
            
            //Logger.Debug("{0}, {1}", e.HorizontalOffset, e.VerticalOffset);
        }


        #endregion

        #endregion
    }
}
