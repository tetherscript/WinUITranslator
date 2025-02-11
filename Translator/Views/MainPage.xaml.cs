using Microsoft.UI.Xaml.Controls;

namespace Translator
{
    public sealed partial class MainPage : Page
    {
        public MainPageVm Vm = new();

        public MainPage()
        {
            this.InitializeComponent();
        }

    }
}
