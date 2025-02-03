using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace Translator
{
    public sealed partial class TTF_OpenAI_1_SettingsPage : Page
    {
        private readonly TVm _vm = App.Vm;
        public TVm Vm { get => _vm; }

        public TTF_OpenAI_1_SettingsPage()
        {
            this.InitializeComponent();
        }
    }
}
