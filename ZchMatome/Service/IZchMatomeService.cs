using System;
using System.Collections.Generic;
using ZchMatome.Data;

namespace ZchMatome.Services
{
    public interface IZchMatomeService
    {
        // Feed
        void GetAllFeedGroupsAndChannels(FeedDataContext dataContext, Action<Exception> callback);
        void GetFeedItems(FeedDataContext dataContext, int classId, int[] channelIds, int page, long baseTime, Action<ZchMatomeService.GetFeedItemsResult, Exception> callback);
        void GetFavoritedFeedItems(FeedDataContext dataContext, int start, Action<ZchMatomeService.GetFeedItemsResult, Exception> callback);
        
        // Favorite
        List<int> GetAllFavorites();
        void AddFavorite(int id);
        void DeleteFavorite(int id);
        void DeleteAllFavorites();

        // Notification
        void RegisterNotificationChannel(string version, string langCode, Action<ZchMatomeService.RegisterNotificationChannelResult, Exception> callback);
        void UnregisterNotificationChannel(string uuid, Action<ZchMatomeService.UnregisterNotificationChannelResult, Exception> callback);
        void UpdateNotificationChannel
        (
            string uuid, string version, string langCode,
            int[] subscriptionChannelIds, int[] notificationChannelIds,
            bool resetUnreads, Action<ZchMatomeService.UpdateNotificationChannelResult, Exception> callback
        );
    }
}
