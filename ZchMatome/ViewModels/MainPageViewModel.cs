using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using ZchMatome.Data;
using ZchMatome.Navigation;
using ZchMatome.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SimpleMvvmToolkit;

namespace ZchMatome.ViewModels
{
    public class MainPageViewModel : ViewModelBase<MainPageViewModel>
    {
        #region Initialization and Cleanup
        /******************************
         * Initialization and Cleanup *
         ******************************/

        public MainPageViewModel() { }

        public MainPageViewModel(PhoneApplicationFrame app, INavigator navigator, Services.IZchMatomeService service, FeedDataContext dataContext)
        {
            RegisterToReceiveMessages(Constants.MessageTokens.MainPageInitializeCompleted, OnInitializeCompleted);
            RegisterToReceiveMessages(Constants.MessageTokens.ReloadRequested, OnReloadRequested);
            RegisterToReceiveMessages(Constants.MessageTokens.NotificationUpdated, OnNotificationUpdated);
            this.app = app;
            this.navigator = navigator;
            this.service = service;
            this.dataContext = dataContext;
            IsInitializing = true;

            if (!IsInDesignMode)
            {
                bool updated = false;
                float? version = Helpers.AppSettings.GetValueOrDefault<float?>(Constants.AppKey.Version, null);
                if (version == null)
                {
                    System.Diagnostics.Debug.WriteLine("first boot");
                }
                else if (version < Helpers.AppAttributes.VersionAsFloat)
                {
                    updated = true;
                    System.Diagnostics.Debug.WriteLine("version up from " + version + " to " + Helpers.AppAttributes.VersionAsFloat);
                }
                Helpers.AppSettings.AddOrUpdateValue(Constants.AppKey.Version, Helpers.AppAttributes.VersionAsFloat);

                DateTime lastUpdate = Helpers.AppSettings.GetValueOrDefault<DateTime>(Constants.AppKey.LastUpdate, DateTime.MinValue);
                if (updated || DateTime.Compare(lastUpdate, DateTime.Now.AddDays(-Constants.App.FeedChannelExpireDays)) < 0)
                {
                    LoadAllFeedGroupsAndChannels();
                }
                else
                {
                    SendMessage(Constants.MessageTokens.MainPageInitializeCompleted, new NotificationEventArgs());
                }
            }
        }

        #endregion

        #region Notifications
        /*****************
         * Notifications *
         *****************/

        public event EventHandler<NotificationEventArgs<Exception>> ErrorNotice;

        private void OnInitializeCompleted(object sender, NotificationEventArgs e)
        {
            string uuid = Helpers.AppSettings.GetValueOrDefault<string>(Constants.AppKey.NotificationUuid, null);
            if (uuid != null)
            {
                CultureInfo uicc = Thread.CurrentThread.CurrentUICulture;
                service.UpdateNotificationChannel(uuid, Helpers.AppAttributes.Version, uicc.Name, null, null, true, UpdateNotificationChannelCompleted);
            }

            InitPivotItems();
            LoadPivotItem(0, true);

            ShellTile shellTile = ShellTile.ActiveTiles.First();
            StandardTileData shellTileData = new StandardTileData()
            {
                Title = null,
                BackgroundImage = null,
                Count = 0,
            };
            shellTile.Update(shellTileData);

            IsInitializing = false;
        }

        private void OnReloadRequested(object sender, NotificationEventArgs e)
        {
            LoadPivotItem(0, true);
            LoadPivotItem(1, true);
        }

        private void OnNotificationUpdated(object sender, NotificationEventArgs e)
        {
            lock (dataContext)
            {
                int[] subscriptionChannelIds =
                    (
                        from channel in dataContext.FeedChannels
                        where channel.Subscription == true && channel.FeedGroup.Class == 1 && channel.FeedGroup.Subscription == true
                        select channel.Id
                    )
                    .ToArray();
                int[] notificationChannelIds =
                    (
                        from channel in dataContext.FeedChannels
                        where channel.Notification == true && channel.FeedGroup.Class == 1 && channel.FeedGroup.Subscription == true
                        select channel.Id
                    )
                    .ToArray();
                string uuid = Helpers.AppSettings.GetValueOrDefault<string>(Constants.AppKey.NotificationUuid, null);
                if (uuid != null)
                {
                    CultureInfo uicc = Thread.CurrentThread.CurrentUICulture;
                    service.UpdateNotificationChannel(uuid, Helpers.AppAttributes.Version, uicc.Name, subscriptionChannelIds, notificationChannelIds, true, UpdateNotificationChannelCompleted);
                }
            };
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

        private bool _IsInitializing = true;
        public bool IsInitializing
        {
            get { return _IsInitializing; }
            set
            {
                if (_IsInitializing == value) return;
                _IsInitializing = value;
                NotifyPropertyChanged(m => IsInitializing);
            }
        }

        private bool _IsBusy = false;
        public bool IsBusy
        {
            get { return _IsBusy; }
            set
            {
                if (_IsBusy == value) return;
                _IsBusy = value;
                NotifyPropertyChanged(m => IsBusy);
            }
        }

        private ChannelsUpdatesListViewModel _SubscriptionChannelsUpdatesListViewModel = null;
        public ChannelsUpdatesListViewModel SubscriptionChannelsUpdatesListViewModel
        {
            get { return _SubscriptionChannelsUpdatesListViewModel; }
            set
            {
                if (_SubscriptionChannelsUpdatesListViewModel == value) return;
                _SubscriptionChannelsUpdatesListViewModel = value;
                NotifyPropertyChanged(m => SubscriptionChannelsUpdatesListViewModel);
            }
        }

        private ChannelsUpdatesListViewModel _FavoriteChannelsUpdatesListViewModel = null;
        public ChannelsUpdatesListViewModel FavoriteChannelsUpdatesListViewModel
        {
            get { return _FavoriteChannelsUpdatesListViewModel; }
            set
            {
                if (_FavoriteChannelsUpdatesListViewModel == value) return;
                _FavoriteChannelsUpdatesListViewModel = value;
                NotifyPropertyChanged(m => FavoriteChannelsUpdatesListViewModel);
            }
        }

        #endregion

        #region Commands
        /************
         * Commands *
         ************/

        public ICommand PivotSelectionChangedCommand
        {
            get
            {
                return new DelegateCommand<Pivot>(
                (e) =>
                {
                    LoadPivotItem(e.SelectedIndex, false);
                }
                );
            }
        }

        #endregion

        #region Methods
        /***********
         * Methods *
         ***********/

        public void LoadAllFeedGroupsAndChannels()
        {
            IsBusy = true;
            service.GetAllFeedGroupsAndChannels(dataContext, GetAllFeedGroupsAndChannelsCompleted);
        }

        private void InitPivotItems()
        {
            SubscriptionChannelsUpdatesListViewModel =
                new ChannelsUpdatesListViewModel
                (
                    app, navigator, service, dataContext,
                    Localization.AppResources.MainPage_PivotItem_ChannelUpdates
                );
            SubscriptionChannelsUpdatesListViewModel.ErrorNotice += OnErrorNotice;

            FavoriteChannelsUpdatesListViewModel =
                new ChannelsUpdatesListViewModel
                (
                    app, navigator, service, dataContext,
                    Localization.AppResources.MainPage_PivotItem_Favorites
                );
            FavoriteChannelsUpdatesListViewModel.ErrorNotice += OnErrorNotice;
        }

        private void LoadPivotItem(int id, bool force)
        {
            switch (id)
            {
                case 0:
                    {
                        if (force || SubscriptionChannelsUpdatesListViewModel.FeedItems.Count == 0)
                        {
                            lock (dataContext)
                            {
                                int[] channelIds =
                                    (
                                        from channel in dataContext.FeedChannels
                                        where channel.Subscription == true && channel.FeedGroup.Class == 1 && channel.FeedGroup.Subscription == true
                                        select channel.Id
                                    )
                                    .ToArray();
                                SubscriptionChannelsUpdatesListViewModel.SetChannelIds(channelIds);
                            };
                            SubscriptionChannelsUpdatesListViewModel.LoadFeedItems(true, false);
                        }
                        break;
                    }
                case 1:
                    {
                        if (force || FavoriteChannelsUpdatesListViewModel.FeedItems.Count == 0)
                        {
                            FavoriteChannelsUpdatesListViewModel.SetAsFavorites();
                            FavoriteChannelsUpdatesListViewModel.LoadFeedItems(true, false);
                    }
                        break;
                    }
            }
        }

        #endregion

        #region Completion Callbacks
        /************************
         * Completion Callbacks *
         ************************/

        void UpdateNotificationChannelCompleted(ZchMatomeService.UpdateNotificationChannelResult result, Exception error)
        {
        }

        private void GetAllFeedGroupsAndChannelsCompleted(Exception error)
        {
            if (!IsBusy)
            {
                return;
            }

            IsBusy = false;

            if (error != null)
            {
                NotifyError(Localization.AppResources.MainPage_Error_FailedToGetAllFeedGroupsAndChannels, error);
                return;
            }

            Helpers.AppSettings.AddOrUpdateValue(Constants.AppKey.LastUpdate, DateTime.Now);

            SendMessage(Constants.MessageTokens.MainPageInitializeCompleted, new NotificationEventArgs());
        }

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