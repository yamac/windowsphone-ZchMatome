using Coding4Fun.Phone.Controls;
using SimpleMvvmToolkit;
using System;
using System.Windows;
using System.Windows.Input;
using Windows.Networking.Proximity;
using ZchMatome.Navigation;

namespace ZchMatome.ViewModels
{
    public class TapAndSendPageViewModel : ViewModelBase<WebPageViewModel>
    {
        #region Initialization and Cleanup
        /******************************
         * Initialization and Cleanup *
         ******************************/

        public TapAndSendPageViewModel(INavigator navigator)
        {
            this.navigator = navigator;
            var uri = (Uri)NavigationService.NavigationArgs;
            WebPageUri = uri;
            StartPublishing();
        }

        #endregion

        #region Notifications
        /*****************
         * Notifications *
         *****************/

        public event EventHandler<NotificationEventArgs<Exception>> ErrorNotice;

        #endregion

        #region Services
        /************
         * Services *
         ************/
        INavigator navigator;

        #endregion

        #region Properties
        /**************
         * Properties *
         **************/

        private ProximityDevice CurrentDevice;
        private long MessageId;
        private bool IsFinished;

        private Uri _WebPageUri;
        public Uri WebPageUri
        {
            get { return _WebPageUri; }
            set
            {
                if (_WebPageUri == value) return;
                _WebPageUri = value;
                NotifyPropertyChanged(m => WebPageUri);
            }
        }

        #endregion

        #region Commands
        /************
         * Commands *
         ************/

        public ICommand CancelCommand
        {
            get
            {
                return new DelegateCommand(
                () =>
                {
                    StopPublishing();
                    NavigationService.GoBack();
                }
                );
            }
        }

        public ICommand BackKeyPressCommand
        {
            get
            {
                return new DelegateCommand(
                () =>
                {
                    StopPublishing();
                }
                );
            }
        }

        #endregion

        #region Methods
        /***********
         * Methods *
         ***********/

        private void StartPublishing()
        {
            try
            {
                CurrentDevice = ProximityDevice.GetDefault();
            }
            catch (UnauthorizedAccessException)
            {
                CurrentDevice = null;
            }
            MessageId = -1;
            IsFinished = false;
            if (CurrentDevice != null)
            {
                MessageId = CurrentDevice.PublishUriMessage(WebPageUri, PublishMessage_MessageTransmitted);
            }
            else
            {
                NotifyError(Localization.AppResources.TapAndSend_Error_UnknownError, null);
                NavigationService.GoBack();
            }
        }

        private void StopPublishing()
        {
            if (CurrentDevice != null && MessageId != -1)
            {
                CurrentDevice.StopPublishingMessage(MessageId);
                MessageId = -1;
            }
        }

        #endregion

        #region Completion Callbacks
        /************************
         * Completion Callbacks *
         ************************/

        private void PublishMessage_MessageTransmitted(ProximityDevice sender, long messageId)
        {
            if (!IsFinished && messageId == MessageId)
            {
                CurrentDevice.StopPublishingMessage(messageId);
                IsFinished = true;
                NavigationService.GoBack();
            }
        }

        #endregion

        #region Helpers
        /***********
         * Helpers *
         ***********/

        private void NotifyError(string message, Exception error)
        {
            Notify(ErrorNotice, new NotificationEventArgs<Exception>(message, error));
        }

        #endregion
    }
}
