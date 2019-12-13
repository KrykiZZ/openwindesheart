﻿using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using WindesHeartApp.Models;
using WindesHeartApp.Pages;
using WindesHeartApp.Resources;
using WindesHeartSDK;
using Xamarin.Forms;

namespace WindesHeartApp.ViewModels
{
    public class SettingsPageViewModel : INotifyPropertyChanged
    {
        private static ISettings AppSettings => CrossSettings.Current;

        private int _languageIndex = 0;
        private int _hourIndex = 0;
        private int _dateIndex = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        public SettingsPageViewModel()
        {
        }

        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void OnAppearing()
        {
            //Set correct settings
            if (DeviceSettings.TimeFormat24Hour) SettingsPage.HourPicker.SelectedIndex = 0;
            else SettingsPage.HourPicker.SelectedIndex = 1;

            if (DeviceSettings.DateFormatDMY) SettingsPage.DatePicker.SelectedIndex = 0;
            else SettingsPage.DatePicker.SelectedIndex = 1;

            SettingsPage.WristSwitch.IsToggled = DeviceSettings.WristRaiseDisplay;

            for (int i = 0; i < SettingsPage.StepsPicker.Items.Count; i++)
            {
                if (Globals.DailyStepsGoal.ToString().Equals(SettingsPage.StepsPicker.Items[i]))
                {
                    SettingsPage.StepsPicker.SelectedIndex = i;
                }
            }

            //Add languages
            int index = 0;
            foreach (string key in Globals.LanguageDictionary.Keys)
            {
                SettingsPage.LanguagePicker.Items.Add(key);

                //Set selected
                Globals.LanguageDictionary.TryGetValue(key, out string code);
                if (DeviceSettings.DeviceLanguage.Equals(code)) SettingsPage.LanguagePicker.SelectedIndex = index;

                index++;
            }

        }

        public void DateIndexChanged(object sender, EventArgs args)
        {
            Picker picker = sender as Picker;
            if (picker.SelectedIndex != -1)
            {
                string format = picker.Items[picker.SelectedIndex];
                bool isDMY = format.Equals("DD/MM/YYYY");

                try
                {
                    //Set on device
                    Windesheart.ConnectedDevice?.SetDateDisplayFormat(isDMY);
                    DeviceSettings.DateFormatDMY = isDMY;
                    _dateIndex = picker.SelectedIndex;
                }
                catch (Exception)
                {
                    //Set picker index back to old value
                    picker.SelectedIndex = _dateIndex;
                    Console.WriteLine("Something went wrong!");
                }
            }
        }
        
        public void HourIndexChanged(object sender, EventArgs e)
        {
            Picker picker = sender as Picker;
            if (picker.SelectedIndex != -1)
            {
                string format = picker.Items[picker.SelectedIndex];
                bool is24 = format.Equals("24 hour");

                try
                {
                    //Set on device
                    Windesheart.ConnectedDevice?.SetTimeDisplayFormat(is24);
                    DeviceSettings.TimeFormat24Hour = is24;
                    _hourIndex = picker.SelectedIndex;
                }
                catch (Exception)
                {
                    //Set picker index back to old value
                    picker.SelectedIndex = _hourIndex;
                    Console.WriteLine("Something went wrong!");
                }
            }
        }

        public void LanguageIndexChanged(object sender, EventArgs e)
        {
            Picker picker = sender as Picker;
            if (picker.SelectedIndex != -1)
            {
                //Get language code
                string language = picker.Items[picker.SelectedIndex];
                Globals.LanguageDictionary.TryGetValue(language, out string languageCode);

                try
                {
                    //Set on device
                    Windesheart.ConnectedDevice?.SetLanguage(languageCode);
                    DeviceSettings.DeviceLanguage = languageCode;
                    _languageIndex = picker.SelectedIndex;
                }
                catch (Exception)
                {
                    //Set picker index back to old value
                    picker.SelectedIndex = _languageIndex;
                    Console.WriteLine("Something went wrong!");
                }      
            }
        }

        public void StepsIndexChanged(object sender, EventArgs e)
        {
            Picker picker = sender as Picker;
            if (picker.SelectedIndex != -1)
            {
                string steps = picker.Items[picker.SelectedIndex];
                Globals.DailyStepsGoal = int.Parse(steps);
            }
        }

        public void OnWristToggled(object sender, ToggledEventArgs e)
        {
            Switch sw = sender as Switch;
            bool toggled = sw.IsToggled;
            
            try
            {
                Windesheart.ConnectedDevice?.SetActivateOnLiftWrist(toggled);
                DeviceSettings.WristRaiseDisplay = toggled;
            }
            catch (Exception)
            {
                //toggle back the switch
                SettingsPage.WristSwitch.IsToggled = !toggled;
                Console.WriteLine("Something went wrong!");
            }
        }
    }
}
