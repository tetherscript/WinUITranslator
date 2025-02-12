using Microsoft.UI.Xaml.Controls;

namespace Translator
{
    public sealed partial class CacheEditorPage : Page
    {
        public CacheEditorPageVm Vm = new();

        public CacheEditorPage()
        {
            this.InitializeComponent();
        }

    }
}
