using ZchMatome.Data;

namespace ZchMatome.SampleData
{
    public class ChannelDetailPageSampleData
    {
        private FeedChannel _TheFeedChannel = new FeedChannel
        {
            Id = 1,
            FeedGroupId = 1,
            FeedGroup = new FeedGroup
            {
                Id = 1,
                Title = "Group Title",
                Class = 1,
                AccentColor = int.Parse("FFF09609", System.Globalization.NumberStyles.HexNumber),
            },
            FeedLink = "http://twitter.com/yamac_support",
            Link = "http://twitter.com/yamac_support",
            AuthorName = "AuthorName AuthorName AuthorName AuthorName AuthorName",
            Title = "Title Title Title Title Title Title Title Title Title Title",
            Subscription = true,
            AccentColor = int.Parse("FFF09609", System.Globalization.NumberStyles.HexNumber),
        };
        public FeedChannel TheFeedChannel
        {
            get { return _TheFeedChannel; }
        }
    }
}
