using Microsoft.UI.Xaml.Controls;

namespace Translator
{
    public sealed partial class TranslatePage : Page
    {
        public TranslatePageVm Vm = new();

        public TranslatePage()
        {
            this.InitializeComponent();
        }

    }
}
