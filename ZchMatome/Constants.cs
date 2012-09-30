using Microsoft.Phone.Info;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using ZchMatome.Data;

namespace ZchMatome
{
    public static class Constants
    {
        public static class App
        {
#if DEBUG
            public const int FeedChannelExpireDays = 0;
#else
            public const int FeedChannelExpireDays = 7;
#endif
            public const int ItemsPerPage = 10;
            public const int BasePage = 1;
            public const int MaxPage = 30;
        }

        public static class AppKey
        {
            public const string Version = "Version";
            public const string LastUpdate = "LastUpdate";
            public const string NotificationConfirmation = "NotificationConfirmation";
            public const string NotificationType = "NotificationType";
            public const string NotificationUuid = "NotificationUuid";
            public const string FavoriteFeedItemIds = "FavoriteFeedItemIds";
        }

        public static class MessageTokens
        {
            public const string MainPageInitializeCompleted = "MainPageInitializeCompleted";
            public const string ReloadRequested = "ReloadRequested";
            public const string GroupAndChannelListPageInitializeCompleted = "GroupAndChannelListPageInitializeCompleted";
            public const string FeedGroupsUpdated = "FeedGroupsUpdated";
            public const string FeedChannelsUpdated = "FeedChannelsUpdated";
            public const string FavoritesUpdated = "FavoritesUpdated";
            public const string NotificationUpdated = "NotificationUpdated";
        }

        public static class Net
        {
            private static string _UserAgent;
            public static string UserAgent
            {
                get
                {
                    if (_UserAgent != null)
                    {
                        return _UserAgent;
                    }
                     _UserAgent = string.Format(
                        "Mozilla/5.0 (iPhone; U; CPU iPhone OS 4_0 fake; compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0; {0}; {1})",
                        DeviceStatus.DeviceManufacturer,
                        DeviceStatus.DeviceName
                    );
                    return _UserAgent;
                }
            }
        }

        public static class Media
        {
            public static Collection<NamedSolidColorBrush> AccentColors = new Collection<NamedSolidColorBrush>
            {
                new NamedSolidColorBrush { Name = Localization.AppResources.Constants_Media_AccentColors_None, Brush = new SolidColorBrush(Color.FromArgb(0x00, 0x00, 0x00, 0x00)) },
                new NamedSolidColorBrush { Name = Localization.AppResources.Constants_Media_AccentColors_Magenta, Brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xD8, 0x00, 0x73)) },
                new NamedSolidColorBrush { Name = Localization.AppResources.Constants_Media_AccentColors_Purple, Brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xA2, 0x00, 0xFF)) },
                new NamedSolidColorBrush { Name = Localization.AppResources.Constants_Media_AccentColors_Teal, Brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0xAB, 0xA9)) },
                new NamedSolidColorBrush { Name = Localization.AppResources.Constants_Media_AccentColors_Lime, Brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xA2, 0xC1, 0x39)) },
                new NamedSolidColorBrush { Name = Localization.AppResources.Constants_Media_AccentColors_Brown, Brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xA0, 0x50, 0x00)) },
                new NamedSolidColorBrush { Name = Localization.AppResources.Constants_Media_AccentColors_Pink, Brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xE6, 0x71, 0xB8)) },
                new NamedSolidColorBrush { Name = Localization.AppResources.Constants_Media_AccentColors_Mango, Brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0x96, 0x09)) },
                new NamedSolidColorBrush { Name = Localization.AppResources.Constants_Media_AccentColors_Blue, Brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x1B, 0xA1, 0xE2)) },
                new NamedSolidColorBrush { Name = Localization.AppResources.Constants_Media_AccentColors_Red, Brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xE5, 0x14, 0x00)) },
                new NamedSolidColorBrush { Name = Localization.AppResources.Constants_Media_AccentColors_Green, Brush = new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0x99, 0x33)) },
            };

            public static readonly Dictionary<int, int> AccentColorCodeToColorIndex = new Dictionary<int, int>
            {
                { int.Parse("FFD80073", System.Globalization.NumberStyles.HexNumber), 0},
                { int.Parse("FFA200FF", System.Globalization.NumberStyles.HexNumber), 1},
                { int.Parse("FF00ABA9", System.Globalization.NumberStyles.HexNumber), 2},
                { int.Parse("FFA2C139", System.Globalization.NumberStyles.HexNumber), 3},
                { int.Parse("FFA05000", System.Globalization.NumberStyles.HexNumber), 4},
                { int.Parse("FFE671B8", System.Globalization.NumberStyles.HexNumber), 5},
                { int.Parse("FFF09609", System.Globalization.NumberStyles.HexNumber), 6},
                { int.Parse("FF1BA1E2", System.Globalization.NumberStyles.HexNumber), 7},
                { int.Parse("FFE51400", System.Globalization.NumberStyles.HexNumber), 8},
                { int.Parse("FF339933", System.Globalization.NumberStyles.HexNumber), 9},
            };

            public static readonly Dictionary<int, string> AccentColorIndexToName = new Dictionary<int,string>
            {
                { 0, "マゼンタ" },
                { 1, "パープル" },
                { 2, "ティール" },
                { 3, "ライム" },
                { 4, "ブラウン" },
                { 5, "ピンク" },
                { 6, "マンゴー" },
                { 7, "ブルー" },
                { 8, "レッド" },
                { 9, "グリーン" },
            };
        }
    }
}
