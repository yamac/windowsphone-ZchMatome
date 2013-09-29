using System;
using System.Windows;
using Microsoft.Phone.Controls;
using SimpleMvvmToolkit;
using Coding4Fun.Phone.Controls;
using ZchMatome.ViewModels;

namespace ZchMatome.Views
{
    public partial class TapAndSendPage : PhoneApplicationPage
    {
        public TapAndSendPage()
        {
            InitializeComponent();
            var vm = (TapAndSendPageViewModel)DataContext;
            vm.ErrorNotice += OnErrorNotice;
        }

        public void OnErrorNotice(object sender, NotificationEventArgs<Exception> e)
        {
            System.Diagnostics.Debug.WriteLine("OnErrorNotify:" + (e.Data != null ? e.Data.ToString() : "null"));
            var toast = new ToastPrompt
            {
                Message = (e.Message != null ? e.Message : null),
                TextWrapping = TextWrapping.Wrap,
            };
            toast.Show();
        }
    }
}