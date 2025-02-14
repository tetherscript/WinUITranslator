using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;

namespace Translator
{

    public partial class MainWIndowVm : ObservableObject
    {
        [ObservableProperty]
        private bool _keyLeftControlPressedOnLaunch = false;

        [ObservableProperty]
        private string _title = "Translator " + string.Format("{0:d}.{1:d}.{2:d}.{3:d}", Package.Current.Id.Version.Major,
                    Package.Current.Id.Version.Minor, Package.Current.Id.Version.Revision, Package.Current.Id.Version.Build);

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


