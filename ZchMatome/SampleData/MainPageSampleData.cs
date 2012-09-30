using System.Collections.ObjectModel;
using ZchMatome.Data;

namespace ZchMatome.SampleData
{
    public class MainPageSampleData
    {
        public bool IsBusy
        {
            get { return false; }
        }

        private SampleChannelsUpdatesListViewModel _SubscriptionChannelsUpdatesListViewModel;
        public SampleChannelsUpdatesListViewModel SubscriptionChannelsUpdatesListViewModel
        {
            get
            {
                if (_SubscriptionChannelsUpdatesListViewModel == null)
                {
                    _SubscriptionChannelsUpdatesListViewModel = new SampleChannelsUpdatesListViewModel();
                }
                return _SubscriptionChannelsUpdatesListViewModel;
            }
        }

        public class SampleChannelsUpdatesListViewModel
        {
            public SampleChannelsUpdatesListViewModel()
            {
            }

            public bool IsBusy
            {
                get { return false; }
            }

            public bool IsOshimem
            {
                get { return false; }
            }

            public string Title
            {
                get { return "Title"; }
            }

            public bool HasMember
            {
                get { return true; }
            }

            public bool HasNextPage
            {
                get { return false; }
            }

            private ObservableCollection<FeedItem> _FeedItems;
            public ObservableCollection<FeedItem> FeedItems
            {
                get
                {
                    if (_FeedItems == null)
                    {
                        _FeedItems = new ObservableCollection<FeedItem>
                        {
                            new FeedItem
                            {
                                Id = 1,
                                FeedGroupId = 1,
                                FeedGroupTitle = "FeedGroupTitle",
                                FeedGroupClass = 1,
                                FeedGroupAccentColor = int.Parse("FFF09609", System.Globalization.NumberStyles.HexNumber),
                                FeedChannelId = 1,
                                FeedChannelFeedLink = "http://twitter.com/yamac_support",
                                FeedChannelAuthorName = "AuthorName AuthorName AuthorName AuthorName AuthorName",
                                FeedChannelLink = "http://twitter.com/yamac_support",
                                FeedChannelTitle = "Channel Title Channel Title Channel Title",
                                Link = "http://twitter.com/yamac_support",
                                PublishedAt = new System.DateTime(2012, 1, 1),
                                Title = "Title Title Title Title Title Title Title Title Title Title",
                                AccentColor = int.Parse("FFF09609", System.Globalization.NumberStyles.HexNumber),
                                Images = new string[]{ "http://a0.twimg.com/profile_images/1382977966/yamac04.jpg" },
                            }
                            ,
                            new FeedItem
                            {
                                Id = 1,
                                FeedGroupId = 1,
                                FeedGroupTitle = "FeedGroupTitle",
                                FeedGroupClass = 1,
                                FeedGroupAccentColor = int.Parse("FFF09609", System.Globalization.NumberStyles.HexNumber),
                                FeedChannelId = 1,
                                FeedChannelFeedLink = "http://twitter.com/yamac_support",
                                FeedChannelAuthorName = "AuthorName AuthorName AuthorName AuthorName AuthorName",
                                FeedChannelLink = "http://twitter.com/yamac_support",
                                FeedChannelTitle = "Channel Title Channel Title Channel Title",
                                Link = "http://twitter.com/yamac_support",
                                PublishedAt = new System.DateTime(2012, 1, 1),
                                Title = "Title Title Title Title Title Title Title Title Title Title",
                                AccentColor = int.Parse("FFF09609", System.Globalization.NumberStyles.HexNumber),
                            }
                        };
                    }
                    return _FeedItems;
                }
            }
        }
    }
}
