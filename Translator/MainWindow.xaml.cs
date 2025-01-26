using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.Storage;

namespace Translator
{
    public sealed partial class MainWindow : Window
    {
        #region LIFECYCLE
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr LoadImage(IntPtr hInstance, string lpIconName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

        private const int WM_SETICON = 0x0080;
        private const uint IMAGE_ICON = 1;
        private const uint LR_LOADFROMFILE = 0x00000010;

        private SynchronizationContext callersCtx;
        private System.Timers.Timer TranslationStatusTimer;

        public MainWindow()
        {
            this.InitializeComponent();
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Closing += AppWindow_Closing;

            callersCtx = SynchronizationContext.Current;

            nint MainWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
            ExtendsContentIntoTitleBar = true;

            // Load the icon from file
            AppWindow.SetIcon("Assets\\app.ico");
            IntPtr hIcon = LoadImage(IntPtr.Zero, "Assets\\app.ico", IMAGE_ICON, 256, 256, LR_LOADFROMFILE);
            if (hIcon != IntPtr.Zero)
            {
                // Set the small and large icons for the taskbar and title bar
                SendMessage(MainWindowHandle, WM_SETICON, new IntPtr(0), hIcon); // Small icon
                SendMessage(MainWindowHandle, WM_SETICON, new IntPtr(1), hIcon); // Large icon
            }

            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            tbTitle.Text = "Translator " + versionInfo.FileVersion;

            TranslationStatusTimer = new(interval: 1000);
            TranslationStatusTimer.Stop();
            TranslationStatusTimer.AutoReset = false;
            TranslationStatusTimer.Elapsed += (sender, e) => TranslationStatusTimerElapsed();

        }

        private void TranslationStatusTimerElapsed()
        {
            TranslationStatusTimer.Stop();
            callersCtx?.Post(new SendOrPostCallback((_) => UpdateTranslateStatus()), null);
            TranslationStatusTimer.Start();
        }

        private void UpdateTranslateStatus()
        {
            tbTranslateLog.Text = TLog.Text;
            pbTranslate.Value = TTranslate.ProgressPerc;
        }

        private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            TranslationStatusTimer.Stop();
            SaveWindowState(this);
        }

        private void GrdMain_Loaded(object sender, RoutedEventArgs e)
        {
            grdMain.FlowDirection = (App.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight);
            RestoreWindowState(this);
            LoadHints();
        }
        #endregion

        #region SETTINGS
        public static class WindowSettingsKeys
        {
            public const string Left = "WindowLeft";
            public const string Top = "WindowTop";
            public const string Width = "WindowWidth";
            public const string Height = "WindowHeight";
            public const string Scale = "WindowScale";
            public const string Path = "Path";
            public const string TranslationFunctionIndex = "TranslationFunctionIndex";
            public const string PivotIndex = "PivotIndex";
            public const string ShowScanHelp = "ShowScanHelp";
            public const string ShowTranslateHelp = "ShowScanHelp";
            public const string ShowHintsHelp = "ShowHintsHelp";
        }

        private void SaveWindowState(Window window)
        {
            var appData = ApplicationData.Current.LocalSettings;
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            var size = appWindow.Size;
            var position = appWindow.Position;

            appData.Values[WindowSettingsKeys.Left] = position.X;
            appData.Values[WindowSettingsKeys.Top] = position.Y;
            appData.Values[WindowSettingsKeys.Width] = size.Width;
            appData.Values[WindowSettingsKeys.Height] = size.Height;
            appData.Values[WindowSettingsKeys.Scale] = grdMain.XamlRoot.RasterizationScale;
            appData.Values[WindowSettingsKeys.Path] = tbScanPath.Text;
            appData.Values[WindowSettingsKeys.TranslationFunctionIndex] = cbTranslationFunction.SelectedIndex;
            appData.Values[WindowSettingsKeys.PivotIndex] = pvtInfo.SelectedIndex;
            appData.Values[WindowSettingsKeys.ShowScanHelp] = cbShowScanHelp.IsChecked;
            appData.Values[WindowSettingsKeys.ShowTranslateHelp] = cbShowTranslateHelp.IsChecked;
            appData.Values[WindowSettingsKeys.ShowHintsHelp] = cbShowHintsHelp.IsChecked;
        }

        private void RestoreWindowState(Window window)
        {
            var appData = ApplicationData.Current.LocalSettings;

            //appData.Values.Clear(); //wipe settings in case it gets messed up

            pvtInfo.SelectedIndex = (appData.Values.ContainsKey(WindowSettingsKeys.PivotIndex)) ?
               (int)appData.Values[WindowSettingsKeys.PivotIndex] : 0;

            cbTranslationFunction.Items.Clear();
            cbTranslationFunction2.Items.Clear();
            foreach (var item in TTransFunc.Types)
            {
                cbTranslationFunction.Items.Add(item);
                cbTranslationFunction2.Items.Add(item);
            }

            cbTranslationFunction.SelectedIndex = (appData.Values.ContainsKey(WindowSettingsKeys.TranslationFunctionIndex)) ?
                (int)appData.Values[WindowSettingsKeys.TranslationFunctionIndex] : 0;
            cbTranslationFunction2.SelectedIndex = cbTranslationFunction.SelectedIndex;

            tbScanPath.Text = (string)appData.Values[WindowSettingsKeys.Path];
            if (!Directory.Exists(tbScanPath.Text))
            {
                tbScanPath.Text = "";
            }
            else
            {
                TUtils.CalcPaths(tbScanPath.Text);
            }

            bool? b = (appData.Values.ContainsKey(WindowSettingsKeys.ShowScanHelp)) ?
               (bool)appData.Values[WindowSettingsKeys.ShowScanHelp] : true;
            cbShowScanHelp.IsChecked = b ?? true;
            rtbScanHelp.Visibility = ((bool)cbShowScanHelp.IsChecked ? Visibility.Visible : Visibility.Collapsed);

            bool? b1 = (appData.Values.ContainsKey(WindowSettingsKeys.ShowTranslateHelp)) ?
               (bool)appData.Values[WindowSettingsKeys.ShowTranslateHelp] : true;
            cbShowTranslateHelp.IsChecked = b ?? true;
            rtbTranslateHelp.Visibility = ((bool)cbShowTranslateHelp.IsChecked ? Visibility.Visible : Visibility.Collapsed);

            bool? b2 = (appData.Values.ContainsKey(WindowSettingsKeys.ShowHintsHelp)) ?
                (bool)appData.Values[WindowSettingsKeys.ShowHintsHelp] : true;
            cbShowHintsHelp.IsChecked = b ?? true;
            rtbHintsHelp.Visibility = ((bool)cbShowHintsHelp.IsChecked ? Visibility.Visible : Visibility.Collapsed);

            if (appData.Values.ContainsKey(WindowSettingsKeys.Left) &&
                appData.Values.ContainsKey(WindowSettingsKeys.Top) &&
                appData.Values.ContainsKey(WindowSettingsKeys.Width) &&
                appData.Values.ContainsKey(WindowSettingsKeys.Height) &&
                appData.Values.ContainsKey(WindowSettingsKeys.Scale))
            {
                var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
                var appWindow = AppWindow.GetFromWindowId(windowId);

                var left = (int)appData.Values[WindowSettingsKeys.Left];
                var top = (int)appData.Values[WindowSettingsKeys.Top];
                var width = (int)appData.Values[WindowSettingsKeys.Width];
                var height = (int)appData.Values[WindowSettingsKeys.Height];
                var scale = (double)appData.Values[WindowSettingsKeys.Scale];

                double InitialRasterizationScale = grdMain.XamlRoot.RasterizationScale;
                double InitialScaleFactor = InitialRasterizationScale / scale;

                appWindow.Move(new PointInt32 { X = left, Y = top });
                appWindow.Resize(new SizeInt32
                {
                    Width = (int)Math.Truncate(width * InitialScaleFactor),
                    Height = (int)Math.Truncate(height * InitialScaleFactor)
                });
            }
            else
            {
                // Set default size and position if no settings are found
                var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
                var appWindow = AppWindow.GetFromWindowId(windowId);
                appWindow.Move(new PointInt32 { X = 100, Y = 100 });
                appWindow.Resize(new SizeInt32 { Width = 800, Height = 600 });
            }
        }

        private void CbShowScanHelp_Click(object sender, RoutedEventArgs e)
        {
            rtbScanHelp.Visibility = ((bool)cbShowScanHelp.IsChecked ? Visibility.Visible : Visibility.Collapsed);
        }

        private void CbShowtranslateHelp_Click(object sender, RoutedEventArgs e)
        {
            rtbTranslateHelp.Visibility = ((bool)cbShowTranslateHelp.IsChecked ? Visibility.Visible : Visibility.Collapsed);
        }

        private void CbShowHintsHelp_Click(object sender, RoutedEventArgs e)
        {
            rtbHintsHelp.Visibility = ((bool)cbShowHintsHelp.IsChecked ? Visibility.Visible : Visibility.Collapsed);
        }

        private void TbScanPath_LostFocus(object sender, RoutedEventArgs e)
        {
            TUtils.CalcPaths(tbScanPath.Text.Trim());
            LoadHints();
        }
        #endregion

        #region HINTS
        private Dictionary<string, string> Hints = new Dictionary<string, string>();

        private void CbTranslationFunction2_SelectionChanged(object sender, Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
            LoadHints();
        }

        public void LoadHints()
        {
            if (File.Exists(TUtils.TargetTranslatorHintsPath))
            {
                if (cbTranslationFunction2.SelectedValue != null)
                {
                    string type = cbTranslationFunction2.SelectedValue.ToString();

                    string loadedJson = File.ReadAllText(TUtils.TargetTranslatorHintsPath);
                    // Deserialize to a list of LocalizedEntry
                    var newEntries = JsonSerializer.Deserialize<List<TUtils.HintKeyValEntry>>(
                        loadedJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                    foreach (var entry in newEntries)
                    {
                        Hints[entry.Key] = entry.Value;
                    }

                    if (Hints.ContainsKey(type + ":@"))
                    {
                        tb1.Text = Hints[type + ":@"];
                    }
                    else 
                    {
                        tb1.Text = "";
                    }

                    if (Hints.ContainsKey(type + ":@@"))
                    {
                        tb2.Text = Hints[type + ":@@"];
                    }
                    else
                    {
                        tb2.Text = "";
                    }

                    if (Hints.ContainsKey(type + ":!"))
                    {
                        tb3.Text = Hints[type + ":!"];
                    }
                    else
                    {
                        tb3.Text = "";
                    }

                    if (Hints.ContainsKey(type + ":!!"))
                    {
                        tb4.Text = Hints[type + ":!!"];
                    }
                    else
                    {
                        tb4.Text = "";
                    }

                }
                else
                {
                    tb1.Text = "";
                    tb2.Text = "";
                    tb3.Text = "";
                    tb4.Text = "";
                }
            }
        }

        private void BtnSaveHints_Click(object sender, RoutedEventArgs e)
        {
            SaveHints(TUtils.TargetTranslatorHintsPath);
        }

        public void SaveHints(string path)
        {
            if (File.Exists(TUtils.TargetTranslatorHintsPath))
            {
                if (cbTranslationFunction2.SelectedValue != null)
                {
                    string type = cbTranslationFunction2.SelectedValue.ToString();

                    Hints[type + ":@"] = tb1.Text;
                    Hints[type + ":@@"] = tb2.Text;
                    Hints[type + ":!"] = tb3.Text;
                    Hints[type + ":!!"] = tb4.Text;

                    var entries = Hints.Select(kvp => new TUtils.HintKeyValEntry
                    {
                        Key = kvp.Key,
                        Value = kvp.Value
                    }).ToList();
                    var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                    string jsonOutput = JsonSerializer.Serialize(entries, jsonOptions);
                    File.WriteAllText(path, jsonOutput);
                }
            }
        }
        #endregion

        //SCAN
        private async void btnScan_Click(object sender, RoutedEventArgs e)
        {
            TLog.Reset();
            btnScan.IsEnabled = false;
            tbScanLog.IsEnabled = false;
            prScan.IsActive = true;
            await Task.Delay(1000);

            if (TUtils.CalcPaths(tbScanPath.Text.Trim()))
            {
                await TScan.Start(TUtils.TargetRootPath, Hints);
            }
            else
            {
                TLog.Log("Target root path does not exist: " + tbScanPath.Text.Trim(), true);
            }

            tbScanLog.Text = TLog.Text;
            prScan.IsActive = false;
            tbScanLog.IsEnabled = true;
            btnScan.IsEnabled = true;
        }

        //TRANSLATE
        private async void BtnTranslateStart_Click(object sender, RoutedEventArgs e)
        {
            if (cbTranslationFunction.SelectedValue != null)
            {
                TLog.Reset();
                btnTranslateStart.IsEnabled = false;
                btnTranslateStop.IsEnabled = true;
                cbTranslationFunction.IsEnabled = false;
                tbTranslateLog.IsEnabled = false;
                prTranslate.IsActive = true;
                pbTranslate.Value = TTranslate.ProgressPerc;
                TranslationStatusTimer.Start();

                if (TUtils.CalcPaths(tbScanPath.Text.Trim()))
                {
                    string mode = cbTranslationFunction.SelectedValue.ToString();
                    await TTranslate.Start(TUtils.TargetRootPath, mode, Hints);
                }
                else
                {
                    TLog.Log("Target root path does not exist: " + tbScanPath.Text.Trim(), true);
                }
                tbTranslateLog.Text = TLog.Text;

                TranslationStatusTimer.Stop();
                btnTranslateStart.IsEnabled = true;
                btnTranslateStop.IsEnabled = false;
                cbTranslationFunction.IsEnabled = true;
                tbTranslateLog.IsEnabled = true;
                prTranslate.IsActive = false;
                pbTranslate.Value = TTranslate.ProgressPerc;
            }
        }

        private void BtnTranslateStop_Click(object sender, RoutedEventArgs e)
        {
            TTranslate.Stop();
        }

    }

}
