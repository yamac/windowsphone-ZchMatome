using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using ZchMatome.Data;
using Microsoft.Phone.Reactive;
using ICSharpCode.SharpZipLib.GZip;
using Microsoft.Phone.Notification;
using System.Runtime.Serialization;
using System.Text;
using System.Collections.Generic;
using Microsoft.Phone.Shell;

namespace ZchMatome.Services
{
    public class ZchMatomeService : IZchMatomeService
    {
        private static class API
        {
#if DEBUG
            private const string Base = "http://apid.yamac.net/zch_matome/v1.0/";
            private const string SecureBase = "https://secure.yamac.net/apid/zch_matome/v1.0/";
#else
            private const string Base = "http://api.yamac.net/zch_matome/v1.0/";
            private const string SecureBase = "https://secure.yamac.net/api/zch_matome/v1.0/";
#endif
            private const string FeedBase = Base + "feed/";
            private const string DeviceBase = SecureBase + "device/";
            private const string DeviceBaseFallback = Base + "device/";
            public const string FeedGroups = FeedBase + "groups";
            public const string FeedChannels = FeedBase + "channels";
            public const string FeedItems = FeedBase + "items";
            public const string DeviceRegister = DeviceBase + "register";
            public const string DeviceUnregister = DeviceBase + "unregister";
            public const string DeviceUpdate = DeviceBase + "update";
            public const string DeviceRegisterFallback = DeviceBaseFallback + "register";
            public const string DeviceUnregisterFallback = DeviceBaseFallback + "unregister";
            public const string DeviceUpdateFallback = DeviceBaseFallback + "update";
        }

        private static class Notification
        {
            public const string ChannelName = "ChannelUpdates";
        }

        public ZchMatomeService()
        {
        }

        public void GetAllFeedGroupsAndChannels(FeedDataContext dataContext, Action<Exception> callback)
        {
            System.Diagnostics.Debug.WriteLine("GetAllFeedGroupsAndChannels:" + API.FeedGroups);
            var groupsReq = WebRequest.CreateHttp(API.FeedGroups);
            groupsReq.UserAgent = Constants.Net.UserAgent;
            groupsReq.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
            System.Diagnostics.Debug.WriteLine("GetAllFeedGroupsAndChannels:" + API.FeedChannels);
            var channelsReq = WebRequest.CreateHttp(API.FeedChannels);
            channelsReq.UserAgent = Constants.Net.UserAgent;
            channelsReq.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";

            var groupsObs =
                Observable
                .FromAsyncPattern<WebResponse>(groupsReq.BeginGetResponse, groupsReq.EndGetResponse)()
                .Select
                (
                    res =>
                    {
                        // ストリームを取得
                        Stream stream = res.GetResponseStream();
                        if (string.Equals("gzip", res.Headers[HttpRequestHeader.ContentEncoding], StringComparison.OrdinalIgnoreCase))
                        {
                            stream = new GZipInputStream(stream);
                        }

                        // シリアライズ
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(FeedGroup[]));
                        var groups = (FeedGroup[])serializer.ReadObject(stream);

                        // データ更新
                        lock (dataContext)
                        {
                            var existsGroupIds = (from theGroup in groups select theGroup.Id).Intersect(from theGroup in dataContext.FeedGroups select theGroup.Id);
                            var existsGroups = from theGroup in groups from existsGroupId in existsGroupIds where theGroup.Id == existsGroupId select theGroup;
                            foreach (var existsGroup in existsGroups)
                            {
                                System.Diagnostics.Debug.WriteLine("既存グループ:" + existsGroup.Id + "," + existsGroup.Title);
                                var oldGroup = dataContext.FeedGroups.Single(theGroup => theGroup.Id == existsGroup.Id);
                                oldGroup.OrderNum = existsGroup.OrderNum;
                                oldGroup.Title = existsGroup.Title;
                                oldGroup.Class = existsGroup.Class;
                                oldGroup.AccentColor = existsGroup.AccentColor;
                                oldGroup.Status = existsGroup.Status;
                            }

                            var newGroupIds = (from theGroup in groups select theGroup.Id).Except(from theGroup in dataContext.FeedGroups select theGroup.Id);
                            var newGroups = from theGroup in groups from newGroupId in newGroupIds where theGroup.Id == newGroupId select theGroup;
                            foreach (var newGroup in newGroups)
                            {
                                System.Diagnostics.Debug.WriteLine("新規グループ:" + newGroup.Id + "," + newGroup.Title);
                                newGroup.Subscription = true;
                                dataContext.FeedGroups.InsertOnSubmit(newGroup);
                            }
                            dataContext.SubmitChanges();
                        };

                        // ストリームを閉じる
                        stream.Close();

                        return 1;
                    }
                );
                
            var channelsObs =
                Observable
                .FromAsyncPattern<WebResponse>(channelsReq.BeginGetResponse, channelsReq.EndGetResponse)()
                .Select
                (
                    res =>
                    {
                        // ストリームを取得
                        Stream stream = res.GetResponseStream();
                        if (string.Equals("gzip", res.Headers[HttpRequestHeader.ContentEncoding], StringComparison.OrdinalIgnoreCase))
                        {
                            stream = new GZipInputStream(stream);
                        }

                        // シリアライズ
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(FeedChannel[]));
                        var channels = (FeedChannel[])serializer.ReadObject(stream);

                        // データ更新
                        lock (dataContext)
                        {
                            var existsChannelIds = (from channel in channels select channel.Id).Intersect(from channel in dataContext.FeedChannels select channel.Id);
                            var existsChannels = from channel in channels from existsChannelId in existsChannelIds where channel.Id == existsChannelId select channel;
                            foreach (var existsChannel in existsChannels)
                            {
                                System.Diagnostics.Debug.WriteLine("既存チャンネル:" + existsChannel.Id + "," + existsChannel.Title);
                                var oldChannel = dataContext.FeedChannels.Single(channel => channel.Id == existsChannel.Id);
                                oldChannel.OrderNum = existsChannel.OrderNum;
                                oldChannel.FeedGroupId = existsChannel.FeedGroupId;
                                oldChannel.FeedLink = existsChannel.FeedLink;
                                oldChannel.AuthorName = existsChannel.AuthorName;
                                oldChannel.Link = existsChannel.Link;
                                oldChannel.Title = existsChannel.Title;
                                oldChannel.Description = existsChannel.Description;
                                oldChannel.AccentColor = existsChannel.AccentColor;
                                oldChannel.Status = existsChannel.Status;
                            }

                            var newChannelIds = (from channel in channels select channel.Id).Except(from channel in dataContext.FeedChannels select channel.Id);
                            var newChannels = from channel in channels from newChannelId in newChannelIds where channel.Id == newChannelId select channel;
                            foreach (var newChannel in newChannels)
                            {
                                System.Diagnostics.Debug.WriteLine("新規チャンネル:" + newChannel.Id + "," + newChannel.Title);
                                newChannel.Subscription = true;
                                newChannel.Notification = false;
                                dataContext.FeedChannels.InsertOnSubmit(newChannel);
                            }
                            dataContext.SubmitChanges();
                        };

                        // ストリームを閉じる
                        stream.Close();

                        // 結果
                        return 1;
                    }
                );
            
            groupsObs.SelectMany(a => channelsObs)
            .Subscribe(
                _ =>
                {
                    // サブミット
                    callback(null);
                }
                ,
                e =>
                {
                    callback(e);
                }
            );
        }

        public class GetFeedItemsResult
        {
            public FeedItem[] FeedItems { get; private set; }
            public bool HasNext { get; private set; }

            public GetFeedItemsResult(FeedItem[] items, bool hasNext)
            {
                FeedItems = items;
                HasNext = hasNext;
            }
        }

        public void GetFeedItems(FeedDataContext dataContext, int classId, int[] channelIds, int page, long baseTime, Action<GetFeedItemsResult, Exception> callback)
        {
            string uri = API.FeedItems;
            uri += "?" + "class=" + classId;
            if (channelIds != null)
            {
                uri += "&" + "channel_id=" + string.Join(",", channelIds);
            }
            uri += "&" + "rows=" + Constants.App.ItemsPerPage + "&page=" + page + "&t=" + baseTime;
            System.Diagnostics.Debug.WriteLine("GetFeedItems:" + uri);
            var req = WebRequest.CreateHttp(uri);
            req.UserAgent = Constants.Net.UserAgent;
            req.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";

            Observable
            .FromAsyncPattern<WebResponse>(req.BeginGetResponse, req.EndGetResponse)
            .Invoke()
            .Select<WebResponse, GetFeedItemsResult>(res =>
            {
                // ストリームを取得
                Stream stream = res.GetResponseStream();
                if (string.Equals("gzip", res.Headers[HttpRequestHeader.ContentEncoding], StringComparison.OrdinalIgnoreCase))
                {
                    stream = new GZipInputStream(stream);
                }

                // シリアライズ
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(FeedItem[]));

                // データ取得
                var items = (FeedItem[])serializer.ReadObject(stream);
                foreach (var item in items)
                {
                    lock (dataContext)
                    {
                        var itemChannel = dataContext.FeedChannels.Single(channel => channel.Id == item.FeedChannelId);
                        item.AccentColor = itemChannel.AccentColor;
                    }
                }

                // ストリームを閉じる
                stream.Close();

                // 結果
                var result = new GetFeedItemsResult(items, (items.Count() == Constants.App.ItemsPerPage) && (page < Constants.App.MaxPage));

                return result;
            }
            )
            .ObserveOnDispatcher()
            .Subscribe(
                s =>
                {
                    callback(s, null);
                },
                e =>
                {
                    callback(null, e);
                }
            );
        }

        public void GetFavoritedFeedItems(FeedDataContext dataContext, int start, Action<ZchMatomeService.GetFeedItemsResult, Exception> callback)
        {
            string uri = API.FeedItems;
            List<int> favorites = GetAllFavorites();
            int count = favorites.Count - start;
            if (count > Constants.App.ItemsPerPage)
            {
                count = Constants.App.ItemsPerPage;
            }
            if (count == 0)
            {
                var result = new GetFeedItemsResult(null, false);
                callback(result, null);
                return;
            }
            List<int> itemIds = favorites.GetRange(start, count);
            uri += "?" + "item_id=" + string.Join(",", itemIds);
            System.Diagnostics.Debug.WriteLine("GetFavoritedFeedItems:" + uri);
            var req = WebRequest.CreateHttp(uri);
            req.UserAgent = Constants.Net.UserAgent;
            req.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";

            Observable
            .FromAsyncPattern<WebResponse>(req.BeginGetResponse, req.EndGetResponse)
            .Invoke()
            .Select<WebResponse, GetFeedItemsResult>(res =>
            {
                // ストリームを取得
                Stream stream = res.GetResponseStream();
                if (string.Equals("gzip", res.Headers[HttpRequestHeader.ContentEncoding], StringComparison.OrdinalIgnoreCase))
                {
                    stream = new GZipInputStream(stream);
                }

                // シリアライズ
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(FeedItem[]));

                // データ取得
                var items = (FeedItem[])serializer.ReadObject(stream);
                foreach (var item in items)
                {
                    lock (dataContext)
                    {
                        var itemChannel = dataContext.FeedChannels.Single(channel => channel.Id == item.FeedChannelId);
                        item.AccentColor = itemChannel.AccentColor;
                    }
                }

                // ストリームを閉じる
                stream.Close();

                // 結果
                var result = new GetFeedItemsResult(items, (items.Count() == Constants.App.ItemsPerPage));

                return result;
            }
            )
            .ObserveOnDispatcher()
            .Subscribe(
                s =>
                {
                    callback(s, null);
                },
                e =>
                {
                    callback(null, e);
                }
            );
        }

        List<int> FavoriteFeedItemIds;

        private void InitFavorites()
        {
            if (FavoriteFeedItemIds == null)
            {
                FavoriteFeedItemIds = Helpers.AppSettings.GetValueOrDefault<List<int>>(Constants.AppKey.FavoriteFeedItemIds, null);
                if (FavoriteFeedItemIds == null)
                {
                    FavoriteFeedItemIds = new List<int>();
                }
            }
        }

        private void SaveFavorites()
        {
            Helpers.AppSettings.AddOrUpdateValue(Constants.AppKey.FavoriteFeedItemIds, FavoriteFeedItemIds);
        }

        public List<int> GetAllFavorites()
        {
            InitFavorites();
            return FavoriteFeedItemIds;
        }

        public void AddFavorite(int id)
        {
            InitFavorites();
            FavoriteFeedItemIds.Insert(0, id);
            SaveFavorites();
        }

        public void DeleteFavorite(int id)
        {
            InitFavorites();
            FavoriteFeedItemIds.Remove(id);
            SaveFavorites();
        }

        public void DeleteAllFavorites()
        {
            InitFavorites();
            FavoriteFeedItemIds.Clear();
            SaveFavorites();
        }

        [DataContract]
        public class RegisterNotificationChannelResult
        {
            [DataContract]
            public class _Response
            {
                [DataMember(Name = "uuid")]
                public string Uuid { get; set; }
            }

            [DataMember(Name = "status_code")]
            public long StatusCode { get; set; }

            [DataMember(Name = "status_name")]
            public string StatusName { get; set; }

            [DataMember(Name = "response")]
            public _Response Response { get; set; }
        }

        public void RegisterNotificationChannel(string version, string langCode, Action<RegisterNotificationChannelResult, Exception> callback)
        {
            System.Diagnostics.Debug.WriteLine("RegisterNotificationChannel");
            bool isNewChannel = false;

            HttpNotificationChannel notificationChannel;
            notificationChannel = HttpNotificationChannel.Find(Notification.ChannelName);
            if (notificationChannel != null)
            {
                notificationChannel.Close();
            }

            isNewChannel = true;
            notificationChannel = new HttpNotificationChannel(Notification.ChannelName);

            notificationChannel.ConnectionStatusChanged += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine("NotificationChannel_ConnectionStatusChanged:" + e.ConnectionStatus.ToString());
            };

            notificationChannel.ErrorOccurred += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine("NotificationChannel_ErrorOccurred:" + e.Message.ToString());
            };

            notificationChannel.HttpNotificationReceived += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine("NotificationChannel_HttpNotificationReceived:" + e.Notification.ToString());
            };

            notificationChannel.ChannelUriUpdated += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine("NotificationChannel_ChannelUriUpdated:" + e.ChannelUri.ToString());
                string uri = isNewChannel ? API.DeviceRegister : API.DeviceUpdate;
                System.Diagnostics.Debug.WriteLine(uri);
                string postDataStr;
                postDataStr = "mpns_channel_url=" + HttpUtility.UrlEncode(e.ChannelUri.ToString());
                if (version != null)
                {
                    postDataStr += "&version=" + version;
                }
                if (langCode != null)
                {
                    postDataStr += "&language_code=" + langCode;
                }
                var req = WebRequest.CreateHttp(uri);
                req.UserAgent = Constants.Net.UserAgent;
                req.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";

                Observable
                .FromAsyncPattern<Stream>(req.BeginGetRequestStream, req.EndGetRequestStream)
                .Invoke()
                .SelectMany
                (
                    stream =>
                    {
                        // POSTデータ
                        var postData = Encoding.UTF8.GetBytes(postDataStr);

                        // 書き込み
                        stream.Write(postData, 0, postData.Length);

                        // ストリームを閉じる
                        stream.Close();

                        // 連結
                        return
                            Observable
                            .FromAsyncPattern<WebResponse>(req.BeginGetResponse, req.EndGetResponse)
                            .Invoke();
                    }
                )
                .Select<WebResponse, RegisterNotificationChannelResult>
                (
                    res =>
                    {
                        // ストリームを取得
                        Stream stream = res.GetResponseStream();
                        if (string.Equals("gzip", res.Headers[HttpRequestHeader.ContentEncoding], StringComparison.OrdinalIgnoreCase))
                        {
                            stream = new GZipInputStream(stream);
                        }

                        // シリアライズ
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(RegisterNotificationChannelResult));
                        var result = (RegisterNotificationChannelResult)serializer.ReadObject(stream);

                        // ストリームを閉じる
                        stream.Close();

                        // 結果
                        return result;
                    }
                )
                .ObserveOnDispatcher()
                .Subscribe
                (
                    s2 =>
                    {
                        callback(s2, null);
                    },
                    e2 =>
                    {
                        uri = isNewChannel ? API.DeviceRegisterFallback : API.DeviceUpdateFallback;
                        System.Diagnostics.Debug.WriteLine(uri);
                        req = WebRequest.CreateHttp(uri);
                        req.UserAgent = Constants.Net.UserAgent;
                        req.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
                        req.Method = "POST";
                        req.ContentType = "application/x-www-form-urlencoded";

                        Observable
                        .FromAsyncPattern<Stream>(req.BeginGetRequestStream, req.EndGetRequestStream)
                        .Invoke()
                        .SelectMany
                        (
                            stream =>
                            {
                                // POSTデータ
                                var postData = Encoding.UTF8.GetBytes(postDataStr);

                                // 書き込み
                                stream.Write(postData, 0, postData.Length);

                                // ストリームを閉じる
                                stream.Close();

                                // 連結
                                return
                                    Observable
                                    .FromAsyncPattern<WebResponse>(req.BeginGetResponse, req.EndGetResponse)
                                    .Invoke();
                            }
                        )
                        .Select<WebResponse, RegisterNotificationChannelResult>
                        (
                            res =>
                            {
                                // ストリームを取得
                                Stream stream = res.GetResponseStream();
                                if (string.Equals("gzip", res.Headers[HttpRequestHeader.ContentEncoding], StringComparison.OrdinalIgnoreCase))
                                {
                                    stream = new GZipInputStream(stream);
                                }

                                // シリアライズ
                                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(RegisterNotificationChannelResult));
                                var result = (RegisterNotificationChannelResult)serializer.ReadObject(stream);

                                // ストリームを閉じる
                                stream.Close();

                                // 結果
                                return result;
                            }
                        )
                        .ObserveOnDispatcher()
                        .Subscribe
                        (
                            s3 =>
                            {
                                callback(s3, null);
                            },
                            e3 =>
                            {
                                notificationChannel.Close();
                                callback(null, e3);
                            }
                        );
                    }
                );
            };

            //notificationChannel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(PushChannel_ShellToastNotificationReceived);

            if (isNewChannel)
            {
                try
                {
                    notificationChannel.Open();
                    notificationChannel.BindToShellToast();
                    notificationChannel.BindToShellTile();
                }
                catch (Exception e)
                {
                    callback(null, e);
                }
            }
            else
            {
                callback(null, null);
            }
        }

        [DataContract]
        public class UnregisterNotificationChannelResult
        {
            [DataMember(Name = "status_code")]
            public long StatusCode { get; set; }

            [DataMember(Name = "status_name")]
            public string StatusName { get; set; }
        }

        public void UnregisterNotificationChannel(string uuid, Action<UnregisterNotificationChannelResult, Exception> callback)
        {
            System.Diagnostics.Debug.WriteLine("UnregisterNotificationChannel");
            HttpNotificationChannel notificationChannel;
            notificationChannel = HttpNotificationChannel.Find(Notification.ChannelName);
            if (notificationChannel != null)
            {
                notificationChannel.ConnectionStatusChanged += (sender, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("NotificationChannel_ConnectionStatusChanged:" + e.ConnectionStatus.ToString());
                };

                notificationChannel.ErrorOccurred += (sender, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("NotificationChannel_ErrorOccurred:" + e.Message.ToString());
                };

                notificationChannel.Close();
                ShellTile shellTile = ShellTile.ActiveTiles.First();
                StandardTileData shellTileData = new StandardTileData();
                shellTileData.BackBackgroundImage = null;
                shellTileData.BackContent = "";
                shellTileData.BackTitle = "";
                shellTile.Update(shellTileData);

                string uri = API.DeviceUnregister;
                System.Diagnostics.Debug.WriteLine(uri);
                string postDataStr = "uuid=" + HttpUtility.UrlEncode(uuid);
                var req = WebRequest.CreateHttp(uri);
                req.UserAgent = Constants.Net.UserAgent;
                req.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";

                Observable
                .FromAsyncPattern<Stream>(req.BeginGetRequestStream, req.EndGetRequestStream)
                .Invoke()
                .SelectMany
                (
                    stream =>
                    {
                        // POSTデータ
                        var postData = Encoding.UTF8.GetBytes(postDataStr);

                        // 書き込み
                        stream.Write(postData, 0, postData.Length);

                        // ストリームを閉じる
                        stream.Close();

                        // 連結
                        return
                            Observable
                            .FromAsyncPattern<WebResponse>(req.BeginGetResponse, req.EndGetResponse)
                            .Invoke();
                    }
                )
                .Select<WebResponse, UnregisterNotificationChannelResult>
                (
                    res =>
                    {
                        // ストリームを取得
                        Stream stream = res.GetResponseStream();
                        if (string.Equals("gzip", res.Headers[HttpRequestHeader.ContentEncoding], StringComparison.OrdinalIgnoreCase))
                        {
                            stream = new GZipInputStream(stream);
                        }

                        // シリアライズ
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(UnregisterNotificationChannelResult));
                        var result = (UnregisterNotificationChannelResult)serializer.ReadObject(stream);

                        // ストリームを閉じる
                        stream.Close();

                        // 結果
                        return result;
                    }
                )
                .ObserveOnDispatcher()
                .Subscribe(
                    s2 =>
                    {
                        callback(s2, null);
                    },
                    e2 =>
                    {
                        uri = API.DeviceUnregisterFallback;
                        System.Diagnostics.Debug.WriteLine(uri);
                        req = WebRequest.CreateHttp(uri);
                        req.UserAgent = Constants.Net.UserAgent;
                        req.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
                        req.Method = "POST";
                        req.ContentType = "application/x-www-form-urlencoded";

                        Observable
                        .FromAsyncPattern<Stream>(req.BeginGetRequestStream, req.EndGetRequestStream)
                        .Invoke()
                        .SelectMany
                        (
                            stream =>
                            {
                                // POSTデータ
                                var postData = Encoding.UTF8.GetBytes(postDataStr);

                                // 書き込み
                                stream.Write(postData, 0, postData.Length);

                                // ストリームを閉じる
                                stream.Close();

                                // 連結
                                return
                                    Observable
                                    .FromAsyncPattern<WebResponse>(req.BeginGetResponse, req.EndGetResponse)
                                    .Invoke();
                            }
                        )
                        .Select<WebResponse, UnregisterNotificationChannelResult>
                        (
                            res =>
                            {
                                // ストリームを取得
                                Stream stream = res.GetResponseStream();
                                if (string.Equals("gzip", res.Headers[HttpRequestHeader.ContentEncoding], StringComparison.OrdinalIgnoreCase))
                                {
                                    stream = new GZipInputStream(stream);
                                }

                                // シリアライズ
                                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(UnregisterNotificationChannelResult));
                                var result = (UnregisterNotificationChannelResult)serializer.ReadObject(stream);

                                // ストリームを閉じる
                                stream.Close();

                                // 結果
                                return result;
                            }
                        )
                        .ObserveOnDispatcher()
                        .Subscribe
                        (
                            s3 =>
                            {
                                callback(s3, null);
                            },
                            e3 =>
                            {
                                //callback(null, e3);
                                callback(null, null);
                            }
                        );
                    }
                );
            }
            else
            {
                callback(null, null);
            }
        }

        [DataContract]
        public class UpdateNotificationChannelResult
        {
            [DataMember(Name = "status_code")]
            public long StatusCode { get; set; }

            [DataMember(Name = "status_name")]
            public string StatusName { get; set; }
        }

        public void UpdateNotificationChannel
        (
            string uuid, string version, string langCode,
            int[] subscriptionChannelIds, int[] notificationChannelIds,
            bool resetUnreads, Action<UpdateNotificationChannelResult, Exception> callback
        )
        {
            System.Diagnostics.Debug.WriteLine("UpdateNotificationChannel");
            string uri = API.DeviceUpdate;
            System.Diagnostics.Debug.WriteLine(uri);
            string postDataStr;
            postDataStr = "uuid=" + HttpUtility.UrlEncode(uuid);
            if (version != null)
            {
                postDataStr += "&version=" + version;
            }
            if (langCode != null)
            {
                postDataStr += "&language_code=" + langCode;
            }
            if (subscriptionChannelIds != null)
            {
                postDataStr += "&subscription_channel_id=" + string.Join(",", subscriptionChannelIds);
            }
            if (notificationChannelIds != null)
            {
                postDataStr += "&notification_channel_id=" + string.Join(",", notificationChannelIds);
            }
            if (resetUnreads)
            {
                postDataStr += "&unread_count=0";
            }
            var req = WebRequest.CreateHttp(uri);
            req.UserAgent = Constants.Net.UserAgent;
            req.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            Observable
            .FromAsyncPattern<Stream>(req.BeginGetRequestStream, req.EndGetRequestStream)
            .Invoke()
            .SelectMany
            (
                stream =>
                {
                    // POSTデータ
                    var postData = Encoding.UTF8.GetBytes(postDataStr);

                    // 書き込み
                    stream.Write(postData, 0, postData.Length);

                    // ストリームを閉じる
                    stream.Close();

                    // 連結
                    return
                        Observable
                        .FromAsyncPattern<WebResponse>(req.BeginGetResponse, req.EndGetResponse)
                        .Invoke();
                }
            )
            .Select
            (
                res =>
                {
                    // ストリームを取得
                    Stream stream = res.GetResponseStream();
                    if (string.Equals("gzip", res.Headers[HttpRequestHeader.ContentEncoding], StringComparison.OrdinalIgnoreCase))
                    {
                        stream = new GZipInputStream(stream);
                    }

                    // シリアライズ
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(UpdateNotificationChannelResult));
                    var result = (UpdateNotificationChannelResult)serializer.ReadObject(stream);

                    // ストリームを閉じる
                    stream.Close();

                    // 結果
                    return result;
                }
            )
            .ObserveOnDispatcher()
            .Subscribe(
                s2 =>
                {
                    callback(s2, null);
                },
                e2 =>
                {
                    uri = API.DeviceUpdateFallback;
                    System.Diagnostics.Debug.WriteLine(uri);
                    req = WebRequest.CreateHttp(uri);
                    req.UserAgent = Constants.Net.UserAgent;
                    req.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded";

                    Observable
                    .FromAsyncPattern<Stream>(req.BeginGetRequestStream, req.EndGetRequestStream)
                    .Invoke()
                    .SelectMany
                    (
                        stream =>
                        {
                            // POSTデータ
                            var postData = Encoding.UTF8.GetBytes(postDataStr);

                            // 書き込み
                            stream.Write(postData, 0, postData.Length);

                            // ストリームを閉じる
                            stream.Close();

                            // 連結
                            return
                                Observable
                                .FromAsyncPattern<WebResponse>(req.BeginGetResponse, req.EndGetResponse)
                                .Invoke();
                        }
                    )
                    .Select
                    (
                        res =>
                        {
                            // ストリームを取得
                            Stream stream = res.GetResponseStream();
                            if (string.Equals("gzip", res.Headers[HttpRequestHeader.ContentEncoding], StringComparison.OrdinalIgnoreCase))
                            {
                                stream = new GZipInputStream(stream);
                            }

                            // シリアライズ
                            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(UpdateNotificationChannelResult));
                            var result = (UpdateNotificationChannelResult)serializer.ReadObject(stream);

                            // ストリームを閉じる
                            stream.Close();

                            // 結果
                            return result;
                        }
                    )
                    .ObserveOnDispatcher()
                    .Subscribe
                    (
                        s3 =>
                        {
                            callback(s3, null);
                        },
                        e3 =>
                        {
                            callback(null, e3);
                        }
                    );
                }
            );
        }
    }
}