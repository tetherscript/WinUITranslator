using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

namespace Translator
{
    public sealed partial class MainPage : Page
    {
        public MainPageVm Vm;

        public class NavHeaderData
        {
            public Symbol NavIcon { get; set; }
            public string NavLabel { get; set; }
        }

        public MainPage()
        {
            this.InitializeComponent();
            DataContext = this;
            Vm = new();
            //PopulateNavigationViewHeader(Symbol.Home, "Home");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
           // GotoPage(TSettings.LastNavItemTag);
        }

        private void NvMain_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected == true)
            {
                //GotoPage("Settings");
            }
            else
            {
                NavigationViewItem item = (NavigationViewItem)args.SelectedItem;
                //GotoPage(item.Tag.ToString());
            }
        }

        private void PopulateNavigationViewHeader(Symbol icon, string label)
        {
            var headerData = new NavHeaderData
            {
                NavIcon = icon,
                NavLabel = label
            };
            //nvMain.Header = headerData;
        }

        private void SetActiveNavItem(NavigationViewItem item)
        {
            //if ((NavigationViewItem)nvMain.SelectedItem != item)
            //{
            //    //nvMain.SelectedItem = item;
            //}
        }

        private string _lastTag = string.Empty;
        //public void GotoPage(string tag)
        //{
        //    if (tag == _lastTag) { return; }
        //    TSettings.LastNavItemTag = tag;
        //    _lastTag = tag;
        //    switch (tag)
        //    {
        //        case "Target":
        //            SetActiveNavItem(nviTarget);
        //            PopulateNavigationViewHeader(Symbol.Home, "1. Target a Project");
        //            frMain.Navigate(typeof(TargetPage), new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        //            break;

        //        case "Scan":
        //            SetActiveNavItem(nviScan);
        //            PopulateNavigationViewHeader(Symbol.Play, "2. Scan Target Project");
        //            frMain.Navigate(typeof(ScanPage), new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        //            break;

        //        case "Translate":
        //            SetActiveNavItem(nviTranslate);
        //            PopulateNavigationViewHeader(Symbol.Globe, "3. Translate Scan Results");
        //            frMain.Navigate(typeof(TranslatePage), new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        //            break;

        //        case "ProfileEditor":
        //            SetActiveNavItem(nviProfileEditor);
        //            PopulateNavigationViewHeader(Symbol.Bookmarks, "Profile Editor");
        //            frMain.Navigate(typeof(ProfilePage), new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        //            break;

        //        case "CacheExplorer":
        //            SetActiveNavItem(nviCacheExplorer);
        //            PopulateNavigationViewHeader(Symbol.Edit, "Cache Explorer");
        //            frMain.Navigate(typeof(CacheEditorPage), new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        //            break;

        //        case "Settings":
        //            PopulateNavigationViewHeader(Symbol.Setting, "Settings");
        //            frMain.Navigate(typeof(SettingsPage), new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
        //            break;
        //    }
        //}

    }
}
