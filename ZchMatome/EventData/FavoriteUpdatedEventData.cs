using SimpleMvvmToolkit;
using ZchMatome.ViewModels;

namespace ZchMatome
{
    public class FavoriteUpdatedEventData
    {
        public enum FavoriteUpdatedEvent
        {
            None,
            Add,
            Delete,
            DeleteAll
        }
        public FavoriteUpdatedEvent Event { get; set; }
        public ChannelsUpdatesListViewModel.MarkableFeedItem FeedItem { get; set; }
    }
}
