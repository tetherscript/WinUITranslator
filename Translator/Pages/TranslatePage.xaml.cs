using Microsoft.UI.Xaml.Controls;

namespace Translator
{
    public sealed partial class TranslatePage : Page
    {
        private readonly TVm _vm = App.Vm;
        public TVm Vm { get => _vm; }

        public TranslatePage()
        {
            this.InitializeComponent();
        }
    }
}
