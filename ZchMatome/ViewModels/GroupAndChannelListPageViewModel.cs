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
    public class GroupAndChannelListPageViewModel : ViewModelBase<GroupAndChannelListPageViewModel>
    {
        #region Initialization and Cleanup
        /******************************
         * Initialization and Cleanup *
         ******************************/

        public GroupAndChannelListPageViewModel() { }

        public GroupAndChannelListPageViewModel(PhoneApplicationFrame app, INavigator navigator, Services.IZchMatomeService service, FeedDataContext dataContext)
        {
            this.app = app;
            this.navigator = navigator;
            this.service = service;
            this.dataContext = dataContext;
            InitPivotItems();
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

        PhoneApplicationFrame app;
        INavigator navigator;
        IZchMatomeService service;
        FeedDataContext dataContext;

        #endregion

        #region Properties
        /**************
         * Properties *
         **************/

        private GroupListViewModel _GroupListViewModel = null;
        public GroupListViewModel GroupListViewModel
        {
            get { return _GroupListViewModel; }
            set
            {
                if (_GroupListViewModel == value) return;
                _GroupListViewModel = value;
                NotifyPropertyChanged(m => GroupListViewModel);
            }
        }

        private ChannelListViewModel _ChannelListViewModel = null;
        public ChannelListViewModel ChannelListViewModel
        {
            get { return _ChannelListViewModel; }
            set
            {
                if (_ChannelListViewModel == value) return;
                _ChannelListViewModel = value;
                NotifyPropertyChanged(m => ChannelListViewModel);
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
                    SendMessage(Constants.MessageTokens.ReloadRequested, new NotificationEventArgs());
                }
                );
            }
        }

        #endregion

        #region Methods
        /***********
         * Methods *
         ***********/

        private void InitPivotItems()
        {
            /*
            GroupListViewModel =
                new GroupListViewModel
                (
                    app, navigator, service, dataContext
                );
            GroupListViewModel.ErrorNotice += OnErrorNotice;
            */

            ChannelListViewModel =
                new ChannelListViewModel
                (
                    app, navigator, service, dataContext
                );
            ChannelListViewModel.ErrorNotice += OnErrorNotice;
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


        private void OnErrorNotice(object sender, NotificationEventArgs<Exception> e)
        {
            Notify(ErrorNotice, e);
        }

        #endregion
    }
}
