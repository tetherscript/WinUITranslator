using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml.Media.Animation;
using System.Runtime.CompilerServices;

namespace Translator
{
    public sealed partial class MainPage : Page
    {
        private readonly TVm _vm = App.Vm;
        public TVm Vm { get => _vm; }

        public class NavHeaderData
        {
            public Symbol NavIcon { get; set; }
            public string NavLabel { get; set; }
        }

        public MainPage()
        {
            this.InitializeComponent();
            DataContext = this;

            PopulateNavigationViewHeader(Symbol.Home, "Home");

            


        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            GotoPage(TSettings.LastNavItemTag);
        }

        private void NvMain_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected == true)
            {
                GotoPage("Settings");
                //frMain.Navigate(typeof(SettingsPage), args.RecommendedNavigationTransitionInfo);
            }
            else
            {
                NavigationViewItem item = (NavigationViewItem)args.SelectedItem;
                GotoPage(item.Tag.ToString());
            }
        }

        private void PopulateNavigationViewHeader(Symbol icon, string label)
        {
            var headerData = new NavHeaderData
            {
                NavIcon = icon,
                NavLabel = label
            };
            nvMain.Header = headerData;
        }

        private void SetActiveNavItem(NavigationViewItem item)
        {
            if ((NavigationViewItem)nvMain.SelectedItem != item)
            {
                nvMain.SelectedItem = item;
            }
        }

        public void GotoPage(string tag)
        {
            TSettings.LastNavItemTag = tag;
            switch (tag)
            {
                case "Target":
                    SetActiveNavItem(nviTarget);
                    PopulateNavigationViewHeader(Symbol.Folder, "1. Target a Project");
                    frMain.Navigate(typeof(TargetPage), new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
                    break;
            }
            switch (tag)
            {
                case "Scan":
                    SetActiveNavItem(nviScan);
                    PopulateNavigationViewHeader(Symbol.Play, "2. Scan Target Project");
                    frMain.Navigate(typeof(ScanPage), new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
                    break;
            }
            switch (tag)
            {
                case "Translate":
                    SetActiveNavItem(nviTranslate);
                    PopulateNavigationViewHeader(Symbol.Globe, "3. Translate Scan Results");
                    frMain.Navigate(typeof(TranslatePage), new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
                    break;
            }

            switch (tag)
            {
                case "Settings":
                    PopulateNavigationViewHeader(Symbol.Help, "Introduction");
                    frMain.Navigate(typeof(SettingsPage), new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
                    break;
            }

        }

    }
}
