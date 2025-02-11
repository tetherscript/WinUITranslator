using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Threading.Tasks;

namespace Translator
{
    public sealed partial class CacheEditorPage : Page
    {
        private readonly TVm _vm = App.Vm;
        public TVm Vm { get => _vm; }

        public CacheEditorPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Vm.LoadCache();
            await Task.Delay(500);
            SearchBox.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
            //Vm.CacheSearchSelectionStart = Vm.SearchText.Length;
            //Vm.CacheSearchSelectionLength = 0;

            Vm.CacheSearchSelectionLength = Vm.SearchText.Length;
            Vm.CacheSearchSelectionStart = 0;

            WeakReferenceMessenger.Default.Register<TTargetChanged>(this, (r, m) =>
            {
                Vm.LoadCache();
            });

        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            
        }

    }
}
