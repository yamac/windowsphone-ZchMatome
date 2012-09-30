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
using System.ComponentModel;

namespace ZchMatome.ViewModels
{
    public class ChannelListViewModel : ViewModelBase<ChannelListViewModel>
    {
        #region Initialization and Cleanup
        /******************************
         * Initialization and Cleanup *
         ******************************/

        public ChannelListViewModel() { }

        public ChannelListViewModel(PhoneApplicationFrame app, INavigator navigator, Services.IZchMatomeService service, FeedDataContext dataContext)
        {
            RegisterToReceiveMessages(Constants.MessageTokens.FeedGroupsUpdated, OnFeedGroupsUpdated);
            this.app = app;
            this.navigator = navigator;
            this.service = service;
            this.dataContext = dataContext;
            LoadFeedChannels();
        }

        #endregion

        #region Notifications
        /*****************
         * Notifications *
         *****************/

        public event EventHandler<NotificationEventArgs<Exception>> ErrorNotice;

        private void OnFeedGroupsUpdated(object sender, NotificationEventArgs e)
        {
            LoadFeedChannels();
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

        private ObservableCollection<PublicGrouping<FeedGroup, FeedChannel>> _FeedChannels;
        public ObservableCollection<PublicGrouping<FeedGroup, FeedChannel>> FeedChannels
        {
            get { return _FeedChannels; }
            set
            {
                if (_FeedChannels == value) return;
                _FeedChannels = value;
                NotifyPropertyChanged(m => FeedChannels);
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
                        NavigationService.Navigate(new Uri("/Views/ChannelDetailPage.xaml", UriKind.Relative), e.SelectedItem);
                        e.SelectedItem = null;
                    }
                }
                );
            }
        }

        public ICommand TapFavoriteCommand
        {
            get
            {
                return new DelegateCommand<FeedItem>((e) =>
                {
                    System.Windows.MessageBox.Show("Tap");
                }
                );
            }
        }

        #endregion

        #region Methods
        /***********
         * Methods *
         ***********/

        private void LoadFeedChannels()
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork +=
                (s, e) =>
                {
                    IsBusy = true;

                    lock (dataContext)
                    {
                        var groupedChannels =
                            from channel in dataContext.FeedChannels
                            where channel.FeedGroup.Class == 1 && channel.FeedGroup.Subscription == true && channel.FeedGroup.Status == 1 && channel.Status == 1
                            orderby
                                channel.Subscription == true && channel.Notification == true descending,
                                channel.Subscription descending,
                                channel.Notification descending,
                                channel.AuthorName ascending
                            group channel by channel.FeedGroup into grouping
                            select new PublicGrouping<FeedGroup, FeedChannel>(grouping);
                        FeedChannels = new ObservableCollection<PublicGrouping<FeedGroup, FeedChannel>>(groupedChannels);
                    };

                    IsBusy = false;
                };
            bw.RunWorkerAsync();
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
