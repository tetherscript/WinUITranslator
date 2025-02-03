using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Translator
{
    public sealed partial class TFPage : Page
    {
        private readonly TVm _vm = App.Vm;
        public TVm Vm { get => _vm; }

        public TFPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            GotoPage(TSettings.TFLastSelectorBarItemTag);
        }

        private void SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            SelectorBarItem item = (SelectorBarItem)sender.SelectedItem;
            string tag = item.Tag.ToString();
            GotoPage(tag);
        }

        private void GotoPage(string tag)
        {
            switch (tag)
            {
                case "Test":
                    frContent.Navigate(typeof(TFTestPage), new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
                    if (!sbiTest.IsSelected) { sbiTest.IsSelected = true; }
                    TSettings.TFLastSelectorBarItemTag = tag;
                    break;
                case "Settings":
                    frContent.Navigate(typeof(TFSettingsPage), new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
                    if (!sbiSettings.IsSelected) { sbiSettings.IsSelected = true; }
                    TSettings.TFLastSelectorBarItemTag = tag;
                    break;
                default: break;
            }
        }

        private void SbTabs_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {

        }
    }
}
