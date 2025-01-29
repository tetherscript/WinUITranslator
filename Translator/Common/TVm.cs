using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.VoiceCommands;

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
        int _scanLogSelectionStart = 0;

        [ObservableProperty]
        int _scanLogSelectionLength = 0;

        [ObservableProperty]
        private string _target;

        #region SCAN
        [RelayCommand]
        private async Task StartScan()
        {
            TLog.Reset(TLog.eMode.scan);
            if (TUtils.CalcPaths(Target))
            {
                IsBusy = true;
                IsScanning = true;
                ScanLog = "";
                ScanLogScrollToBottom();
                await Task.Delay(1000);
                await TScan.Start(TLog.eMode.scan, TUtils.TargetRootPath);
            }
            else
            {
                TLog.Log(TLog.eMode.scan, TLog.eLogItemType.err, 0, "Target root path does not exist: " + Target);
            }
            ScanLogScrollToBottom();
            IsScanning = false;
            IsBusy = false;
        }

        private void ScanLogScrollToBottom()
        {
            ScanLogSelectionStart = ScanLog.Length;
            ScanLogSelectionLength = 0;
        }

        [ObservableProperty]
        private bool _isScanning;

        [ObservableProperty]
        private string _scanLog;
        #endregion

        #region TRANSLATE

        [ObservableProperty]
        int _transLogSelectionStart = 0;

        [ObservableProperty]
        int _transLogSelectionLength = 0;


        [ObservableProperty]
        List<TTransFuncName> _translationFunctions = new();

        [ObservableProperty]
        private string _selectedTranslationFunction;

        [RelayCommand]
        private async Task StartTranslate()
        {
            TLog.Reset(TLog.eMode.translate);
            if (TUtils.CalcPaths(Target))
            {
                IsBusy = true;
                IsTranslating = true;
                TranslateLog = "";
                TranslateLogScrollToBottom();
                await Task.Delay(1000);
                await TTranslate.Start(TLog.eMode.translate, TUtils.TargetRootPath, SelectedTranslationFunction);
            }
            else
            {
                TLog.Log(TLog.eMode.translate, TLog.eLogItemType.err, 0, "Target root path does not exist: " + Target);
            }
            TranslateLogScrollToBottom();
            TranslateProgress = 0;
            IsTranslating = false;
            IsBusy = false;
        }

        [RelayCommand]
        private void CancelTranslate()
        {
            TTranslate.Stop();
        }

        private void TranslateLogScrollToBottom()
        {
            TransLogSelectionStart = TranslateLog.Length;
            TransLogSelectionLength = 0;
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


























        #region TRANSLATION FUNCTIONS

        [ObservableProperty]
        int _tFLogSelectionStart = 0;

        [ObservableProperty]
        int _tFLogSelectionLength = 0;

        [ObservableProperty]
        private string _tFSelectedTranslationFunction;


        [ObservableProperty]
        private string _tFSettings;

        [RelayCommand]
        private async Task TFTestSLoadSettings()
        {
            TLog.Log(TLog.eMode.tfTranslate, TLog.eLogItemType.inf, 0, "Loaded settings.");
        }

        [RelayCommand]
        private async Task TFTestSaveSettings()
        {
            TLog.Log(TLog.eMode.tfTranslate, TLog.eLogItemType.inf, 0, "Saved settings.");
        }



        [RelayCommand]
        private async Task StartTFTest()
        {
            TLog.Reset(TLog.eMode.tfTranslate);
            if (TUtils.CalcPaths(Target))
            {
                IsBusy = true;
                TFIsTranslating = true;
                TFLog = "";
                await Task.Delay(1000);
                await TTranslate.StartTest(TLog.eMode.tfTranslate, TUtils.TargetRootPath, SelectedTranslationFunction, TFTextToTranslate);
            }
            else
            {
                TLog.Log(TLog.eMode.tfTranslate, TLog.eLogItemType.err, 0, "Target root path does not exist: " + Target);
            }
            TranslateLogScrollToBottom();
            TFIsTranslating = false;
            IsBusy = false;
        }

        private void TFTranslateLogScrollToBottom()
        {
            TransLogSelectionStart = TranslateLog.Length;
            TransLogSelectionLength = 0;
        }

        [ObservableProperty]
        private int _tFTranslationFunctionIndex;

        [ObservableProperty]
        private string _tFTextToTranslate;

        [ObservableProperty]
        private bool _tFIsTranslating;

        [ObservableProperty]
        private string _tFLog;


        #endregion









    }

}
