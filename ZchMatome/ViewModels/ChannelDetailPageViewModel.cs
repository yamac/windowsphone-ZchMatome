using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ZchMatome.Data;
using ZchMatome.Navigation;
using ZchMatome.Services;
using Microsoft.Phone.Controls;
using SimpleMvvmToolkit;

namespace ZchMatome.ViewModels
{
    public class ChannelDetailPageViewModel : ViewModelBase<ChannelListViewModel>
    {
        #region Initialization and Cleanup
        /******************************
         * Initialization and Cleanup *
         ******************************/

        public ChannelDetailPageViewModel() { }

        public ChannelDetailPageViewModel(PhoneApplicationFrame app, INavigator navigator, IZchMatomeService service, FeedDataContext dataContext)
        {
            this.app = app;
            this.navigator = navigator;
            this.service = service;
            this.dataContext = dataContext;
            int channelId = ((FeedChannel)NavigationService.NavigationArgs).Id;
            LoadFeedChannel(channelId);
            TheChannelsUpdatesListViewModel =
                new ChannelsUpdatesListViewModel(app, navigator, service, dataContext, Localization.AppResources.ChannelDetailPage_PivotItem_ChannelUpdates);
            TheChannelsUpdatesListViewModel.SetChannelIds(new int[] { channelId });
            TheChannelsUpdatesListViewModel.ErrorNotice += OnErrorNotice;
            TheChannelsUpdatesListViewModel.LoadFeedItems(true, false);
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

        private FeedChannel _TheFeedChannel;
        public FeedChannel TheFeedChannel
        {
            get { return _TheFeedChannel; }
            set
            {
                if (_TheFeedChannel == value) return;
                _TheFeedChannel = value;
                NotifyPropertyChanged(m => TheFeedChannel);
            }
        }

        private ChannelsUpdatesListViewModel _TheChannelsUpdatesListViewModel = null;
        public ChannelsUpdatesListViewModel TheChannelsUpdatesListViewModel
        {
            get { return _TheChannelsUpdatesListViewModel; }
            set
            {
                if (_TheChannelsUpdatesListViewModel == value) return;
                _TheChannelsUpdatesListViewModel = value;
                NotifyPropertyChanged(m => TheChannelsUpdatesListViewModel);
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

        private void LoadFeedChannel(int id)
        {
            lock (dataContext)
            {
                TheFeedChannel = dataContext.FeedChannels.Single(channel => channel.Id == id);
            };
        }

        private void SubmitChanges()
        {
            dataContext.SubmitChanges();
            SendMessage(Constants.MessageTokens.FeedChannelsUpdated, new NotificationEventArgs());
            SendMessage(Constants.MessageTokens.NotificationUpdated, new NotificationEventArgs());
        }

        private void RejectChanges()
        {
            dataContext.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues, TheFeedChannel);
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
