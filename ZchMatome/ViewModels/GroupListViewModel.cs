using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ZchMatome.Data;
using ZchMatome.Navigation;
using ZchMatome.Services;
using Helpers;
using Microsoft.Phone.Controls;
using SimpleMvvmToolkit;

namespace ZchMatome.ViewModels
{
    public class GroupListViewModel : ViewModelBase<ChannelListViewModel>
    {
        #region Initialization and Cleanup
        /******************************
         * Initialization and Cleanup *
         ******************************/

        public GroupListViewModel() { }

        public GroupListViewModel(PhoneApplicationFrame app, INavigator navigator, Services.IZchMatomeService service, FeedDataContext dataContext)
        {
            RegisterToReceiveMessages(Constants.MessageTokens.FeedGroupsUpdated, OnFeedGroupsUpdated);
            this.app = app;
            this.navigator = navigator;
            this.service = service;
            this.dataContext = dataContext;
            LoadFeedGroups();
        }

        #endregion

        #region Notifications
        /*****************
         * Notifications *
         *****************/

        public event EventHandler<NotificationEventArgs<Exception>> ErrorNotice;

        private void OnFeedGroupsUpdated(object sender, NotificationEventArgs e)
        {
            LoadFeedGroups();
        }

        #endregion

        #region Services
        /************
         * Services *
         ************/

        PhoneApplicationFrame app;
        INavigator navigator;
        IZchMatomeService service;
        FeedDataContext dataContext;

        #endregion

        #region Properties
        /**************
         * Properties *
         **************/

        private ObservableCollection<FeedGroup> _FeedGroups;
        public ObservableCollection<FeedGroup> FeedGroups
        {
            get { return _FeedGroups; }
            set
            {
                if (_FeedGroups == value) return;
                _FeedGroups = value;
                NotifyPropertyChanged(m => FeedGroups);
            }
        }

        #endregion

        #region Commands
        /************
         * Commands *
         ************/

        public ICommand ListSelectionChangedCommand
        {
            get
            {
                return new DelegateCommand<LongListSelector>((e) =>
                {
                    if (e.SelectedItem != null)
                    {
                        NavigationService.Navigate(new Uri("/Views/GroupDetailPage.xaml", UriKind.Relative), e.SelectedItem);
                        e.SelectedItem = null;
                    }
                }
                );
            }
        }

        #endregion

        #region Methods
        /***********
         * Methods *
         ***********/

        private void LoadFeedGroups()
        {
            lock (dataContext)
            {
                var groups =
                    from theGroup in dataContext.FeedGroups
                    where theGroup.Class == 1 && theGroup.Status == 1
                    orderby theGroup.Title ascending
                    select theGroup;
                FeedGroups = new ObservableCollection<FeedGroup>(groups);
            };
        }

        #endregion

        #region Completion Callbacks
        /************************
         * Completion Callbacks *
         ************************/

        #endregion

        #region Helpers
        /***********
         * Helpers *
         ***********/

        private void NotifyError(string message, Exception error)
        {
            Notify(ErrorNotice, new NotificationEventArgs<Exception>(message, error));
        }

        #endregion
    }
}
