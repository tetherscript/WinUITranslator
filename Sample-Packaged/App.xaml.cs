using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Threading;

namespace Sample_Packaged
{
    public partial class App : Application
    {
        public static bool IsRTL = false;

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

        public App()
        {
            this.InitializeComponent();

            //In this Packaged app, you could have the user select the language in the sample app UI, save it, 
            //  restart the app, then based on that setting choose the PrimaryLanguageOverride.
            //  The user has to have the language installed on the computer for this to work.
            //If you didn't use the PrimaryLanguageOverride, the language used would be the one
            //  currently used by the computer.  

            //We'll set the language PrimaryLanguageOverride here for testing. 
            //This is better than having to change system language and log out/in for changes to be seen in this app.
            //Comment out the remainder of this function if you want to change system language and log out/in to test.
            //try ar-SA for RTL testing
            //******* 
            string lang = "de-DE"; //en-US  de-DE  fr-FR  ar-SA
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = lang;

            // **Synchronize .NET CultureInfo with PrimaryLanguageOverride**
            var culture = new CultureInfo(lang);

            // Set the default culture for new threads
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            // Optionally, set the culture for the current thread
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            //*******
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var currentUICulture = CultureInfo.CurrentUICulture;
            IsRTL = currentUICulture.TextInfo.IsRightToLeft;

            if (IsRTL)
            {
                // Example: Set process default layout to RTL (the maximize, restore buttons etc)
                //   but we'll still need to set the FlowDirection of the root xaml element.
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
