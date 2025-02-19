using Windows.Foundation;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Runtime.InteropServices;
using Windows.Graphics;
using Windows.Storage;
using WinRT.Interop;

namespace Translator
{
    public sealed partial class MainWindow : Window
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr LoadImage(IntPtr hInstance, string lpIconName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // Constants for showing/hiding
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        private const int WM_SETICON = 0x0080;
        private const uint IMAGE_ICON = 1;
        private const uint LR_LOADFROMFILE = 0x00000010;

        private readonly MainWIndowVm _vm = App.Vm;
        public MainWIndowVm Vm { get => _vm; }

        public MainWindow()
        {
            this.InitializeComponent();
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Closing += AppWindow_Closing;

            TSettings.Load();

            nint MainWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(this.AppTitleBar);

            // Load the icon from file
            AppWindow.SetIcon("Assets\\app.ico");
            IntPtr hIcon = LoadImage(IntPtr.Zero, "Assets\\app.ico", IMAGE_ICON, 256, 256, LR_LOADFROMFILE);
            if (hIcon != IntPtr.Zero)
            {
                // Set the small and large icons for the taskbar and title bar
                SendMessage(MainWindowHandle, WM_SETICON, new IntPtr(0), hIcon); // Small icon
                SendMessage(MainWindowHandle, WM_SETICON, new IntPtr(1), hIcon); // Large icon
            }

            frMainPage.Navigate(typeof(MainPage), null, new SuppressNavigationTransitionInfo());
        }

        private void GrdMain_Loaded(object sender, RoutedEventArgs e)
        {
            grdMain.FlowDirection = (App.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight);
            RestoreWindowSizePos(this);
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            ShowWindow(hWnd, SW_SHOW);
            SetRegionsForCustomTitleBar();
        }

        private bool _firstActivation = true;
        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);

            // Hide
            if (_firstActivation)
            {
                _firstActivation = false;
                ShowWindow(hWnd, SW_HIDE);
            }
        }

        private void SetRegionsForCustomTitleBar()
        {
            var appWindow = App.m_window;
            double scaleAdjustment = grdMain.XamlRoot.RasterizationScale;

            GeneralTransform transform = btnTheme.TransformToVisual(null);
            Rect bounds = transform.TransformBounds(new Rect(0, 0,
                                                             btnTheme.ActualWidth,
                                                             btnTheme.ActualHeight));
            RectInt32 btnThemeRect = GetRect(bounds, scaleAdjustment);

            var rectArray = new RectInt32[] { btnThemeRect };

            InputNonClientPointerSource nonClientInputSrc =
                InputNonClientPointerSource.GetForWindowId(this.AppWindow.Id);
            nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, rectArray);
        }

        private Windows.Graphics.RectInt32 GetRect(Rect bounds, double scale)
        {
            return new Windows.Graphics.RectInt32(
                _X: (int)Math.Round(bounds.X * scale),
                _Y: (int)Math.Round(bounds.Y * scale),
                _Width: (int)Math.Round(bounds.Width * scale),
                _Height: (int)Math.Round(bounds.Height * scale)
            );
        }

        private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            SaveWindowSizePos(this);
            TSettings.Save();
            WeakReferenceMessenger.Default.Send(new TShuttingDown(""));
        }

        private void RestoreWindowSizePos(Window window)
        {
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            var left = TSettings.WindowLeft;
            var top = TSettings.WindowTop;
            var width = TSettings.WindowWidth;
            var height = TSettings.WindowHeight;
            var scale = TSettings.WindowScale;
            double InitialRasterizationScale = grdMain.XamlRoot.RasterizationScale;
            double InitialScaleFactor = InitialRasterizationScale / scale; 
            if (TSettings.IsMaximized)
            {
                OverlappedPresenter presenter = (OverlappedPresenter)appWindow.Presenter;
                appWindow.Move(new PointInt32 { X = left, Y = top });
                presenter.Maximize();
            }
            else
            {
            appWindow.Move(new PointInt32 { X = left, Y = top });
            appWindow.Resize(new SizeInt32
            {
                Width = (int)Math.Truncate(width * InitialScaleFactor),
                Height = (int)Math.Truncate(height * InitialScaleFactor)
            });

            }
        }

        private void SaveWindowSizePos(Window window)
        {
            var appData = ApplicationData.Current.LocalSettings;
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            var size = appWindow.Size;
            var position = appWindow.Position;

            TSettings.WindowLeft = position.X;
            TSettings.WindowTop = position.Y;
            TSettings.WindowWidth = size.Width;
            TSettings.WindowHeight = size.Height;
            TSettings.WindowScale = grdMain.XamlRoot.RasterizationScale;

            OverlappedPresenter presenter = (OverlappedPresenter)appWindow.Presenter;
            TSettings.IsMaximized = (presenter.State == OverlappedPresenterState.Maximized);
        }

    }

}
