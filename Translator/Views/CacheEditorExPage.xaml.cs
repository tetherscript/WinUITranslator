using Microsoft.UI.Xaml.Controls;

namespace Translator
{
    public sealed partial class CacheEditorExPage : Page
    {
        public CacheEditorPageExVm Vm = new();

        public CacheEditorExPage()
        {
            this.InitializeComponent();
        }

        private void ListView_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            Vm.EditSelected();
        }
    }

}
