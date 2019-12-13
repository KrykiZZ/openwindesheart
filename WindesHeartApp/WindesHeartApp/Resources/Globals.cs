﻿using Plugin.Settings;
using Plugin.Settings.Abstractions;
﻿using System.Collections.Generic;
using WindesHeartApp.Data;
using WindesHeartApp.Data.Interfaces;
using WindesHeartApp.Services;
using WindesHeartApp.ViewModels;
using Xamarin.Forms;

namespace WindesHeartApp.Resources
{
    public static class Globals
    {
        private static ISettings AppSettings => CrossSettings.Current;

        public static HeartratePageViewModel HeartratePageViewModel;
        public static SamplesService SamplesService { get; private set; }
        public static DevicePageViewModel DevicePageViewModel;
        public static HomePageViewModel HomePageViewModel;
        public static SettingsPageViewModel SettingsPageViewModel;
        public static StepsPageViewModel StepsPageViewModel;
        public static SleepPageViewModel SleepPageViewModel;

        public static double ScreenHeight { get; set; }
        public static double ScreenWidth { get; set; }
        public static Color PrimaryColor { get; set; } = Color.FromHex("#96d1ff");
        public static Color SecondaryColor { get; set; } = Color.FromHex("#53b1ff");
        public static Color LightTextColor { get; set; } = Color.FromHex("#999999");
        public static IStepsRepository StepsRepository { get; set; }
        public static ISleepRepository SleepRepository { get; set; }
        public static IHeartrateRepository HeartrateRepository { get; set; }

        public static DatabaseContext DatabaseContext { get; set; }
        public static Dictionary<string, Color> ColorDictionary;
        public static Dictionary<string, string> FormatDictionary;

        public static int DailyStepsGoal
        {
            get => AppSettings.GetValueOrDefault(nameof(DailyStepsGoal), 10000);
            set => AppSettings.AddOrUpdateValue(nameof(DailyStepsGoal), value);
        }

        public static Dictionary<string, string> LanguageDictionary;

        public static void BuildGlobals(IHeartrateRepository heartrateRepository, ISleepRepository sleepRepository, IStepsRepository stepsRepository, DatabaseContext databaseContext)
        {
            StepsRepository = stepsRepository;
            SleepRepository = sleepRepository;
            HeartrateRepository = heartrateRepository;
            HeartratePageViewModel = new HeartRatePageViewModel(HeartrateRepository);
            SamplesService = new SamplesService(HeartrateRepository, StepsRepository, SleepRepository);
            StepsViewModel = new StepsPageViewModel(StepsRepository);
            SettingsPageViewModel = new SettingsPageViewModel();
            SleepPageViewModel = new SleepPageViewModel(sleepRepository);
            DevicePageViewModel = new DevicePageViewModel();
            HomePageViewModel = new HomePageViewModel();

            LanguageDictionary = new Dictionary<string, string>
            {
                {"Nederlands", "nl-NL"},
                {"English", "en-EN"},
                {"Deutsch", "de-DE"}
            };
        }
    }
}