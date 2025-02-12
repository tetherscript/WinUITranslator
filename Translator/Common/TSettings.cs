using Microsoft.UI.Xaml;
using Windows.Storage;

namespace Translator
{
    public static class TSettings
    {
        public static int WindowLeft;
        public static int WindowTop;
        public static int WindowWidth;
        public static int WindowHeight;
        public static double WindowScale;
        public static bool IsMaximized;

        public static class AppSettingsKeys
        {
            public const string IsMaximized = "IsMaximized";
            public const string WindowLeft = "WindowLeft";
            public const string WindowTop = "WindowTop";
            public const string WindowWidth = "WindowWidth";
            public const string WindowHeight = "WindowHeight";
            public const string WindowScale = "WindowScale";
            public const string IsDarkTheme = "IsDarkTheme";
        }

        public static void Load()
        {
            var appData = ApplicationData.Current.LocalSettings;
            if (App.Vm.KeyLeftControlPressedOnLaunch)
            {
                appData.Values.Clear(); //wipe settings in case it gets messed up            
            }
            bool isDarkTheme = (appData.Values.ContainsKey(AppSettingsKeys.IsDarkTheme)) ?
                (bool)appData.Values[AppSettingsKeys.IsDarkTheme] : true;
            App.Vm.IsDarkTheme = isDarkTheme;
            App.Vm.Theme = isDarkTheme ? ElementTheme.Dark : ElementTheme.Light;
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
            appData.Values[AppSettingsKeys.IsDarkTheme] = App.Vm.IsDarkTheme;
            appData.Values[AppSettingsKeys.IsMaximized] = IsMaximized;
            appData.Values[AppSettingsKeys.WindowLeft] = WindowLeft;
            appData.Values[AppSettingsKeys.WindowTop] = WindowTop;
            appData.Values[AppSettingsKeys.WindowWidth] = WindowWidth;
            appData.Values[AppSettingsKeys.WindowHeight] = WindowHeight;
            appData.Values[AppSettingsKeys.WindowScale] = WindowScale;
        }

    }
}

