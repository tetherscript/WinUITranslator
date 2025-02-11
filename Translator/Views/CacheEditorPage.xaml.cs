using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Translator
{
    public sealed partial class CacheEditorPage : Page
    {
        public CacheEditorPageVm Vm = new();

        public CacheEditorPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

    }
}
