using Microsoft.UI.Xaml;
using System;
using System.IO;
using TeeLocalized;
using System.Diagnostics;
using Windows.ApplicationModel;
using Microsoft.UI.Windowing;

namespace Sample_Packaged
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var aw = AppWindow.GetFromWindowId(windowId);
            aw.Closing += AppWindow_Closing;
            AppWindow.Title = "Packaged Translator Demo App";
        }

        private void LogSample(string msg)
        {
            tbSamples.Text = tbSamples.Text + msg + Environment.NewLine;
        }

        private string _localizedDataPath;
        private void GrdMain_Loaded(object sender, RoutedEventArgs e)
        {
            grdMain.FlowDirection = (App.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight);

            //we will be saving a file in the \Translator folder in this project, so calc path for it
            //this calculation can sometimes fail, so you can also implicitly set it to
            //  ex. "c:\dev\myapp\Translator\TLocalizedGets.json"
            _localizedDataPath = Path.Combine(Package.Current.InstalledPath, @"..\..\..\..\..\..");
            _localizedDataPath = Path.GetFullPath(_localizedDataPath);
            _localizedDataPath = Path.Combine(_localizedDataPath, @"Translator\TLocalizedGets.json");
            TLocalized.TrackKeyVals = Debugger.IsAttached;
            TLocalized.ThrowExceptionsOnErrors = false;
            TLocalized.LoadKeyVals(_localizedDataPath);

            //let's translate some dynamic text
            //get a translated string
            string s = TLocalized.Get("LoadingFile", "@", "Loading {0}}, please wait...");
            LogSample(Environment.NewLine + "Get a translated string.");
            LogSample(@"TLocalized.Get('LoadingFile', '@', 'Loading {0}, please wait...');");
            LogSample("returns");
            LogSample(s);

            //get a translated string with a generic hint - this translation is probably not so good
            LogSample(@"----------------------------------------");
            LogSample(Environment.NewLine + "Get a translated string with a generic hint - this translation is probably not very accurate.");
            s = TLocalized.Get("OpenAperture_Generic", "@", "Open the aperture for a shallower depth of field.");
            LogSample(@"TLocalized.Get('OpenAperture_Generic', '@', 'Open the aperture for a shallower depth of field.');");
            LogSample("returns");
            LogSample(s);

            //get a translated string with a hint that has more context - this translation should be better
            LogSample(@"----------------------------------------");
            LogSample("Get a translated string with a hint that has more context - this translation should be better.");
            s = TLocalized.Get("OpenAperture_MoreContext", "!", "Open the aperture for a shallower depth of field.");
            LogSample(@"TLocalized.Get('OpenAperture_MoreContext', '!', 'Open the aperture for a shallower depth of field.');");
            LogSample("returns");
            LogSample(s);

            //get a translated string with a hint that has more context and contrain the length to the same or shorter than the untranslated text
            LogSample(@"----------------------------------------");
            LogSample("Get a translated string with a hint that has more context and contrain the length to the same or shorter than the untranslated text.");
            s = TLocalized.Get("OpenAperture_MoreContext_Constrained", "!!", "Open the aperture for a shallower depth of field.");
            LogSample(@"TLocalized.Get('OpenAperture_MoreContext_Constrained', '!!', 'Open the aperture for a shallower depth of field.');");
            LogSample("returns");
            LogSample(s);

            //gets a non-translated value, like a localized icon filename, glyph or color
            LogSample(@"----------------------------------------");
            LogSample("Gets a non-translated value, like a localized icon filename, glyph or color.");
            s = TLocalized.GetSpecial("SettingsIcon" );
            LogSample(@"s = TLocalized.GetSpecial('SettingsIcon');");
            LogSample("returns");
            LogSample(s ?? "null");

        }

        private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            TLocalized.SaveKeyVals();
        }
    }
}
