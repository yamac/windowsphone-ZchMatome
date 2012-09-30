using ZchMatome.Data;

namespace ZchMatome.SampleData
{
    public class GroupDetailPageSampleData
    {
        private FeedGroup _TheFeedGroup = new FeedGroup
        {
            Id = 1,
            Title = "Group Title",
            Class = 1,
            AccentColor = int.Parse("FFF09609", System.Globalization.NumberStyles.HexNumber),
        };
        public FeedGroup TheFeedGroup
        {
            get { return _TheFeedGroup; }
        }
    }
}
