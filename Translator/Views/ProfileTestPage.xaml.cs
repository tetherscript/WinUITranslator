using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Translator
{

    public sealed partial class ProfileTestPage : Page
    {
        private readonly TVm _vm = App.Vm;
        public TVm Vm { get => _vm; }

        public ProfileTestPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            WeakReferenceMessenger.Default.Register<TProfileSelected>(this, (r, m) =>
            {
                App.Vm.ProfileTestLogItems.Clear();
            });
        }
    }
}
