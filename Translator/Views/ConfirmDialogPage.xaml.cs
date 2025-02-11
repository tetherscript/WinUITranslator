using Microsoft.UI.Xaml.Controls;

namespace Translator
{

    public sealed partial class ConfirmDialogPage : Page
    {
        public ConfirmDialogPage(string message)
        {
            this.InitializeComponent();
            tbMessage.Text = message;
        }
    }
}
