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
            public const string SelectedTranslationFunction = "SelectedTranslationFunction";
            public const string ThemeIndex = "ThemeIndex";
            public const string Debug = "Debug";
            public const string CacheEditorSearchText = "CacheEditorSearchText";

            public const string TFToCulture = "TFToCulture";
            public const string TFTestRepeats = "TFTestRepeats";
            public const string TFTextToTranslate = "TFTextToTranslate";
            public const string TFLastSelectorBarItemTag = "TFLastSelectorBarItemTag";

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

            App.Vm.Target = (appData.Values.ContainsKey(AppSettingsKeys.Target)) ?
                (string)appData.Values[AppSettingsKeys.Target] : @"C:\Repo\WinUITranslator\Sample-Packaged";

            App.Vm.SelectedTranslationFunction = (appData.Values.ContainsKey(AppSettingsKeys.SelectedTranslationFunction)) ?
                (string)appData.Values[AppSettingsKeys.SelectedTranslationFunction] : "OpenAI_1";

            App.Vm.SearchText = (appData.Values.ContainsKey(AppSettingsKeys.CacheEditorSearchText)) ?
                (string)appData.Values[AppSettingsKeys.CacheEditorSearchText] : "";

            App.Vm.TFToCulture = (appData.Values.ContainsKey(AppSettingsKeys.TFToCulture)) ?
                (string)appData.Values[AppSettingsKeys.TFToCulture] : "de-DE";
            App.Vm.TFTestRepeats = (appData.Values.ContainsKey(AppSettingsKeys.TFTestRepeats)) ?
                (int)appData.Values[AppSettingsKeys.TFTestRepeats] : 1;
            App.Vm.TFTextToTranslate = (appData.Values.ContainsKey(AppSettingsKeys.TFTextToTranslate)) ?
                (string)appData.Values[AppSettingsKeys.TFTextToTranslate] : "Light";

            LastNavItemTag = (appData.Values.ContainsKey(AppSettingsKeys.LastNavItemTag)) ?
                (string)appData.Values[AppSettingsKeys.LastNavItemTag] : "Target";

            TFLastSelectorBarItemTag = (appData.Values.ContainsKey(AppSettingsKeys.TFLastSelectorBarItemTag)) ?
                (string)appData.Values[AppSettingsKeys.TFLastSelectorBarItemTag] : "Test";

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
            appData.Values[AppSettingsKeys.SelectedTranslationFunction] = App.Vm.SelectedTranslationFunction;
            appData.Values[AppSettingsKeys.TFTestText] = App.Vm.TFTextToTranslate;
            appData.Values[AppSettingsKeys.CacheEditorSearchText] = App.Vm.SearchText;

            appData.Values[AppSettingsKeys.TFToCulture] = App.Vm.TFToCulture;
            appData.Values[AppSettingsKeys.TFTestRepeats] = App.Vm.TFTestRepeats;
            appData.Values[AppSettingsKeys.TFTextToTranslate] = App.Vm.TFTextToTranslate;




            appData.Values[AppSettingsKeys.IsMaximized] = IsMaximized;
            appData.Values[AppSettingsKeys.WindowLeft] = WindowLeft;
            appData.Values[AppSettingsKeys.WindowTop] = WindowTop;
            appData.Values[AppSettingsKeys.WindowWidth] = WindowWidth;
            appData.Values[AppSettingsKeys.WindowHeight] = WindowHeight;
            appData.Values[AppSettingsKeys.WindowScale] = WindowScale;
            appData.Values[AppSettingsKeys.LastNavItemTag] = LastNavItemTag;

            appData.Values[AppSettingsKeys.TFLastSelectorBarItemTag] = TFLastSelectorBarItemTag;

        }

    }
}

