using Microsoft.UI.Xaml.Controls;

namespace Translator
{
    public sealed partial class ProfileSettingPage : Page
    {
        public TProfileSettingPageVm Vm = new();

        public ProfileSettingPage()
        {
            this.InitializeComponent();
        }
    }

}
        