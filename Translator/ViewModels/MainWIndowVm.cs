using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;

namespace Translator
{

    public partial class MainWIndowVm : ObservableObject
    {
        [ObservableProperty]
        private bool _keyLeftControlPressedOnLaunch = false;

        [ObservableProperty]
        private string _title = "Translator";

        [ObservableProperty]
        private ElementTheme _theme;

        [ObservableProperty]
        private bool _isDarkTheme;
        partial void OnIsDarkThemeChanged(bool oldValue, bool newValue)
        {
            Theme = newValue ? ElementTheme.Dark : ElementTheme.Light;
        }
    }
}


