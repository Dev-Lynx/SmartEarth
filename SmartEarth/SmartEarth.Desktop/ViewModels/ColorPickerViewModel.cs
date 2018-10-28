using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Logging;
using Prism.Mvvm;
using SmartEarth.Common.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SmartEarth.Desktop.ViewModels
{
    public class ColorPickerViewModel : BindableBase, IInteractionRequestAware
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        #endregion


        #region IInteractionRequestAware Properties
        INotification _notification;
        public INotification Notification { get => _notification; set => SetProperty(ref _notification, value); }
        public Action FinishInteraction { get; set; }
        #endregion

        #region Bindables
        Color _selectedColor;
        public Color SelectedColor { get => _selectedColor; set { SetProperty(ref _selectedColor, value); RaisePropertyChanged(nameof(SelectedBrush)); } }

        public SolidColorBrush SelectedBrush => new SolidColorBrush(SelectedColor);

        Point _currentPosition = new Point();
        public Point CurrentPosition { get => _currentPosition; set => SetProperty(ref _currentPosition, value); }

        public bool CanMove { get; private set; }

        public double SelectorWidth { get; } = 10;
        public double SelectorHeight { get; } = 10;

        #endregion

        #region Commands
        public ICommand MouseMoveCommand { get; }
        public ICommand MouseUpCommand { get; }
        public ICommand MouseDownCommand { get; }
        public ICommand DoneCommand { get; }
        #endregion

        #endregion

        #region Constructors
        public ColorPickerViewModel(ILoggerFacade logger)
        {
            Logger = logger;

            MouseMoveCommand = new DelegateCommand<object>(OnMouseMove);
            MouseUpCommand = new DelegateCommand<object>(OnMouseUp);
            MouseDownCommand = new DelegateCommand<object>(OnMouseDown);
        }
        #endregion

        #region Methods

        #region Command Handlers
        void OnMouseMove(object obj)
        {
            if (!CanMove) return;
            if (!(obj is Image)) return;
            var element = (Image)obj;
            
            var position = Mouse.GetPosition(element);

            if (position.X < 0) position.X = 0;
            else if (position.X > element.ActualWidth) position.X = element.ActualWidth;

            if (position.Y < 0) position.Y = 0;
            else if (position.Y > element.ActualHeight) position.Y = element.ActualHeight;

            
            CurrentPosition = new Point(position.X - (SelectorWidth / 2), position.Y - (SelectorHeight / 2));

            var xScale = element.Source.Width / element.ActualWidth;
            var yScale = element.Source.Height / element.ActualHeight;
            position.X = position.X * xScale;
            position.Y = position.Y * yScale;
            SelectedColor = UIHelper.GetColor((BitmapSource)((Image)obj).Source, position);
        }
        
        void OnMouseUp(object obj)
        {
            if (!(obj is FrameworkElement)) return;
            var element = (FrameworkElement)obj;
            CanMove = false;
            try { element.ReleaseMouseCapture(); }
            catch { }
            
        }

        void OnMouseDown(object obj)
        {
            if (!(obj is FrameworkElement)) return;
            var element = (FrameworkElement)obj;
            CanMove = true;
            try { element.CaptureMouse(); }
            catch { }
        }

        void OnDone(object obj)
        {
            if (!(obj is bool)) return;
            
        }
        #endregion

        #endregion
    }
}
