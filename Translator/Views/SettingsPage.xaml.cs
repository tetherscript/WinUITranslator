using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;

namespace Translator
{
    public sealed partial class SettingsPage : Page
    {
        private readonly TVm _vm = App.Vm;
        public TVm Vm { get => _vm; }

        public SettingsPage()
        {
            this.InitializeComponent();
        }

    }
}
