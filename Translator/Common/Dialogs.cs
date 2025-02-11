using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace Translator
{
    public static class Dialogs
    {
        public static async Task<ContentDialogResult> ShowConfirmation(FlowDirection flowDirection, ElementTheme theme, XamlRoot xamlRoot, string title, string message)
        {
            ContentDialog dialog = new();
            dialog.FlowDirection = flowDirection;
            dialog.XamlRoot = xamlRoot;
            dialog.RequestedTheme = theme;
            dialog.Title = title;
            dialog.PrimaryButtonText = "Yes";
            dialog.SecondaryButtonText = "No";
            dialog.DefaultButton = ContentDialogButton.Secondary;
            dialog.Content = new ConfirmDialogPage(message);
            ContentDialogResult res = new();
            res = await dialog.ShowAsync();
            return res;
        }
    }


}
