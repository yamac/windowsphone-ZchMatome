using System.Collections.ObjectModel;
using ZchMatome.Data;

namespace ZchMatome.SampleData
{
    public class GroupListSampleData
    {
        private ObservableCollection<FeedGroup> _FeedGroups;
        public ObservableCollection<FeedGroup> FeedGroups
        {
            get
            {
                if (_FeedGroups == null)
                {
                    _FeedGroups = new ObservableCollection<FeedGroup>
                    {
                        new FeedGroup
                        {
                            Id = 1,
                            Title = "Group1 Title",
                            Class = 1,
                            AccentColor = int.Parse("FFF09609", System.Globalization.NumberStyles.HexNumber),
                            Subscription = true,
                        },
                        new FeedGroup
                        {
                            Id = 2,
                            Title = "Group2 Title",
                            Class = 1,
                            AccentColor = int.Parse("FFF09609", System.Globalization.NumberStyles.HexNumber),
                            Subscription = true,
                        }
                    };
                }
                return _FeedGroups;
            }
        }
    }
}
