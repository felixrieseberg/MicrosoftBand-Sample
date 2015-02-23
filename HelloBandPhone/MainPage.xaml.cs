using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//Band
using Microsoft.Band;
using Microsoft.Band.Store;
using Microsoft.Band.Sensors;
using Microsoft.Band.Notifications;
using System.Collections.ObjectModel;

namespace HelloBandPhone
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public IBandInfo[] pairedBands;
        public IBandClient bandClient;
        public IBandHeartRateReading heartreading;

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            ConnectWithBand();

        }

        public async void ConnectWithBand()
        {
            pairedBands = await BandClientManager.Instance.GetBandsAsync();

            try
            {
                if (pairedBands.Length > 0)
                {
                    bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]);
                    bandFound_value.Text = pairedBands[0].Name;

                    // Enable buttons
                    button_getHeartRate.IsEnabled = true;
                    button_vibrate.IsEnabled = true;
                    button_getVersions.IsEnabled = true;
                }
                else
                {
                    bandFound_value.Text = "None";
                }
            }
            catch (BandException ex)
            {
                bandFound_value.Text = ex.Message;
            }

        }

        public async void GetVersions()
        {
            hwVersion_value.Text = await bandClient.GetFirmwareVersionAsync();
            fwVersion_value.Text = await bandClient.GetHardwareVersionAsync();
        }

        public async void Vibrate()
        {
            try
            {
                // send a vibration request of type alert alarm to the device
                await bandClient.NotificationManager.VibrateAsync(VibrationType.ThreeToneHigh);
            }
            catch (BandException ex)
            {
                throw ex;
            }

        }

        public async void GetHeartRate()
        {
            IEnumerable<TimeSpan> supportedHeartBeatReportingIntervals = bandClient.SensorManager.HeartRate.SupportedReportingIntervals;
            bandClient.SensorManager.HeartRate.ReportingInterval = supportedHeartBeatReportingIntervals.First<TimeSpan>();

            // hook up to the HeartRate sensor ReadingChanged event
            bandClient.SensorManager.HeartRate.ReadingChanged += async (sender, args) =>
            {
                heartreading = args.SensorReading;
                await heartrate_value.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                {
                    heartrate_value.Text = heartreading.HeartRate.ToString();
                });
            };

            try
            {
                await bandClient.SensorManager.HeartRate.StartReadingsAsync();
            }
            catch (BandException ex)
            {
                throw ex;
            }


        }

        private void Vibrate(object sender, RoutedEventArgs e)
        {
            Vibrate();
        }

        private void GetVersions(object sender, RoutedEventArgs e)
        {
            GetVersions();
        }

        private void GetHeartRate(object sender, RoutedEventArgs e)
        {
            GetHeartRate();
        }
    }
}
