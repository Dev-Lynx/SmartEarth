using Prism.Logging;
using Prism.Mvvm;
using Prism.Regions;
using SmartEarth.Common.Infrastructure.Extensions;
using SmartEarth.Common.Infrastructure.Models;
using SmartEarth.Common.Infrastructure.Models.Interfaces;
using SmartEarth.Common.Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmartEarth.Common.Infrastructure.Services
{
    public class ViewManager : BindableBase, IViewManager
    {
        #region Properties

        #region Services
        ILoggerFacade Logger { get; }
        IDatabaseManager DatabaseManager { get; }
        #endregion

        #region Bindables
        public ObservableCollection<IViewable> Recent { get; } = new ObservableCollection<IViewable>();
        public IViewable View { get; private set; }
        #endregion

        #region Internals
        string _currentView = string.Empty;
        string CurrentView
        {
            get => _currentView;
            set
            {
                switch (value)
                {
                    case Core.RECENT_VIEW:
                        LoadRecent();
                        break;
                }
            }
        }
        #endregion

        #endregion

        #region Constructors
        public ViewManager(ILoggerFacade logger, IDatabaseManager databaseManager)
        {
            Logger = logger;
            DatabaseManager = databaseManager;

            UIHelper.ViewChanged += (s, e) => CurrentView = e.View;
        }
        #endregion

        #region Methods

        #region IViewManager Implementation
        public void SetView(IViewable view)
        {
            if (View != null) View.Reset();

            View = view;
            if (view == null) return;
            
            View.Load();
            RaisePropertyChanged(nameof(View));
        }

        public void LoadRecent()
        {
            Recent.Clear();
            for (int i = 0; i < DatabaseManager.CompletedTasks.Count; i++)
            {
                var viewable = ((IViewable)DatabaseManager.CompletedTasks[i]);
                Recent.Add(viewable);
                viewable.Load(true, new Int32Rect(0, 100, SmartEarthImage.STRETCH_THUMBNAIL_WIDTH, SmartEarthImage.STRECTH_THUMBNAIL_HEIGHT));
            }
        }
        #endregion

        #endregion
    }
}
