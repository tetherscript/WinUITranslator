using Microsoft.UI.Xaml.Controls;
namespace Translator
{
    public sealed partial class TargetPage : Page
    {
        private readonly TVm _vm = App.Vm;
        public TVm Vm { get => _vm; }

        public TargetPage()
        {
            this.InitializeComponent();
        }

    }

}
