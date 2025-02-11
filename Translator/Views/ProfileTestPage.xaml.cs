using Microsoft.UI.Xaml.Controls;

namespace Translator
{
    public sealed partial class ProfileTestPage : Page
    {
        public TProfileTestPageVm Vm = new();

        public ProfileTestPage()
        {
            this.InitializeComponent();
        }
    }
}
