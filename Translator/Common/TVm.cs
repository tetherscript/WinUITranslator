using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;


namespace Translator
{
    public partial class Pair : ObservableObject
    {
        [ObservableProperty]
        private string key;

        [ObservableProperty]
        private string value;
    }

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
        private bool _keyLeftControlPressedOnLaunch = false;

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
                ScanLog = "Scanning...";
                ScanLogScrollToBottom();
                await Task.Delay(100);
                await TScan.Start(TLog.eMode.scan, TUtils.TargetRootPath);
                TLog.Save(TLog.eMode.scan, TUtils.TargetScanLogPath);
            }
            else
            {
                TLog.Log(TLog.eMode.scan, TLog.eLogItemType.err, 0, "Target root path does not exist: " + Target);
            }
            TLog.Flush(TLog.eMode.scan);
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
        partial void OnSelectedTranslationFunctionChanged(string value)
        {
            TFSettings = "";
            GoToTFSettingsPage();
        }

        [RelayCommand]
        private async Task StartTranslate()
        {
            TLog.Reset(TLog.eMode.translate);
            if (TUtils.CalcPaths(Target))
            {
                IsBusy = true;
                IsTranslating = true;
                TranslateLog = "Translating...";
                TranslateLogScrollToBottom();
                await Task.Delay(100);
                await TTranslate.Start(TLog.eMode.translate, TUtils.TargetRootPath, SelectedTranslationFunction);
            }
            else
            {
                TLog.Log(TLog.eMode.translate, TLog.eLogItemType.err, 0, "Target root path does not exist: " + Target);
            }
            TLog.Flush(TLog.eMode.translate);
            TranslateLogScrollToBottom();
            TranslateProgress = 0;
            IsTranslating = false;
            IsBusy = false;
        }

        [RelayCommand]
        private static void CancelTranslate()
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
        private int _tFLastSelectorBarIndex = 0;

        [ObservableProperty]
        int _tFLogSelectionStart = 0;

        [ObservableProperty]
        int _tFLogSelectionLength = 0;

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
            if (path == null)
            {
                TFSettings = "No settings file specified.";
            }
            else 
            { 
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
        }

        [RelayCommand]
        private async void TFTestSaveSettings()
        {
            TUtils.CalcPaths(Target);
            TLog.Reset(TLog.eMode.tfTranslate);
            string path = TTransFunc.GetSettingsPath(SelectedTranslationFunction);
            if (path == null) return;
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
            TLog.Flush(TLog.eMode.tfTranslate);
        }

        private void PopulateCultures()
        {

        var cultures = CultureInfo
             .GetCultures(CultureTypes.AllCultures)
             .Select(c => c.Name)
             .Where(name => !string.IsNullOrWhiteSpace(name)) // Filter out empty strings.
             .Distinct() // Ensure uniqueness.
             .OrderBy(name => name).ToList(); // Sort alphabetically.

            // Populate the observable collection.
            foreach (var culture in cultures)
            {
                CultureList.Add(culture);
            }

        }


        [RelayCommand]
        private async Task StartTFTest()
        {
            TFIsCancelling = false;
            TLog.Reset(TLog.eMode.tfTranslate);
            TranslateLog = "Translating...";
            await Task.Delay(100);
            if (TUtils.CalcPaths(Target))
            {
                IsBusy = true;
                TFIsTranslating = true;
                TFLog = "";
                await Task.Delay(10);
                await TFTranslateBatch();
            }
            else
            {
                TLog.Log(TLog.eMode.tfTranslate, TLog.eLogItemType.err, 0, "Target root path does not exist: " + Target);
            }
            TLog.Flush(TLog.eMode.tfTranslate);
            //TFTranslateLogScrollToBottom();
            TFIsTranslating = false;
            IsBusy = false;
        }

        private void TFTranslateLogScrollToBottom()
        {
            TFLogSelectionStart = TFLog.Length;
            TFLogSelectionLength = 0;
        }

        [ObservableProperty]
        private int _tFTranslationFunctionIndex;

        private async Task TFTranslateBatch()
        {
            TFLog1 = "";

            string[] lines = TFTextToTranslate.Split(new[] { "\r" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in lines)
            {
                TFLog1 = TFLog1 + item + Environment.NewLine;
                for (int i = 0; i < TFTestRepeats; i++)
                {
                    if (TFIsCancelling) return;
                    string trans = await TTranslate.StartTest(TLog.eMode.tfTranslate, TUtils.TargetRootPath,
                            SelectedTranslationFunction, item, TFToCulture);
                    int prefixLength = 1;
                    TFLog1 = TFLog1 + new string(' ', prefixLength) + trans + Environment.NewLine;
                    await Task.Delay(10);
                }
                TFLog1 = TFLog1 + Environment.NewLine;
            }
        }

        private async Task TFTestMatrixTranslate()
        {
            async Task Log1(string msg)
            {
                await Task.Delay(10);
                TFTestResult1 = TFTestResult1 + ((TFTestResult1 == null) ? null : Environment.NewLine) +msg;
            }

            async Task Log2(string msg)
            {
                await Task.Delay(10);
                TFTestResult2 = TFTestResult2 + ((TFTestResult2 == null) ? null : Environment.NewLine) + msg;
            }

            async Task Log3(string msg)
            {
                await Task.Delay(10);
                TFTestResult3 = TFTestResult3 + ((TFTestResult3 == null) ? null : Environment.NewLine) + msg;
            }

            async Task Log4(string msg)
            {
                await Task.Delay(10);
                TFTestResult4 = TFTestResult4 + ((TFTestResult4 == null) ? null : Environment.NewLine) + msg;
            }

            TFTestResult1 = null;
            TFTestResult2 = null;
            TFTestResult3 = null;
            TFTestResult4 = null;

            if (TFTest1Active)
            {
                if (TFTestHint1Active) 
                {
                    for (global::System.Int32 i = 0; i < TFTestRepeats; i++)
                    {
                        if (TFIsCancelling) return;
                        await Log1("@  --> " + await TTranslate.StartTest(TLog.eMode.tfTranslate, TUtils.TargetRootPath,
                                SelectedTranslationFunction, "@" + TFTextToTranslate1, TFToCulture));
                    }

                }

                if (TFTestHint2Active)
                {
                    for (global::System.Int32 i = 0; i < TFTestRepeats; i++)
                    {
                        if (TFIsCancelling) return;
                        await Log1("@@ --> " + await TTranslate.StartTest(TLog.eMode.tfTranslate, TUtils.TargetRootPath,
                        SelectedTranslationFunction, "@@" + TFTextToTranslate1, TFToCulture));
                    }
                }

                if (TFTestHint3Active)
                {
                    for (global::System.Int32 i = 0; i < TFTestRepeats; i++)
                    {
                        if (TFIsCancelling) return;
                        await Log1("!  --> " + await TTranslate.StartTest(TLog.eMode.tfTranslate, TUtils.TargetRootPath,
                        SelectedTranslationFunction, "!" + TFTextToTranslate1, TFToCulture));
                    }
                }

                if (TFTestHint4Active)
                {
                    for (global::System.Int32 i = 0; i < TFTestRepeats; i++)
                    {
                        if (TFIsCancelling) return;
                        await Log1("!! --> " + await TTranslate.StartTest(TLog.eMode.tfTranslate, TUtils.TargetRootPath,
                        SelectedTranslationFunction, "!!" + TFTextToTranslate1, TFToCulture));
                    }
                }
            }

            if (TFTest2Active)
            {
                if (TFTestHint2Active)
                {
                    for (global::System.Int32 i = 0; i < TFTestRepeats; i++)
                    {
                        if (TFIsCancelling) return;
                        await Log2("@  --> " + await TTranslate.StartTest(TLog.eMode.tfTranslate, TUtils.TargetRootPath,
                        SelectedTranslationFunction, "@" + TFTextToTranslate2, TFToCulture));
                    }
                }

                if (TFTestHint2Active)
                {
                    for (global::System.Int32 i = 0; i < TFTestRepeats; i++)
                    {
                        if (TFIsCancelling) return;
                        await Log2("@@ --> " + await TTranslate.StartTest(TLog.eMode.tfTranslate, TUtils.TargetRootPath,
                        SelectedTranslationFunction, "@@" + TFTextToTranslate2, TFToCulture));
                    }
                }

                if (TFTestHint3Active)
                {
                    {
                        if (TFIsCancelling) return;
                        for (global::System.Int32 i = 0; i < TFTestRepeats; i++)
                            await Log2("!  --> " + await TTranslate.StartTest(TLog.eMode.tfTranslate, TUtils.TargetRootPath,
                            SelectedTranslationFunction, "!" + TFTextToTranslate2, TFToCulture));
                    }
                }

                if (TFTestHint4Active)
                {
                    {
                        if (TFIsCancelling) return;
                        for (global::System.Int32 i = 0; i < TFTestRepeats; i++)
                            await Log2("!! --> " + await TTranslate.StartTest(TLog.eMode.tfTranslate, TUtils.TargetRootPath,
                            SelectedTranslationFunction, "!!" + TFTextToTranslate2, TFToCulture));
                    }
                }
            }

            if (TFTest3Active)
            {
                if (TFTestHint3Active)
                {
                    for (global::System.Int32 i = 0; i < TFTestRepeats; i++)
                    {
                        if (TFIsCancelling) return;
                        await Log3("@  --> " + await TTranslate.StartTest(TLog.eMode.tfTranslate, TUtils.TargetRootPath,
                        SelectedTranslationFunction, "@" + TFTextToTranslate3, TFToCulture));
                    }
                }

                if (TFTestHint3Active)
                {
                    for (global::System.Int32 i = 0; i < TFTestRepeats; i++)
                    {
                        if (TFIsCancelling) return;
                        await Log3("@@ --> " + await TTranslate.StartTest(TLog.eMode.tfTranslate, TUtils.TargetRootPath,
                        SelectedTranslationFunction, "@@" + TFTextToTranslate3, TFToCulture));
                    }
                }

                if (TFTestHint3Active)
                {
                    for (global::System.Int32 i = 0; i < TFTestRepeats; i++)
                    {
                        if (TFIsCancelling) return;
                        await Log3("!  --> " + await TTranslate.StartTest(TLog.eMode.tfTranslate, TUtils.TargetRootPath,
                        SelectedTranslationFunction, "!" + TFTextToTranslate3, TFToCulture));
                    }
                }

                if (TFTestHint4Active)
                {
                    for (global::System.Int32 i = 0; i < TFTestRepeats; i++)
                    {
                        if (TFIsCancelling) return;
                        await Log3("!! --> " + await TTranslate.StartTest(TLog.eMode.tfTranslate, TUtils.TargetRootPath,
                        SelectedTranslationFunction, "!!" + TFTextToTranslate3, TFToCulture));
                    }
                }

            }

            if (TFTest4Active)
            {
                if (TFTestHint4Active)
                {
                    for (global::System.Int32 i = 0; i < TFTestRepeats; i++)
                    {
                        if (TFIsCancelling) return;
                        await Log4("@  --> " + await TTranslate.StartTest(TLog.eMode.tfTranslate, TUtils.TargetRootPath,
                        SelectedTranslationFunction, "@" + TFTextToTranslate4, TFToCulture));
                    }
                }

                if (TFTestHint4Active)
                {
                    for (global::System.Int32 i = 0; i < TFTestRepeats; i++)
                    {
                        if (TFIsCancelling) return;
                        await Log4("@@ --> " + await TTranslate.StartTest(TLog.eMode.tfTranslate, TUtils.TargetRootPath,
                        SelectedTranslationFunction, "@@" + TFTextToTranslate4, TFToCulture));
                    }
                }

                if (TFTestHint4Active)
                {
                    for (global::System.Int32 i = 0; i < TFTestRepeats; i++)
                    {
                        if (TFIsCancelling) return;
                        await Log4("!  --> " + await TTranslate.StartTest(TLog.eMode.tfTranslate, TUtils.TargetRootPath,
                        SelectedTranslationFunction, "!" + TFTextToTranslate4, TFToCulture));
                    }
                }

                if (TFTestHint4Active)
                {
                    for (global::System.Int32 i = 0; i < TFTestRepeats; i++)
                    {
                        if (TFIsCancelling) return;
                        await Log4("!! --> " + await TTranslate.StartTest(TLog.eMode.tfTranslate, TUtils.TargetRootPath,
                        SelectedTranslationFunction, "!!" + TFTextToTranslate4, TFToCulture));
                    }
                }
            }

        }

        [ObservableProperty]
        private string _tFToCulture = "de-DE";

        [ObservableProperty]
        private int _tFTestRepeats = 1;

        [ObservableProperty]
        private string _tFTextToTranslate = "Light";

        [ObservableProperty]
        private string _tFTestResult = null;

        public void GoToTFSettingsPage()
        {
            TTransFunc.LoadSettingsPage(SelectedTranslationFunction);
        }



        [ObservableProperty]
        private string _tFTextToTranslate1 = "Light";

        [ObservableProperty]
        private string _tFTextToTranslate2 = "Select a profile folder";

        [ObservableProperty]
        private string _tFTextToTranslate3 = "Shutter Speed";

        [ObservableProperty]
        private string _tFTextToTranslate4 = "Select a white balance.";

        [ObservableProperty]
        private bool _tFTest1Active = true;

        [ObservableProperty]
        private bool _tFTest2Active = true;

        [ObservableProperty]
        private bool _tFTest3Active = false;

        [ObservableProperty]
        private bool _tFTest4Active = false;

        [ObservableProperty]
        private bool _tFTestHint1Active = true;

        [ObservableProperty]
        private bool _tFTestHint2Active = true;

        [ObservableProperty]
        private bool _tFTestHint3Active = false;

        [ObservableProperty]
        private bool _tFTestHint4Active = false;

        [ObservableProperty]
        private string _tFTestResult1 = null;

        [ObservableProperty]
        private string _tFTestResult2 = null;

        [ObservableProperty]
        private string _tFTestResult3 = null;

        [ObservableProperty]
        private string _tFTestResult4 = null;

        [ObservableProperty]
        private bool _tFIsTranslating;

        [ObservableProperty]
        private string _tFLog;

        [ObservableProperty]
        private string _tFLog1;

        [ObservableProperty]
        private List<string> _cultureList = CultureInfo
             .GetCultures(CultureTypes.AllCultures)
             .Select(c => c.Name)
             .Where(name => !string.IsNullOrWhiteSpace(name)) // Filter out empty strings.
             .Distinct() // Ensure uniqueness.
             .OrderBy(name => name).ToList();

        [ObservableProperty]
        private bool _tFIsCancelling = false;

        [RelayCommand]
        private void TFCancelTranslate()
        {
            TFIsCancelling = true;
        }


        #endregion

        #region CACHE EDITOR
        // The collection used by the UI (ListView) to display results
        public ObservableCollection<Pair> FilteredEntries { get; } = new ObservableCollection<Pair>();

        // Search text in the TextBox
        [ObservableProperty]
        private string _searchText = string.Empty;

        // Called automatically whenever SearchText changes
        partial void OnSearchTextChanged(string value)
        {
            RefreshFilteredEntries(value);
        }

        private void RefreshFilteredEntries(string text)
        {
            if (TCache._entries == null) return;
            FilteredEntries.Clear();

            var matches = string.IsNullOrEmpty(text)
                ? TCache._entries
                : TCache._entries.Where(kvp =>
                        kvp.Key.Contains(text, System.StringComparison.OrdinalIgnoreCase));

            foreach (var kvp in matches)
            {
                FilteredEntries.Add(new Pair { Key = kvp.Key, Value = kvp.Value });
            }

            // If the currently selected item no longer appears in the filtered list, unselect it
            if (!FilteredEntries.Contains(SelectedPair))
            {
                if (FilteredEntries.Count > 0)
                {
                    SelectedPair = FilteredEntries.First();
                    return;
                }
            }

        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = string.Empty;
            SelectedPair = FilteredEntries.First();
        }

        [RelayCommand(CanExecute = nameof(CanSaveChanges))]
        private async Task SaveChanges()
        {
            if (SelectedPair == null)
                return;
            _savedPairKey = SelectedPair.Key;
            await TCache.UpdateEntryAsync(SelectedPair.Key, SelectedPair.Value);
            _hasPendingChanges = false;
            SaveChangesCommand.NotifyCanExecuteChanged();
            // Re-filter to reflect any possible key changes
            RefreshFilteredEntries(SearchText);
            if (_savedPairKey != null)
            {
                Pair found = FilteredEntries.FirstOrDefault(pair => pair.Key == _savedPairKey);
                if (found != null)
                {
                    SelectedPair = found;
                }
            }
        }

        public async void LoadCache()
        {
            TUtils.CalcPaths(Target);
            if (!Directory.Exists(Target)) return;
            StorageFolder x = await StorageFolder.GetFolderFromPathAsync(TUtils.TargetTranslatorPath);
            TCache.Init(x);
            await TCache.InitializeAsync();
            //if (SearchText == "")
            //{
                RefreshFilteredEntries(SearchText);
            //}
        }

        // The "dirty" flag indicating if the user has changed something
        private bool _hasPendingChanges;

        // The currently selected pair in the ListView
        private Pair _selectedPair;
        public Pair SelectedPair
        {
            get => _selectedPair;
            set
            {
                if (SetProperty(ref _selectedPair, value))
                {
                    // Each time we pick a new Pair, reset the "dirty" flag
                    _hasPendingChanges = false;
                    SaveChangesCommand.NotifyCanExecuteChanged();

                    if (SelectedPair != null)
                    {
                        // Unsubscribe from previous Pair (if needed)
                        // and subscribe to property changes on the new Pair
                        SelectedPair.PropertyChanged += OnSelectedPairPropertyChanged;
                    }
                }
            }
        }

        private void OnSelectedPairPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // If user changes "Value" (or "Key"), set dirty = true
            if (e.PropertyName == nameof(Pair.Value) || e.PropertyName == nameof(Pair.Key))
            {
                _hasPendingChanges = true;
                SaveChangesCommand.NotifyCanExecuteChanged();
            }
        }

        private void OnSelectedPairChanged()
        {
            // Reset the dirty flag whenever the user selects a new Pair
            _hasPendingChanges = false;
            SaveChangesCommand.NotifyCanExecuteChanged();
            DeleteSelectedCommand.NotifyCanExecuteChanged();

            if (SelectedPair == null)
                return;

            // Unsubscribe from any previous pair's PropertyChanged (if needed)
            // (In a more advanced scenario, you'd keep track of old pair references.)

            // Subscribe to property changes on the new SelectedPair
            SelectedPair.PropertyChanged += (s, e) =>
            {
                // If the user changes *any* property on the Pair, we assume there's pending changes
                if (e.PropertyName == nameof(Pair.Value) || e.PropertyName == nameof(Pair.Key))
                {
                    _hasPendingChanges = true;
                    // Let the command system know it can run (or not)
                    SaveChangesCommand.NotifyCanExecuteChanged();
                    DeleteSelectedCommand.NotifyCanExecuteChanged();
                }
            };

        }

        private bool CanSaveChanges()
        {
            return SelectedPair != null && _hasPendingChanges;
        }

        private string _savedPairKey;

        [RelayCommand]
        private async Task DeleteSelected()
        {
            await TCache.RemoveEntryAsync(SelectedPair.Key);
            LoadCache();
            if (FilteredEntries.Count == 1)
            {
                SelectedPair = null;
                SearchText = "";
            }
            RefreshFilteredEntries(SearchText);
        }

        private bool CanDeleteSelected()
        {
            return SelectedPair != null;
        }

        [ObservableProperty]
        int _cacheSearchSelectionStart = 0;

        [ObservableProperty]
        int _cacheSearchSelectionLength = 0;
        #endregion
    }
}


