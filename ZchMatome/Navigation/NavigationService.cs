using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace ZchMatome.Navigation
{
    public static class NavigationService
    {
        private static object _NavigationArgs;
        public static object NavigationArgs
        {
            get { return _NavigationArgs; }
        }

        private static PhoneApplicationFrame _RootFrame;
        private static PhoneApplicationFrame RootFrame
        {
            get
            {
                if (_RootFrame != null)
                {
                    return _RootFrame;
                }
                _RootFrame = Application.Current.RootVisual as PhoneApplicationFrame;
                return _RootFrame;
            }
        }

        public static void Navigate(Uri pageUri, Object args = null)
        {
            _NavigationArgs = args;
            var dispatcher = Deployment.Current.Dispatcher;
            dispatcher.BeginInvoke(() =>
            {
                RootFrame.Navigate(pageUri);
            });
        }

        public static void GoBack()
        {
            var dispatcher = Deployment.Current.Dispatcher;
            dispatcher.BeginInvoke(() =>
            {
                RootFrame.GoBack();
            });
        }

        public static void GoForward()
        {
            var dispatcher = Deployment.Current.Dispatcher;
            dispatcher.BeginInvoke(() =>
            {
                RootFrame.GoForward();
            });
        }
    }
}
