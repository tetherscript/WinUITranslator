using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Translator
{
    public partial class TVm(string[] arguments) : ObservableObject
    {
        public class TTransFuncName(string display, string name)
        {
            public string Display { get; set; } = display;
            public string Value { get; set; } = name;
        }

        private nint _mainWindowHandle;
        public nint MainWindowHandle
        {
            get { return _mainWindowHandle; }
            set
            {
                _mainWindowHandle = value;
            }
        }

        private string[] _arguments = arguments;
        public string[] Arguments { get => _arguments; set => _arguments = value; }

        [ObservableProperty]
        private string _title = "Translator";

        [ObservableProperty]
        private ElementTheme _theme;

        [ObservableProperty]
        private int _themeIndex;
        partial void OnThemeIndexChanged(int value)
        {
            if (value != -1)
            {
                Theme = (ElementTheme)ThemeIndex;
            }
        }

        [ObservableProperty]
        private bool _debug;
        partial void OnDebugChanged(bool oldValue, bool newValue)
        {
            TSettings.Debug = newValue;
        }

        [ObservableProperty]
        private bool _debugRetranslate;
        partial void OnDebugRetranslateChanged(bool oldValue, bool newValue)
        {
            TSettings.DebugRetranslate = newValue;
        }

        [ObservableProperty]
        private int _debugRetranslateItemsCount;
        partial void OnDebugRetranslateItemsCountChanged(int oldValue, int newValue)
        {
            TSettings.DebugRetranslateItemsCount = newValue;
        }

        [ObservableProperty]
        private bool _isBusy = false;

        [ObservableProperty]
        private string _target;

        #region SCAN
        [RelayCommand]
        private async Task StartScan()
        {
            TLog.Reset();
            TLog.Mode = TLog.eMode.scan;
            if (TUtils.CalcPaths(Target))
            {
                IsBusy = true;
                IsScanning = true;
                ScanLog = "";
                await Task.Delay(1000);
                await TScan.Start(TUtils.TargetRootPath);
            }
            else
            {
                TLog.LogExt("Target root path does not exist: " + Target, true);
            }
            IsScanning = false;
            IsBusy = false;
        }

        [ObservableProperty]
        private bool _isScanning;

        [ObservableProperty]
        private string _scanLog;
        #endregion

        #region TRANSLATE
        [ObservableProperty]
        List<TTransFuncName> _translationFunctions = new();

        [ObservableProperty]
        private string _selectedTranslationFunction;

        [RelayCommand]
        private async Task StartTranslate()
        {
            TLog.Mode = TLog.eMode.translate;
            TLog.Reset();
            if (TUtils.CalcPaths(Target))
            {
                IsBusy = true;
                IsTranslating = true;
                TranslateLog = "";
                await Task.Delay(1000);
                await TTranslate.Start(TUtils.TargetRootPath, SelectedTranslationFunction);
            }
            else
            {
                TLog.LogExt("Target root path does not exist: " + Target, true);
            }
            IsTranslating = false;
            IsBusy = false;
        }

        [RelayCommand]
        private void CancelTranslate()
        {
            TTranslate.Stop();
        }

        [ObservableProperty]
        private int _translationFunctionIndex;

        [ObservableProperty]
        private bool _isTranslating;

        [ObservableProperty]
        private string _translateLog;

        [ObservableProperty]
        private int _translateProgress = 0;

        #endregion

    }

}
