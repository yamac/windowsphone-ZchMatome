using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ZchMatome.Data;
using ZchMatome.Navigation;
using Microsoft.Phone.Controls;
using SimpleMvvmToolkit;

namespace ZchMatome.ViewModels
{
    public class GroupDetailPageViewModel : ViewModelBase<GroupDetailPageViewModel>
    {
        #region Initialization and Cleanup
        /******************************
         * Initialization and Cleanup *
         ******************************/

        public GroupDetailPageViewModel() { }

        public GroupDetailPageViewModel(INavigator navigator, FeedDataContext dataContext)
        {
            this.navigator = navigator;
            this.dataContext = dataContext;
            LoadFeedGroup(((FeedGroup)NavigationService.NavigationArgs).Id);
        }

        #endregion

        #region Notifications
        /*****************
         * Notifications *
         *****************/

        public event EventHandler<NotificationEventArgs<Exception>> ErrorNotice;

        #endregion

        #region Services
        /************
         * Services *
         ************/

        INavigator navigator;
        FeedDataContext dataContext;

        #endregion

        #region Properties
        /**************
         * Properties *
         **************/

        private FeedGroup _TheFeedGroup;
        public FeedGroup TheFeedGroup
        {
            get { return _TheFeedGroup; }
            set
            {
                if (_TheFeedGroup == value) return;
                _TheFeedGroup = value;
                NotifyPropertyChanged(m => TheFeedGroup);
            }
        }

        #endregion

        #region Commands
        /************
         * Commands *
         ************/

        public ICommand BackKeyPressCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    RejectChanges();
                }
                );
            }
        }

        public ICommand OkCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    SubmitChanges();
                    NavigationService.GoBack();
                }
                );
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    RejectChanges();
                    NavigationService.GoBack();
                }
                );
            }
        }

        #endregion

        #region Methods
        /***********
         * Methods *
         ***********/

        private void LoadFeedGroup(int id)
        {
            lock (dataContext)
            {
                TheFeedGroup = dataContext.FeedGroups.Single(group => group.Id == id);
            };
        }

        private void SubmitChanges()
        {
            dataContext.SubmitChanges();
            SendMessage(Constants.MessageTokens.FeedGroupsUpdated, new NotificationEventArgs());
            SendMessage(Constants.MessageTokens.NotificationUpdated, new NotificationEventArgs());
        }

        private void RejectChanges()
        {
            dataContext.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, TheFeedGroup);
            SendMessage(Constants.MessageTokens.FeedGroupsUpdated, new NotificationEventArgs());
            SendMessage(Constants.MessageTokens.NotificationUpdated, new NotificationEventArgs());
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
