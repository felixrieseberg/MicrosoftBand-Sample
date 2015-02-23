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
using System.Collections.ObjectModel;
using Microsoft.Band.Notifications;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace HelloBand
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public IBandInfo[] pairedBands;
        public IBandClient bandClient;
        public IBandHeartRateReading heartreading;

        public MainPage()
        {
            this.InitializeComponent();
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
                await bandClient.NotificationManager.VibrateAsync(VibrationType.AlertAlarm);
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
