﻿using Plugin.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using WindesHeartSdk.Model;
using WindesHeartSDK.Devices.MiBand3Device.Helpers;
using WindesHeartSDK.Devices.MiBand3Device.Models;
using WindesHeartSDK.Devices.MiBand3Device.Resources;
using WindesHeartSDK.Helpers;
using static WindesHeartSDK.Helpers.ConversionHelper;

namespace WindesHeartSDK.Devices.MiBand3Device.Services
{
    class MiBand3FetchService
    {
        private readonly MiBand3 _miBand3;
        private readonly List<ActivitySample> _samples = new List<ActivitySample>();

        private DateTime _firstTimestamp;
        private DateTime _lastTimestamp;
        private int _pkg = 0;

        private IDisposable _charUnknownSub;
        private IDisposable _charActivitySub;


        public MiBand3FetchService(MiBand3 device)
        {
            _miBand3 = device;
        }

        /// <summary>
        /// Clear the list of samples and start fetching
        /// </summary>
        public void StartFetching(DateTime date)
        {
            _samples.Clear();
            InitiateFetching(date);
        }

        /// <summary>
        /// Setup the disposables for the fetch operation
        /// </summary>
        /// <param name="date"></param>
        public async void InitiateFetching(DateTime date)
        {
            //Dispose all DIsposables to prevent double data
            _charActivitySub?.Dispose();
            _charUnknownSub?.Dispose();

            // Subscribe to the unknown and activity characteristics
            _charUnknownSub = _miBand3.GetCharacteristic(MiBand3Resource.GuidUnknownCharacteristic4).RegisterAndNotify().Subscribe(handleUnknownChar);
            _charActivitySub  = _miBand3.GetCharacteristic(MiBand3Resource.GuidCharacteristic5ActivityData).RegisterAndNotify().Subscribe(handleActivityChar);

            // Write the date and time from which to receive samples to the Mi Band
            await WriteDateBytes(date);
        }

        /// <summary>
        /// Write the date from wich to recieve data to the mi band
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private async Task WriteDateBytes(DateTime date)
        {
            // Convert date to bytes
            byte[] Timebytes = GetTimeBytes(date, TimeUnit.Minutes);
            byte[] Fetchbytes = new byte[10] { 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
            
            // Copy the date in the byte template to send to the device
            Buffer.BlockCopy(Timebytes, 0, Fetchbytes, 2, 8);

            // Send the bytes to the device
            await _miBand3.GetCharacteristic(MiBand3Resource.GuidUnknownCharacteristic4).WriteWithoutResponse(Fetchbytes);
        }

        /// <summary>
        /// Called when recieving MetaData
        /// </summary>
        /// <param name="result"></param>
        public async void handleUnknownChar(CharacteristicGattResult result)
        {
            Console.WriteLine("handleUnknownChar");

            // Create an empty byte array and copy the response type to it
            byte[] responseByte = new byte[3];
            Buffer.BlockCopy(result.Data, 0, responseByte, 0, 3);

            Console.WriteLine("responseByte: " + responseByte[0].ToString() + " - " + responseByte[1].ToString() + " - " + responseByte[2].ToString());

            if (result.Data.Length > 3)
            {
                Console.WriteLine("Expected Samples: " + result.Data[3].ToString() + " - " + result.Data[4].ToString() + " - " + result.Data[5].ToString());
            }

            // Check if our request was accepted
            if(responseByte.SequenceEqual(new byte[3] { 0x10, 0x01, 0x01 }))
            {
                Console.WriteLine("First If");

                // Get the timestamp of the first sample
                byte[] DateTimeBytes = new byte[8];
                Buffer.BlockCopy(result.Data, 7, DateTimeBytes, 0, 8);
                _firstTimestamp = RawBytesToCalendar(DateTimeBytes);

                Console.WriteLine("Fetching data from: " + _firstTimestamp.ToString());

                // Write 0x02 to tell the band to start the fetching process
                await _miBand3.GetCharacteristic(MiBand3Resource.GuidUnknownCharacteristic4).WriteWithoutResponse(new byte[] { 0x02 });
                Console.WriteLine("Done writing 0x02");


            }
            // Check if done fetching
            else if(responseByte.SequenceEqual(new byte[3] { 0x10, 0x02, 0x01 }))
            {
                Console.WriteLine("Done Fetching: " + _samples.Count + " Samples");
                _charActivitySub?.Dispose();
                _charUnknownSub?.Dispose();
                foreach(ActivitySample sample in _samples)
                {
                    Console.WriteLine(sample.ToString());
                }
            }
            else
            {
                Console.WriteLine("Error while Fetching");
                // Error while fetching
                _charActivitySub?.Dispose();
                _charUnknownSub?.Dispose();
            }
        }

        /// <summary>
        /// Called when recieving samples
        /// </summary>
        /// <param name="result"></param>
        private void handleActivityChar(CharacteristicGattResult result)
        {
            Console.WriteLine("HandleActivityChar");


            if (result.Data.Length % 4 != 1)
            {
                if (_lastTimestamp > DateTime.Now.AddMinutes(-1))
                {
                    Console.WriteLine("Done Fetching: " + _samples.Count + " Samples");
                }
                Console.WriteLine("Need More fetching");
                InitiateFetching(_lastTimestamp.AddMinutes(1));
            }
            else
            {
                Console.WriteLine("ElseStatement");
                var LocalPkg = _pkg; // ??
                _pkg++;
                var i = 1;
                while (i < result.Data.Length)
                {
                    int timeIndex = (LocalPkg) * 4 + (i - 1) / 4;
                    var timeStamp = _firstTimestamp.AddMinutes(timeIndex);
                    _lastTimestamp = timeStamp; //This doesn't seem right


                    foreach (byte b in result.Data)
                    {
                        Console.WriteLine(b);
                    }

                    // Create a sample from the recieved bytes
                    var category = result.Data[i] & 0xff; //ToUint16(new byte[] { result.Data[i], result.Data[i + 1] });
                    var intensity = result.Data[i + 1] & 0xff; //ToUint16(new byte[] { result.Data[i], result.Data[i + 1] });
                    var steps = result.Data[i + 2] & 0xff;
                    var heartrate = result.Data[i + 3];

                    // Add the sample to the sample list
                    _samples.Add(new ActivitySample(timeStamp, category, intensity, steps, heartrate));
                    Console.WriteLine("Added Sample: Total = " + _samples.Count);

                    i += 4;

                    var d = DateTime.Now.AddMinutes(-1);
                    d.AddSeconds(-d.Second);
                    d.AddMilliseconds(-d.Millisecond);
                    

                    if (timeStamp == d)
                    {
                        Console.WriteLine("Done Fetching");
                        break;
                    }
                }
            }
        }
    }
}
