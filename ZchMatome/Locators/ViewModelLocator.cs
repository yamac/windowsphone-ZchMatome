using Microsoft.Phone.Controls;
using SimpleMvvmToolkit;
using ZchMatome.Services;
using ZchMatome.ViewModels;
using ZchMatome.Data;

namespace ZchMatome.Locators
{
    public class ViewModelLocator
    {
        private PhoneApplicationFrame TheApp
        {
            get
            {
                return App.Current.RootVisual as PhoneApplicationFrame;
            }
        }

        private INavigator _TheNavigator;
        private INavigator TheNavigator
        {
            get
            {
                if (_TheNavigator == null)
                {
                    _TheNavigator = new Navigator();
                }
                return _TheNavigator;
            }
        }

        private static IZchMatomeService _TheZchMatomeService;
        private IZchMatomeService TheZchMatomeService
        {
            get
            {
                if (_TheZchMatomeService == null)
                {
                    _TheZchMatomeService = new ZchMatomeService();
                }
                return _TheZchMatomeService;
            }
        }

        private static FeedDataContext _TheFeedDataContext;
        private FeedDataContext TheFeedDataContext
        {
            get
            {
                if (_TheFeedDataContext == null)
                {
                    _TheFeedDataContext = new FeedDataContext();
                }
                return _TheFeedDataContext;
            }
        }

        public MainPageViewModel MainPageViewModel
        {
            get
            {
                return new MainPageViewModel(TheApp, TheNavigator, TheZchMatomeService, TheFeedDataContext);
            }
        }

        public WebPageViewModel WebPageViewModel
        {
            get
            {
                return new WebPageViewModel();
            }
        }

        public GroupAndChannelListPageViewModel GroupAndChannelListPageViewModel
        {
            get
            {
                return new GroupAndChannelListPageViewModel(TheApp, TheNavigator, TheZchMatomeService, TheFeedDataContext);
            }
        }

        public GroupDetailPageViewModel GroupDetailPageViewModel
        {
            get
            {
                return new GroupDetailPageViewModel(TheNavigator, TheFeedDataContext);
            }
        }

        public ChannelDetailPageViewModel ChannelDetailPageViewModel
        {
            get
            {
                return new ChannelDetailPageViewModel(TheApp, TheNavigator, TheZchMatomeService, TheFeedDataContext);
            }
        }

        public PreferencesPageViewModel PreferencesPageViewModel
        {
            get
            {
                return new PreferencesPageViewModel(TheApp, TheNavigator, TheZchMatomeService, TheFeedDataContext);
            }
        }

        public TapAndSendPageViewModel TapAndSendPageViewModel
        {
            get
            {
                return new TapAndSendPageViewModel(TheNavigator);
            }
        }
    }
}