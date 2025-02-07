using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Windows.Storage;

namespace Translator
{
    public static class TSettings
    {
        public static bool Debug = false;

        public static int WindowLeft;
        public static int WindowTop;
        public static int WindowWidth;
        public static int WindowHeight;
        public static double WindowScale;
        public static string LastNavItemTag;
        public static bool IsMaximized;
        public static string TFLastSelectorBarItemTag;

        private static string DefaultTarget = @"C:\Repo\WinUITranslator\Sample-Packaged";

        public static class AppSettingsKeys
        {
            public const string IsMaximized = "IsMaximized";
            public const string WindowLeft = "WindowLeft";
            public const string WindowTop = "WindowTop";
            public const string WindowWidth = "WindowWidth";
            public const string WindowHeight = "WindowHeight";
            public const string WindowScale = "WindowScale";
            public const string LastNavItemTag = "LastNavItemTag";
            public const string Target = "Target";
            public const string TargetList = "TargetList";
            public const string SelectedProfile = "SelectedTranslationFunction";
            public const string ThemeIndex = "ThemeIndex";
            public const string Debug = "Debug";
            public const string CacheEditorSearchText = "CacheEditorSearchText";


            public const string TranslateSaveToCache = "TranslateSaveToCache";


            public const string TranslateLogFilter = "TranslateLogFilter";



            public const string TFToCulture = "TFToCulture";
            public const string TFTestRepeats = "TFTestRepeats";
            public const string TFTextToTranslate = "TFTextToTranslate";
            public const string TFLastTabIndex = "TFLastTabIndex";

            public const string TFTestText = "TFTestText"; //?

        }

        public static void Load()
        {
            var appData = ApplicationData.Current.LocalSettings;

            if (App.Vm.KeyLeftControlPressedOnLaunch)
            {
                appData.Values.Clear(); //wipe settings in case it gets messed up            
            }

            App.Vm.ThemeIndex = (appData.Values.ContainsKey(AppSettingsKeys.ThemeIndex)) ?
                (int)appData.Values[AppSettingsKeys.ThemeIndex] : 0;

            App.Vm.Debug = (appData.Values.ContainsKey(AppSettingsKeys.Debug)) ?
                (bool)appData.Values[AppSettingsKeys.Debug] : false;

            LoadTargetList();
            App.Vm.Target = (appData.Values.ContainsKey(AppSettingsKeys.Target)) ?
                (string)appData.Values[AppSettingsKeys.Target] : @"C:\Repo\WinUITranslator\Sample-Packaged";


            //App.Vm.GetProfiles();

            App.Vm.SelectedProfile = (appData.Values.ContainsKey(AppSettingsKeys.SelectedProfile)) ?
                (string)appData.Values[AppSettingsKeys.SelectedProfile] : "Loopback";


            App.Vm.SearchText = (appData.Values.ContainsKey(AppSettingsKeys.CacheEditorSearchText)) ?
                (string)appData.Values[AppSettingsKeys.CacheEditorSearchText] : "";

            App.Vm.TFToCulture = (appData.Values.ContainsKey(AppSettingsKeys.TFToCulture)) ?
                (string)appData.Values[AppSettingsKeys.TFToCulture] : "de-DE";

            App.Vm.TFTestRepeats = (appData.Values.ContainsKey(AppSettingsKeys.TFTestRepeats)) ?
                (int)appData.Values[AppSettingsKeys.TFTestRepeats] : 1;

            App.Vm.TFTextToTranslate = (appData.Values.ContainsKey(AppSettingsKeys.TFTextToTranslate)) ?
                (string)appData.Values[AppSettingsKeys.TFTextToTranslate] : "@Close\r//@@Click to save save your profile.\r//!Aperture\r//!!Click to adjust white balance.\r//@Loading {0}, please wait...";

            App.Vm.TFLastTabIndex = (appData.Values.ContainsKey(AppSettingsKeys.TFLastTabIndex)) ?
                (int)appData.Values[AppSettingsKeys.TFLastTabIndex] : 0;



            App.Vm.TranslateSaveToCache = (appData.Values.ContainsKey(AppSettingsKeys.TranslateSaveToCache)) ?
                (bool)appData.Values[AppSettingsKeys.TranslateSaveToCache] : false;


            string TranslateLogFilter = (appData.Values.ContainsKey(AppSettingsKeys.TranslateLogFilter)) ?
                (string)appData.Values[AppSettingsKeys.TranslateLogFilter] : "inf,sum,wrn,err,tra";
            App.Vm.SetTranslateLogFilter(TranslateLogFilter);







            LastNavItemTag = (appData.Values.ContainsKey(AppSettingsKeys.LastNavItemTag)) ?
                (string)appData.Values[AppSettingsKeys.LastNavItemTag] : "Target";

            IsMaximized = (appData.Values.ContainsKey(AppSettingsKeys.IsMaximized)) ?
                (bool)appData.Values[AppSettingsKeys.IsMaximized] : false;

            WindowLeft = (appData.Values.ContainsKey(AppSettingsKeys.WindowLeft)) ?
                (int)appData.Values[AppSettingsKeys.WindowLeft] : 100;

            WindowTop = (appData.Values.ContainsKey(AppSettingsKeys.WindowTop)) ?
                (int)appData.Values[AppSettingsKeys.WindowTop] : 100;

            WindowWidth = (appData.Values.ContainsKey(AppSettingsKeys.WindowWidth)) ?
                (int)appData.Values[AppSettingsKeys.WindowWidth] : 800;

            WindowHeight = (appData.Values.ContainsKey(AppSettingsKeys.WindowHeight)) ?
                (int)appData.Values[AppSettingsKeys.WindowHeight] : 600;

            WindowScale = (appData.Values.ContainsKey(AppSettingsKeys.WindowScale)) ?
                (double)appData.Values[AppSettingsKeys.WindowScale] : 1.0f;

        }

        public static void Save()
        {
            var appData = ApplicationData.Current.LocalSettings;

            appData.Values[AppSettingsKeys.ThemeIndex] = App.Vm.ThemeIndex;
            appData.Values[AppSettingsKeys.Debug] = App.Vm.Debug;

            appData.Values[AppSettingsKeys.Target] = App.Vm.Target;
            SaveTargetList();

            appData.Values[AppSettingsKeys.SelectedProfile] = App.Vm.SelectedProfile;
            appData.Values[AppSettingsKeys.TFTestText] = App.Vm.TFTextToTranslate;
            appData.Values[AppSettingsKeys.CacheEditorSearchText] = App.Vm.SearchText;

            appData.Values[AppSettingsKeys.TFToCulture] = App.Vm.TFToCulture;
            appData.Values[AppSettingsKeys.TFTestRepeats] = App.Vm.TFTestRepeats;
            appData.Values[AppSettingsKeys.TFTextToTranslate] = App.Vm.TFTextToTranslate;

            appData.Values[AppSettingsKeys.TranslateSaveToCache] = App.Vm.TranslateSaveToCache;

            string TranslateLogFilter = App.Vm.GetTranslateLogFilter();
            appData.Values[AppSettingsKeys.TranslateLogFilter] = TranslateLogFilter;



            appData.Values[AppSettingsKeys.IsMaximized] = IsMaximized;
            appData.Values[AppSettingsKeys.WindowLeft] = WindowLeft;
            appData.Values[AppSettingsKeys.WindowTop] = WindowTop;
            appData.Values[AppSettingsKeys.WindowWidth] = WindowWidth;
            appData.Values[AppSettingsKeys.WindowHeight] = WindowHeight;
            appData.Values[AppSettingsKeys.WindowScale] = WindowScale;
            appData.Values[AppSettingsKeys.LastNavItemTag] = LastNavItemTag;

            appData.Values[AppSettingsKeys.TFLastTabIndex] = App.Vm.TFLastTabIndex;


        }


        private static void LoadTargetList()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.TryGetValue(AppSettingsKeys.TargetList, out object value))
            {
                try
                {
                    string historyJson = value as string;
                    if (!string.IsNullOrEmpty(historyJson))
                    {
                        // Deserialize the JSON string to a List<string>
                        List<string> historyItems = JsonSerializer.Deserialize<List<string>>(historyJson);
                        if (historyItems.Count == 0)
                        {
                            App.Vm.TargetList.Add(DefaultTarget);
                        }
                        else
                        {
                            if (historyItems != null)
                            {
                                foreach (var item in historyItems)
                                {
                                    App.Vm.TargetList.Add(item);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }


        private static void SaveTargetList()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            try
            {
                // Serialize the current history list to a JSON string
                string historyJson = JsonSerializer.Serialize(App.Vm.TargetList);
                localSettings.Values[AppSettingsKeys.TargetList] = historyJson;
            }
            catch (Exception ex)
            {

            }
        }

    }
}

