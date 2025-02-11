using Microsoft.UI.Xaml.Controls;

namespace Translator
{
    public sealed partial class ScanPage : Page
    {
        public ScanPageVm Vm = new();

        public ScanPage()
        {
            this.InitializeComponent();
        }
    }
}
