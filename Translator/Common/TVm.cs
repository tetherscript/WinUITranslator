using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
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
        private bool _navIsPaneOpen;

        [ObservableProperty]
        private bool _debug;
        partial void OnDebugChanged(bool oldValue, bool newValue)
        {
            TSettings.Debug = newValue;
        }

        [ObservableProperty]
        private bool _isBusy = false;

        [ObservableProperty]
        private string _target;

        #region SCAN
        [ObservableProperty]
        int _scanLogSelectionStart = 0;

        [ObservableProperty]
        int _scanLogSelectionLength = 0;
        
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
        partial void OnTFSettingsChanged(string value)
        {
            TFSettingsModified = true;
        }

        [ObservableProperty]
        private bool _tFSettingsModified;


        [RelayCommand]
        private void TFTestLoadSettings()
        {
            TUtils.CalcPaths(Target);
            string path = TTransFunc.GetSettingsPath(SelectedTranslationFunction);
            try
            {
                string loadedJson = File.ReadAllText(path);
                TLog.Log(TLog.eMode.tfTranslate, TLog.eLogItemType.inf, 0, "Loaded settings: " + path);
                TFSettings = loadedJson;
                TFSettingsModified = false;
            }
            catch (Exception ex)
            {
                TLog.Log(TLog.eMode.tfTranslate, TLog.eLogItemType.err, 0, "Load setting failed: " + path + ": " + ex.Message);
            }
        }

        [RelayCommand]
        private void TFTestSaveSettings()
        {
            TUtils.CalcPaths(Target);
            string path = TTransFunc.GetSettingsPath(SelectedTranslationFunction);
            try
            {
                File.WriteAllText(path, TFSettings);
                TLog.Log(TLog.eMode.tfTranslate, TLog.eLogItemType.inf, 0, "Saved settings: " + path);
                TFSettingsModified = false;
            }
            catch (Exception ex)
            {
                TLog.Log(TLog.eMode.tfTranslate, TLog.eLogItemType.err, 0, "Save setting failed: " + path + ": " + ex.Message);
            }
        }

        [ObservableProperty]
        private string _tFToCulture = "de-DE";

        [RelayCommand]
        private async Task StartTFTest()
        {
            TLog.Reset(TLog.eMode.tfTranslate);
            if (TFTextToTranslate.Trim() == "")
            {
                TLog.Log(TLog.eMode.tfTranslate, TLog.eLogItemType.err, 0, "Enter text to translate.");
            }
            else 
            { 
                if (TUtils.CalcPaths(Target))
                {
                    IsBusy = true;
                    TFIsTranslating = true;
                    TFLog = "";
                    await TTranslate.StartTest(TLog.eMode.tfTranslate, TUtils.TargetRootPath, 
                        SelectedTranslationFunction, TFTextToTranslate, TFToCulture);
                }
                else
                {
                    TLog.Log(TLog.eMode.tfTranslate, TLog.eLogItemType.err, 0, "Target root path does not exist: " + Target);
                }
                TFTranslateLogScrollToBottom();
                TFIsTranslating = false;
                IsBusy = false;
            }
        }

        private void TFTranslateLogScrollToBottom()
        {
            TFLogSelectionStart = TFLog.Length;
            TFLogSelectionLength = 0;
        }

        [ObservableProperty]
        private int _tFTranslationFunctionIndex;

        [ObservableProperty]
        private string _tFTextToTranslate = "@Aperture";

        [ObservableProperty]
        private bool _tFIsTranslating;

        [ObservableProperty]
        private string _tFLog;


        #endregion

    }

}
