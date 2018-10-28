using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace SmartEarth.Demos.Sorting.ViewModels
{
    public class ShellViewModel : BindableBase
    {
        #region Properties

        #region Statics
        const double RefreshInterval = 2000;
        static readonly List<string> Names = new List<string>()
        {
            "Marin", "Liberty", "Lidia", "Erik", "Sulema", "Christene",
            "Fernanda", "Alvera", "Arturo", "Livia", "Jeane", "Thomasina",
            "Jovita", "Zachary", "Tamekia", "Vella", "Porsha", "Scarlett",
            "Zenobia", "Katrice", "Dianna", "Eddy", "Maryanna", "Zetta",
            "Marisa", "Dustin", "Leigh", "Beaulah", "Joslyn", "Shemeka"
        };
        const string ADD_REMOVE_BUTTON_START = "Start Recreating";
        const string ADD_REMOVE_BUTTON_STOP = "Stop Recreating";
        const string CHANGE_BUTTON_START = "Start Reshuffling";
        const string CHANGE_BUTTON_STOP = "Stop Reshuffling";
        #endregion

        #region Bindables

        public ICollectionView View { get; }
        public ICollectionViewLiveShaping SortedView { get; }

        public string ChangeButtonText => ChangeActive ? CHANGE_BUTTON_STOP : CHANGE_BUTTON_START;
        public string AddButtonText => AddRemoveActive ? ADD_REMOVE_BUTTON_STOP : ADD_REMOVE_BUTTON_START;

        #endregion

        #region Commands
        public ICommand StartChangingCommand { get; }
        public ICommand StartAddCommand { get; }
        #endregion

        #region Internals
        ObservableCollection<Person> People { get; } = new ObservableCollection<Person>()
        {
            new Person() { Id = 1, Name = "Prince", Worth = 1000 },
            new Person() { Id = 2, Name = "Zion", Worth = 1000 },
            new Person() { Id = 3, Name = "Queen", Worth = 2900 },
            new Person() { Id = 4, Name = "Israel", Worth = 1000 },
            new Person() { Id = 5, Name = "Andy", Worth = 1000 },
            new Person() { Id = 6, Name = "Victoria", Worth = 1000 },
            new Person() { Id = 7, Name = "Harry", Worth = 1000 },
            new Person() { Id = 8, Name = "Isaac", Worth = 500 },
            new Person() { Id = 9, Name = "Z", Worth = 34 },
            new Person() { Id = 10, Name = "Anthony", Worth = 5000 },
            new Person() { Id = 11, Name = "PadFoot", Worth = 893 },
            new Person() { Id = 12, Name = "Thomas", Worth = 1 },
        };
        Random Randomizer { get; } = new Random();
        bool ChangeActive { get; set; } = false;
        bool AddRemoveActive { get; set; } = false;
        DispatcherTimer ChangeTimer { get; } = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(RefreshInterval) };
        DispatcherTimer AddRemoveTimer { get; } = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(RefreshInterval) };
        #endregion

        #endregion

        #region Constructors
        public ShellViewModel()
        {
            View = CollectionViewSource.GetDefaultView(People);
            View.SortDescriptions.Add(new SortDescription("Worth", ListSortDirection.Descending));

            SortedView = (ICollectionViewLiveShaping)View;
            SortedView.IsLiveSorting = true;

            StartChangingCommand = new DelegateCommand(OnStartChange);
            StartAddCommand = new DelegateCommand(OnStartAddRemove);
        }

        #endregion

        #region Methods

        #region Command Handlers
        void OnStartChange()
        {
            if (ChangeActive)
            {
                ChangeTimer.Stop();
                ChangeActive = false;
                RaisePropertyChanged(nameof(ChangeButtonText));
                return;
            }

            ChangeActive = true;
            ChangeTimer.Interval = TimeSpan.FromMilliseconds(RefreshInterval);
            RaisePropertyChanged(nameof(ChangeButtonText));

            ChangeTimer.Tick += (s, e) =>
            {
                foreach (var person in People)
                    person.Worth = RandomCash();
                RaisePropertyChanged(nameof(People));
            };
            ChangeTimer.Start();
        }

        void OnStartAddRemove()
        {
            if (AddRemoveActive)
            {
                AddRemoveTimer.Stop();
                AddRemoveActive = false;
                RaisePropertyChanged(nameof(AddButtonText));
                return;
            }

            AddRemoveActive = true;
            AddRemoveTimer.Interval = TimeSpan.FromMilliseconds(RefreshInterval);
            RaisePropertyChanged(nameof(AddButtonText));

            AddRemoveTimer.Tick += (s, e) =>
            {
                WorldAction action = (WorldAction)Randomizer.Next(3);

                switch (action)
                {
                    case WorldAction.Add:
                        People.Add(new Person() { Name = Names[Randomizer.Next(Names.Count)], Id = Names.Count, Worth = RandomCash() });
                        break;

                    case WorldAction.Remove:
                        People.RemoveAt(Randomizer.Next(People.Count));
                        break;
                }
                Application.Current.Dispatcher.Invoke(() => RaisePropertyChanged(nameof(People)));
            };
            AddRemoveTimer.Start();
        }
        #endregion

        int RandomCash(int min = 10, int max = 1000)
        {
            int cash = 6;
            while (cash % 5 != 0) cash = Randomizer.Next(min, max);
            return cash;
        }

        #endregion
    }
}
