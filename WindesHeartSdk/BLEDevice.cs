﻿using Plugin.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WindesHeartSDK.Models;

namespace WindesHeartSDK
{
    public abstract class BLEDevice
    {
        public int Rssi;
        public readonly IDevice Device;
        public bool Authenticated;
        public List<IGattCharacteristic> Characteristics = new List<IGattCharacteristic>();

        //Services
        public readonly BluetoothService BluetoothService;

        public BLEDevice(int rssi, IDevice device)
        {
            Rssi = rssi;
            Device = device;
            BluetoothService = new BluetoothService(this);
            Device.WhenConnected().Subscribe(x => OnConnect());
        }

        public abstract void OnConnect();
        public abstract void Connect();
        public abstract void Disconnect();
        public abstract Task<bool> SetTime(DateTime dateTime);
        public abstract Task<StepInfo> GetSteps();
        public abstract void EnableRealTimeSteps(Action<StepInfo> OnStepsChanged);
        public abstract void DisableRealTimeSteps();
        public abstract Task<Battery> GetBattery();
        public abstract void FetchData();

        /// <summary>
        /// Get a certain characteristic with its UUID.
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns>IGattCharacteristic</returns>
        public IGattCharacteristic GetCharacteristic(Guid uuid)
        {
            return Characteristics.Find(x => x.Uuid == uuid);
        }

        public abstract void EnableRealTimeBattery(Action<Battery> getBatteryStatus);
        public abstract void DisableRealTimeBattery();
        public abstract void EnableRealTimeHeartrate(Action<Heartrate> getHeartrate);
        public abstract void DisableRealTimeHeartrate();
        public abstract void SetHeartrateMeasurementInterval(int minutes);
    }
}
