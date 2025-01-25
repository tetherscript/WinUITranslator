using Microsoft.UI.Xaml;
using System;

namespace Translator
{

    public partial class App : Application
    {

        public static bool IsRTL = false;

        public App()
        {
            this.InitializeComponent();
            this.FocusVisualKind = FocusVisualKind.Reveal;
            this.UnhandledException += App_UnhandledException;
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Console.WriteLine($"Unhandled exception: {e.Exception.Message}");
            e.Handled = true;
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }

        private Window m_window;
    }
}
