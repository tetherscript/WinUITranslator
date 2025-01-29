using Microsoft.UI.Xaml.Controls;

namespace Translator
{
    public sealed partial class TranslationFunctionsPage : Page
    {
        private readonly TVm _vm = App.Vm;
        public TVm Vm { get => _vm; }

        public TranslationFunctionsPage()
        {
            this.InitializeComponent();
        }

    }
}
