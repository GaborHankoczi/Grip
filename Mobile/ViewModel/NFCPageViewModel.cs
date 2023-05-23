using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GripMobile.Model;
using GripMobile.Service;
using Plugin.NFC;
using System.Net;

namespace GripMobile.ViewModel
{
    public partial class NFCPageViewModel: ObservableObject
    {
        [ObservableProperty]
        private string labelText;

        [ObservableProperty]
        private Color backgroundColor;

        private readonly NFCService nfcService;

        public NFCPageViewModel(NFCService nfcService)
        {
            this.nfcService = nfcService;

            BackgroundColor = Colors.Red;

            if(CrossNFC.Current.IsEnabled)
            {
                LabelText = "Kattints a gombra, majd tartsd a\nkészüléket közel a leolvasóhoz!";
            }
            else
            {
                LabelText = "Az NFC nincs bekapcsolva!";
            }

            SubscribeEvents();
        }

        public const string ALERT_TITLE = "NFC";
        public const string MIME_TYPE = "application/com.companyname.nfcsample";

        //NFCNdefTypeFormat _type;
        //bool _makeReadOnly = false;
        bool _eventsAlreadySubscribed = false;
        bool _isDeviceiOS = false;

        /// <summary>
        /// Property that tracks whether the Android device is still listening,
        /// so it can indicate that to the user.
        /// </summary>
        public bool DeviceIsListening
        {
            get => _deviceIsListening;
            set
            {
                _deviceIsListening = value;
                OnPropertyChanged(nameof(DeviceIsListening));
            }
        }
        private bool _deviceIsListening;

        private bool _nfcIsEnabled;
        public bool NfcIsEnabled
        {
            get => _nfcIsEnabled;
            set
            {
                _nfcIsEnabled = value;
                OnPropertyChanged(nameof(NfcIsEnabled));
                OnPropertyChanged(nameof(NfcIsDisabled));
            }
        }
        public bool NfcIsDisabled => !NfcIsEnabled;

        /// <summary>
        /// Subscribe to the NFC events
        /// </summary>
        void SubscribeEvents()
        {
            if (_eventsAlreadySubscribed)
                UnsubscribeEvents();

            _eventsAlreadySubscribed = true;

            CrossNFC.Current.OnMessageReceived += Current_OnMessageReceived;
            CrossNFC.Current.OnNfcStatusChanged += Current_OnNfcStatusChanged;
            CrossNFC.Current.OnTagListeningStatusChanged += Current_OnTagListeningStatusChanged;

            if (_isDeviceiOS)
                CrossNFC.Current.OniOSReadingSessionCancelled += Current_OniOSReadingSessionCancelled;
        }

        /// <summary>
        /// Unsubscribe from the NFC events
        /// </summary>
        void UnsubscribeEvents()
        {
            CrossNFC.Current.OnMessageReceived -= Current_OnMessageReceived;
            CrossNFC.Current.OnNfcStatusChanged -= Current_OnNfcStatusChanged;
            CrossNFC.Current.OnTagListeningStatusChanged -= Current_OnTagListeningStatusChanged;

            if (_isDeviceiOS)
                CrossNFC.Current.OniOSReadingSessionCancelled -= Current_OniOSReadingSessionCancelled;

            _eventsAlreadySubscribed = false;
        }

        /// <summary>
        /// Event raised when Listener Status has changed
        /// </summary>
        /// <param name="isListening"></param>
        void Current_OnTagListeningStatusChanged(bool isListening) => DeviceIsListening = isListening;

        /// <summary>
        /// Event raised when NFC Status has changed
        /// </summary>
        /// <param name="isEnabled">NFC status</param>
        void Current_OnNfcStatusChanged(bool isEnabled)
        {
            NfcIsEnabled = isEnabled;

            if (isEnabled == false)
            {
                LabelText = "Az NFC nincs bekapcsolva!";
            }
            else
            {
                LabelText = "Kattints a gombra, majd tartsd a\nkészüléket közel a leolvasóhoz!";
            }
        }

        /// <summary>
        /// Event raised when a NDEF message is received
        /// </summary>
        /// <param name="tagInfo">Received <see cref="ITagInfo"/></param>
        async void Current_OnMessageReceived(ITagInfo tagInfo)
        {
            if (tagInfo == null)
            {
                LabelText = "NFC tag nem található!";
                return;
            }

            // Customized serial number
            var identifier = tagInfo.Identifier;
            var serialNumber = NFCUtils.ByteArrayToHexString(identifier, ":");
            var title = !string.IsNullOrWhiteSpace(serialNumber) ? $"Tag [{serialNumber}]" : "Tag Info";

            if (!tagInfo.IsSupported)
            {
                LabelText = "Nem támogatott NFC tag!";
            }
            else if (tagInfo.IsEmpty)
            {
                LabelText = "A beolvasott NFC tag üres!";
            }
            else
            {
                ActiveAttendanceDTO data = new()
                {
                    Message = tagInfo.Records[0].Message,
                    Token = tagInfo.Records[1].Message
                };

                HttpStatusCode result = await nfcService.RegisterAttendance(data);

                if (result == HttpStatusCode.OK) { LabelText = "Sikeres regisztrálás"; }
                else { LabelText = "Sikertelen regisztrálás"; }

                Task.Run(() => StopListening());
            }
        }

        /// <summary>
        /// Event raised when user cancelled NFC session on iOS 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Current_OniOSReadingSessionCancelled(object sender, EventArgs e) => System.Diagnostics.Debug.WriteLine("iOS NFC Session has been cancelled");

        [RelayCommand]
        async void StartListening()
        {
            BackgroundColor = Colors.Green;
            await BeginListening();
        }

        /// <summary>
        /// Task to safely start listening for NFC Tags
        /// </summary>
        /// <returns>The task to be performed</returns>
        async Task BeginListening()
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    CrossNFC.Current.StartListening();
                });
            }
            catch (Exception ex)
            {
                LabelText = "Hiba lépett fel (BeginListening)";
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Task to safely stop listening for NFC tags
        /// </summary>
        /// <returns>The task to be performed</returns>
        async Task StopListening()
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    CrossNFC.Current.StopListening();
                    UnsubscribeEvents();
                    BackgroundColor = Colors.Red;
                });
            }
            catch (Exception ex)
            {
                LabelText = "Hiba lépett fel (StopListening)";
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}
