using System;
using System.Windows;
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
using Windows.Devices.Spi;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PIREADER
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            timer = new DispatcherTimer(); //Set up new dispatcher timer
            timer.Interval = TimeSpan.FromMilliseconds(100); //read at 100ms intervals.
            timer.Tick += timer_tick; //incriment clock cycle.
            timer.Start(); //start clock

            InitializeSpi(); //run function called initializeSpi.
        }

            private async void InitializeSpi() //declare function called Spi.
        {
            try
            {
                var settings = new SpiConnectionSettings(CHIP_SELECT); //setup SPI settings.
                settings.ClockFrequency = 1000000; //set clock frequency to 1MHz.
                settings.Mode = SpiMode.Mode0; //set spi mode to 0: clk idle low, latch on rising edge

                string SpiInput = SpiDevice.GetDeviceSelector(INPUT_CHANNEL);  //Select which Input channel to read from.
                var DeviceInfo = await DeviceInformation.FindAllAsync(SpiInput); //Find asynchronous signals from A/DC
                SpiDisplay = await SpiDevice.FromIdAsync(DeviceInfo[0].Id, settings); // Starts an asynchronous object 
     
            }
            catch (Exception ex)  //If this doesnt work.
            {
                throw new Exception("Spi Initialization Failed", ex);  //send exception to UI.
            }
        }

        private void timer_tick(object sender, object e) //
        {
            DisplayTextBoxContents();
        }

        public void DisplayTextBoxContents()
        {
            SpiDisplay.TransferFullDuplex(ReadBuffer, WriteBuffer);
            resolution = ConvertToInt(ReadBuffer);
            TextPlaceHolder.Text = resolution.ToString();
        }

        public int ConvertToInt(byte[] data)
        {
            int result = data[1] & 0x07;
            result <<= 8;
            result += data[2];
            return result;
        }

        private const string INPUT_CHANNEL = "SPI0";
        private const Int32 CHIP_SELECT = 0;

        byte[] ReadBuffer = new byte[3];
        byte[] WriteBuffer = new byte[3] {0x01, 0x00, 0x00 };

        private SpiDevice SpiDisplay;

        private DispatcherTimer timer;
        int resolution;
    }   
}
