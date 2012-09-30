using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Reactive;
using SimpleMvvmToolkit;
using ZchMatome.Data;
using ZchMatome.Navigation;
using ZchMatome.Services;
using System.ComponentModel;

namespace ZchMatome.ViewModels
{
    public class ChannelsUpdatesListViewModel : ViewModelBase<ChannelsUpdatesListViewModel>
    {
        #region Classes

        public class MarkableFeedItem : FeedItem
        {
            public static ChannelsUpdatesListViewModel TheViewModel;

            public MarkableFeedItem(FeedItem feedItem)
            {
                this.Id = feedItem.Id;
                this.FeedGroupId = feedItem.FeedGroupId;
                this.FeedGroupTitle = feedItem.FeedGroupTitle;
                this.FeedGroupAccentColor = feedItem.FeedGroupAccentColor;
                this.FeedChannelId = feedItem.FeedChannelId;
                this.FeedChannelFeedLink = feedItem.FeedChannelFeedLink;
                this.FeedChannelAuthorName = feedItem.FeedChannelAuthorName;
                this.FeedChannelLink = feedItem.FeedChannelLink;
                this.FeedChannelTitle = feedItem.FeedChannelTitle;
                this.Link = feedItem.Link;
                this.Title = feedItem.Title;
                this.PublishedAt = feedItem.PublishedAt;
                this.Images = feedItem.Images;
                this.AccentColor = feedItem.AccentColor;
            }

            private bool _IsMarked = false;
            public bool IsMarked
            {
                get { return _IsMarked; }
                set
                {
                    if (_IsMarked == value) return;
                    _IsMarked = value;
                    NotifyPropertyChanged(m => IsMarked);
                }
            }            
            public ICommand MarkCommand
            {
                get
                {
                    return new DelegateCommand(() =>
                    {
                        IsMarked = !IsMarked;
                        TheViewModel.MarkableFeedItemMarked(this);
                    }
                    );
                }
            }            
        }

        #endregion

        #region Initialization and Cleanup
        /******************************
         * Initialization and Cleanup *
         ******************************/

        public ChannelsUpdatesListViewModel() { }

        public ChannelsUpdatesListViewModel(PhoneApplicationFrame app, INavigator navigator, IZchMatomeService service, FeedDataContext dataContext, string title)
        {
            MarkableFeedItem.TheViewModel = this;
            RegisterToReceiveMessages(Constants.MessageTokens.FeedGroupsUpdated, OnFeedGroupsOrChannelsUpdated);
            RegisterToReceiveMessages(Constants.MessageTokens.FeedChannelsUpdated, OnFeedGroupsOrChannelsUpdated);
            RegisterToReceiveMessages<FavoriteUpdatedEventData>(Constants.MessageTokens.FavoritesUpdated, OnFavoritesUpdated);
            this.app = app;
            this.navigator = navigator;
            this.service = service;
            this.dataContext = dataContext;
            this.title = title;
            this.isFavorites = false;
            this.channelIds = null;
        }

        #endregion

        #region Notifications
        /*****************
         * Notifications *
         *****************/

        public event EventHandler<NotificationEventArgs<Exception>> ErrorNotice;

        private void OnFeedGroupsOrChannelsUpdated(object sender, NotificationEventArgs e)
        {
            read = false;
        }

        private void OnFavoritesUpdated(object sender, NotificationEventArgs<FavoriteUpdatedEventData> e)
        {
            if (!isFavorites)
            {
                switch (e.Data.Event)
                {
                    case FavoriteUpdatedEventData.FavoriteUpdatedEvent.Delete:
                        {
                            try
                            {
                                MarkableFeedItem theItem =
                                    (
                                        from item in FeedItems
                                        where item.Id == e.Data.FeedItem.Id
                                        select item
                                    ).Single();
                                theItem.IsMarked = false;
                            }
                            catch
                            {
                            }
                            break;
                        }
                    case FavoriteUpdatedEventData.FavoriteUpdatedEvent.DeleteAll:
                        {
                            for (int i = 0; i < FeedItems.Count; i++)
                            {
                                FeedItems[i].IsMarked = false;
                            }
                            break;
                        }
                }
            }
            else
            {
                switch (e.Data.Event)
                {
                    case FavoriteUpdatedEventData.FavoriteUpdatedEvent.Add:
                        {
                            HasNextPage = false;
                            FeedItems.Insert(0, e.Data.FeedItem);
                            if (FeedItems.Count < 3)
                            {
                                LoadFeedItems(false, true);
                            }
                            HasMember = true;
                            break;
                        }
                    case FavoriteUpdatedEventData.FavoriteUpdatedEvent.Delete:
                        {
                            FeedItems.Remove(e.Data.FeedItem);
                            if (FeedItems.Count == 0)
                            {
                                HasNextPage = false;
                                HasMember = false;
                            }
                            else if (FeedItems.Count < 3)
                            {
                                LoadFeedItems(false, true);
                            }
                            break;
                        }
                    case FavoriteUpdatedEventData.FavoriteUpdatedEvent.DeleteAll:
                        {
                            HasMember = false;
                            FeedItems.Clear();
                            break;
                        }
                }
            }
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
        string title;
        bool isFavorites;
        int[] channelIds;

        #endregion

        #region Properties
        /**************
         * Properties *
         **************/

        private bool read = false;
        private int page = Constants.App.BasePage;
        private long loadStartTime = 0;

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

        public string Title
        {
            get { return title; }
        }

        private bool _HasMember = false;
        public bool HasMember
        {
            get { return _HasMember; }
            set
            {
                if (_HasMember == value) return;
                _HasMember = value;
                NotifyPropertyChanged(m => HasMember);
            }
        }

        private bool _HasNextPage = false;
        public bool HasNextPage
        {
            get { return _HasNextPage; }
            set
            {
                if (_HasNextPage == value) return;
                _HasNextPage = value;
                NotifyPropertyChanged(m => HasNextPage);
            }
        }

        private ObservableCollection<MarkableFeedItem> _FeedItems = new ObservableCollection<MarkableFeedItem>();
        public ObservableCollection<MarkableFeedItem> FeedItems
        {
            get { return _FeedItems; }
            set
            {
                if (_FeedItems == value) return;
                _FeedItems = value;
                NotifyPropertyChanged(m => FeedItems);
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
                        NavigationService.Navigate(new Uri("/Views/WebPage.xaml", UriKind.Relative), e.SelectedItem);
                        e.SelectedItem = null;
                    }
                }
                );
            }
        }

        public ICommand ListStretchingBottomCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    LoadFeedItems(false, true);
                }
                );
            }
        }


        public ICommand RefreshCommand
        {
            get
            {
                return new DelegateCommand(
                () =>
                {
                    LoadFeedItems(true, false);
                }
                ,
                () =>
                {
                    return !IsBusy;
                }
                );
            }
        }

        public ICommand TruncateFavoritesCommand
        {
            get
            {
                return new DelegateCommand(
                () =>
                {
                    TruncateFavorites();
                }
                ,
                () =>
                {
                    return !IsBusy;
                }
                );
            }
        }

        #endregion

        #region Methods
        /***********
         * Methods *
         ***********/

        public void SetChannelIds(int[] channelIds)
        {
            this.channelIds = channelIds;
            isFavorites = false;
            if (channelIds != null)
            {
                HasMember = channelIds.Length > 0 ? true : false;
            }
        }

        public void SetAsFavorites()
        {
            isFavorites = true;
            HasMember = service.GetAllFavorites().Count > 0 ? true : false;
        }

        public void LoadFeedItems(bool clear, bool next)
        {
            if (IsBusy) return;

            if (clear)
            {
                HasNextPage = true;
                FeedItems.Clear();
                page = Constants.App.BasePage;
                loadStartTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            }
            else
            {
                if (read)
                {
                    if (!next) return;
                    if (!HasNextPage) return;
                    page++;
                } else {
                    HasNextPage = true;
                }
            }

            IsBusy = true;

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork +=
                (s, e) =>
                {
                    if (!isFavorites)
                    {
                        service.GetFeedItems(dataContext, 0, channelIds, page, loadStartTime, LoadFeedItemsCompleted);
                    }
                    else
                    {
                        service.GetFavoritedFeedItems(dataContext, FeedItems.Count, LoadFeedItemsCompleted);
                    }
                };
            bw.RunWorkerAsync();
        }

        public void MarkableFeedItemMarked(MarkableFeedItem feedItem)
        {
            if (feedItem.IsMarked)
            {
                service.AddFavorite(feedItem.Id);
                FavoriteUpdatedEventData data = new FavoriteUpdatedEventData
                {
                    Event = FavoriteUpdatedEventData.FavoriteUpdatedEvent.Add,
                    FeedItem = feedItem,
                };
                SendMessage(Constants.MessageTokens.FavoritesUpdated, new NotificationEventArgs<FavoriteUpdatedEventData>(null, data));
            }
            else
            {
                service.DeleteFavorite(feedItem.Id);
                FavoriteUpdatedEventData data = new FavoriteUpdatedEventData
                {
                    Event = FavoriteUpdatedEventData.FavoriteUpdatedEvent.Delete,
                    FeedItem = feedItem,
                };
                SendMessage(Constants.MessageTokens.FavoritesUpdated, new NotificationEventArgs<FavoriteUpdatedEventData>(null, data));
            }
        }

        private void TruncateFavorites()
        {
            service.DeleteAllFavorites();
            FavoriteUpdatedEventData data = new FavoriteUpdatedEventData
            {
                Event = FavoriteUpdatedEventData.FavoriteUpdatedEvent.DeleteAll,
            };
            SendMessage(Constants.MessageTokens.FavoritesUpdated, new NotificationEventArgs<FavoriteUpdatedEventData>(null, data));
        }

        #endregion

        #region Completion Callbacks
        /************************
         * Completion Callbacks *
         ************************/

        protected void LoadFeedItemsCompleted(ZchMatomeService.GetFeedItemsResult result, Exception error)
        {
            if (!IsBusy)
            {
                return;
            }

            IsBusy = false;
            read = true;

            if (error != null)
            {
                NotifyError(Localization.AppResources.MainPage_Error_FailedToGetFeedItems, error);
                return;
            }

            List<int> favorites = service.GetAllFavorites();
            if (result.FeedItems != null)
            {
                foreach (var item in result.FeedItems)
                {
                    var markableItem = new MarkableFeedItem(item);
                    markableItem.IsMarked = favorites.Contains(markableItem.Id);
                    FeedItems.Add(markableItem);
                }
            }
            HasNextPage = result.HasNext;
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

        #endregion
    }
}