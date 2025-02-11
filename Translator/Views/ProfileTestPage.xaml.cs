using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

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
