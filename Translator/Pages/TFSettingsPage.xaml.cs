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
using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Translator
{

    public class NavigateMessage
    {
        public Type PageType { get; }
        public object Parameter { get; }

        public NavigateMessage(Type pageType, object parameter = null)
        {
            PageType = pageType;
            Parameter = parameter;
        }
    }

    public sealed partial class TFSettingsPage : Page
    {
        private readonly TVm _vm = App.Vm;
        public TVm Vm { get => _vm; }

        public TFSettingsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            WeakReferenceMessenger.Default.UnregisterAll(this);
            WeakReferenceMessenger.Default.Register<NavigateMessage>(this, async (r, m) =>
            {
                if (m.PageType != null)
                {
                    if (frTFSettings != null)
                    {
                        if (frTFSettings.CurrentSourcePageType != m.PageType)
                        {
                            frTFSettings.Navigate(m.PageType);
                        }
                    }
                }
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }

        private void FrTFSettings_Loaded(object sender, RoutedEventArgs e)
        {
            TTransFunc.LoadSettingsPage(App.Vm.SelectedTranslationFunction);
        }


    }

}
        