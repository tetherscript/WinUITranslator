﻿using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using System;
using Windows.UI.Core;

namespace Translator
{
    public partial class App : Application
    {
        private static MainWIndowVm _vm;
        public static MainWIndowVm Vm { get => _vm; }

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
            _vm = new MainWIndowVm();
            _vm.KeyLeftControlPressedOnLaunch = InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.LeftControl).HasFlag(CoreVirtualKeyStates.Down);

            m_window = new MainWindow();
            m_window.Activate();
        }

        public static Window m_window;
    }
}
