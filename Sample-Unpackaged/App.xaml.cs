using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Sample_Unpackaged
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();

            //In this Unpackaged app, you need to set the system language, logout, then log back in. 
            //Then load this project in Visual Studio and run it.  It will show the translations for
            //that language.

            //there is no way to do this without logging out/in like in the packaged app.

            //you can test RTL without logging out/in by setting IsRTL = true in the OnLaunched() below.

            //if you are doing alot of translation tweaks, best way is to change the system language
            //  to ex. de-DE, logout/login, and start debugging your app in Visual Studio.
            //  Run the Translator App and scan and translate.  Run the app again to see the changes.
            //  Rinse, repeat.  When it looks good in de-DE, check the other languages.
            //  Be sure to try a RTL language like ar-SA too.

            //when you are done with translating for now, you can always switch back to your main
            //language and continue development there.  
        }

        public static class NativeMethods
        {
            // https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-setprocessdefaultlayout

            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetProcessDefaultLayout(uint dwDefaultLayout);

            // Layout flags:
            public const uint LAYOUT_RTL = 0x00000001;
            public const uint LAYOUT_BITMAPORIENTATIONPRESERVED = 0x00000008;
        }

        public static bool IsRTL;
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            var currentUICulture = CultureInfo.CurrentUICulture;
            IsRTL = currentUICulture.TextInfo.IsRightToLeft;
            //IsRTL = true;
            if (IsRTL)
            {
                // Example: Set process default layout to RTL
                bool success = NativeMethods.SetProcessDefaultLayout(NativeMethods.LAYOUT_RTL);
                if (!success)
                {
                    int errorCode = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                    // Handle error as needed
                }
            }

            m_window = new MainWindow();
            m_window.Activate();
        }

        private Window m_window;
    }
}
