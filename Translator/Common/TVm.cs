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
using TeeLocalized;
using System.Threading;
using System.Data.SqlTypes;
using Windows.Devices.Sensors;
using CommunityToolkit.WinUI.Collections;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Translator.Log;



namespace Translator
{


    public partial class TTranslateLogItem(string lineNumber, TLog.eLogItemType type, bool? isSuccessful, string untranslatedText, string? translatedText, int? confidence, string? reasoning, int indent, string? message, SolidColorBrush textColor, bool hasProfileResults) : ObservableObject
    {
        [ObservableProperty]
        private string _lineNumber = lineNumber;

        [ObservableProperty]
        private DateTime _timeStamp = DateTime.Now;

        [ObservableProperty]
        private TLog.eLogItemType _type = type;

        [ObservableProperty]
        private bool? _isSuccessful = isSuccessful;

        [ObservableProperty]
        private string _untranslatedText = untranslatedText;

        [ObservableProperty]
        private string? _translatedText = translatedText;

        [ObservableProperty]
        private int? _confidence = confidence;

        [ObservableProperty]
        private string? _reasoning = reasoning;

        [ObservableProperty]
        private int? _indent = indent;

        [ObservableProperty]
        private string? _message = message;

        [ObservableProperty]
        private string _filterableStr = untranslatedText + ":" + translatedText + ":" + reasoning + ":" + message;

        [ObservableProperty]
        SolidColorBrush _textColor = textColor;


        [ObservableProperty]
        private bool _hasProfileResults = hasProfileResults;

    }



    public partial class Pair : ObservableObject
    {
        [ObservableProperty]
        private string key;

        [ObservableProperty]
        private string value;
    }

    public partial class TVm : ObservableObject
    {
        public TVm(string[] arguments)
        {
            _arguments = arguments;
        }

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

        private string[] _arguments = [];
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
        partial void OnTargetChanged(string value)
        {
            Profiles.Clear();
            TUtils.CalcPaths(Target);
            IsTargetPathInvalid = !TUtils.RootPathIsValid;
            TargetNotConfigured = !TUtils.IsConfigured;
            IsValidConfiguredPath = ((!IsTargetPathInvalid) && (!TargetNotConfigured));
            if (IsValidConfiguredPath)
            {
                GetProfiles();
            }
        }

        [ObservableProperty]
        private bool _isTargetPathInvalid = false;

        [ObservableProperty]
        private bool _targetNotConfigured = false;

        [ObservableProperty]
        private bool _isValidConfiguredPath = false;

        public ObservableCollection<string> TargetList = new();

        [ObservableProperty]
        private bool _isAddingTarget;

        [ObservableProperty]
        private string _inputTarget;

        [RelayCommand]
        private void AddTarget()
        {
            InputTarget = "";
            IsAddingTarget = true;
        }

        [RelayCommand]
        private void RemoveTarget()
        {
            IsAddingTarget = false;
            TargetNotConfigured = false;
            int index = TargetList.IndexOf(Target);
            if (index != -1)
            {
                IsTargetPathInvalid = false;
                TargetList.RemoveAt(index);
                if (TargetList.Count > 0)
                {
                    Target = TargetList[0];
                }
            }
        }

        [RelayCommand]
        private void AddTargetCancel()
        {
            IsAddingTarget = false;
        }

        [RelayCommand]
        private async void AddTargetSave()
        {
            if (InputTarget.Trim() != "")
            {
                TargetList.Add(InputTarget.Trim());
                IsAddingTarget = false;
                await Task.Delay(400);
                Target = InputTarget.Trim();
            }
        }


        #region PROFILES
        [ObservableProperty]
        List<string> _profiles = new();

        public void GetProfiles()
        {
            if (TUtils.CalcPaths(Target))
            {

                var prfFiles = Directory.EnumerateFiles(TUtils.TargetProfilesPath, "*.prf")
                                        .Select(filePath => Path.GetFileNameWithoutExtension(filePath))
                                        .OrderBy(fileName => fileName)
                                        .ToList();
                Profiles.Clear();
                foreach (string file in prfFiles)
                {
                    Profiles.Add(file);
                }
            }
        }

        [ObservableProperty]
        private string _selectedProfile;



        #endregion

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
        private CancellationTokenSource? _translateCts;

        [ObservableProperty]
        private bool _isTranslating;

        [ObservableProperty]
        private int _translateProgress = 0;

        [ObservableProperty]
        private bool _translateSaveToCache;

        [RelayCommand]
        private async Task StartTranslate()
        {
            IsBusy = true;
            IsTranslating = true;
            TranslateLogItems.Clear();
            await Task.Delay(100);
            _translateCts = new();
            TTranslatorExProc translatorExProc = new ();
            await translatorExProc.Start(TLog.eMode.translate, Target, SelectedProfile, _translateCts, TranslateSaveToCache);
            _translateCts.Dispose();
            TranslateProgress = 0;
            IsTranslating = false;
            IsBusy = false;
        }

        [RelayCommand]
        private void CancelTranslate()
        {
            _translateCts.TryReset();
            _translateCts.Cancel();
        }

        #region LOG
        public void AddTranslateLogItem(TranslateProgressReport item)
        {
            SolidColorBrush CalcBrush(string res)
            {
                if (Application.Current.Resources.TryGetValue(res, out var resource) && resource is SolidColorBrush brush)
                {
                    return brush;
                }
                return new SolidColorBrush(Colors.Gray);
            }

            int lineNumber = TranslateLogItems.Count;

            SolidColorBrush textColor;
            switch (item.LogItem.itemType.ToString())
            {
                case "sum": textColor = CalcBrush("SystemFillColorSuccessBrush"); break;
                default: textColor = CalcBrush("TextFillColorPrimaryBrush"); break;
            }

            TLogItemEx x;
            if (item.TranslatorResult == null)
            {
                x = new TLogItemEx(
                    lineNumber.ToString().PadLeft(4, ' '),
                    item.LogItem.itemType,
                    null,
                    null,
                    null,
                    null,
                    null,
                    item.LogItem.indent,
                    new string(' ', item.LogItem.indent) + item.LogItem.Message,
                    textColor,
                    false
                    );
            }
            else
            {
                x = new TLogItemEx(
                    lineNumber.ToString().PadLeft(4, ' '),
                    item.LogItem.itemType,
                    item.TranslatorResult.IsSuccessful,
                    item.TranslatorResult.UntranslatedText,
                    item.TranslatorResult.TranslatedText,
                    item.TranslatorResult.Confidence,
                    item.TranslatorResult.Reasoning,
                    item.LogItem.indent,
                    new string(' ', item.LogItem.indent) + item.LogItem.Message,
                    textColor,
                    true
                    );
            }
            TranslateLogItems.Add(x);
        }

        public ObservableCollection<TLogItemEx> TranslateLogItems = new();

        public ObservableCollection<TLogItemExFilter> TranslateLogFilters = new();

        public void SetTranslateLogFilter(string filter)
        {
            TranslateLogFilters.Clear();
            TranslateLogFilters.Add(new TLogItemExFilter("Translations", "tra", false));
            TranslateLogFilters.Add(new TLogItemExFilter("Summary", "sum", false));
            TranslateLogFilters.Add(new TLogItemExFilter("Info", "inf", false));
            TranslateLogFilters.Add(new TLogItemExFilter("Error", "err", false));
            TranslateLogFilters.Add(new TLogItemExFilter("Warning", "wrn", false));
            TranslateLogFilters.Add(new TLogItemExFilter("Debug", "dbg", false));

            string[] activeFilters = filter.Split(',');

            foreach (TLogItemExFilter item in TranslateLogFilters)
            {
                item.IsChecked = activeFilters.Contains(item.Value);
            }
        }

        public string GetTranslateLogFilter()
        {
            List<string> filter = [];
            foreach (TLogItemExFilter item in TranslateLogFilters)
            {
                if (item.IsChecked)
                {
                    filter.Add(item.Value);
                }
            }
            return string.Join(",", filter);
        }
        #endregion
        #endregion

        #region TRANSLATION FUNCTIONS

        [ObservableProperty]
        private int _tFLastTabIndex = 0;

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
            //TUtils.CalcPaths(Target);
            //string path = TTransFunc.GetSettingsPath(SelectedProfile);
            //if (path == null)
            //{
            //    TFSettings = "No settings file specified.";
            //}
            //else 
            //{ 
            //    try
            //    {
            //        string loadedJson = File.ReadAllText(path);
            //        TLog.Log(TLog.eMode.tfTranslate, TLog.eLogItemType.inf, 0, "Loaded settings: " + path);
            //        TFSettings = loadedJson;
            //        TFSettingsModified = false;
            //    }
            //    catch (Exception ex)
            //    {
            //        TLog.Log(TLog.eMode.tfTranslate, TLog.eLogItemType.err, 0, "Load setting failed: " + path + ": " + ex.Message);
            //    }
            //}
        }

        [RelayCommand]
        private async void TFTestSaveSettings()
        {
            //TUtils.CalcPaths(Target);
            //TLog.Reset(TLog.eMode.tfTranslate);
            //string path = TTransFunc.GetSettingsPath(SelectedProfile);
            //if (path == null) return;
            //try
            //    {
            //    File.WriteAllText(path, TFSettings);
            //    TLog.Log(TLog.eMode.tfTranslate, TLog.eLogItemType.inf, 0, "Saved settings: " + path);
            //    TFSettingsModified = false;
            //}
            //catch (Exception ex)
            //{
            //    TLog.Log(TLog.eMode.tfTranslate, TLog.eLogItemType.err, 0, "Save setting failed: " + path + ": " + ex.Message);
            //}
            //TLog.Flush(TLog.eMode.tfTranslate);
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
            //TranslateLog = "Translating...";
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
            TFLog = "";
            int _failedTranslationCounter = 0;
            string[] lines = TFTextToTranslate.Split(new[] { "\r" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in lines)
            {
                if (item.StartsWith("//")) 
                { 
                    continue;
                }
                else
                {
                    TFLog = TFLog + item + Environment.NewLine;
                }
                (string prefix, _) = TLocalized.SplitPrefix(item);
                int prefixLength = prefix.Length;
                for (int i = 0; i < TFTestRepeats; i++)
                {
                    await Task.Delay(10);
                    if (TFIsCancelling) return;
                    string trans = await TTranslate.StartTest(TLog.eMode.tfTranslate, TUtils.TargetRootPath,
                            SelectedProfile, item, TFToCulture);
                    TFLog = TFLog + (trans == null ? "err" : trans) + Environment.NewLine;
                    if (trans == null)
                    {
                        _failedTranslationCounter++;
                        if (_failedTranslationCounter == 3)
                        {
                            TFLog = TFLog + Environment.NewLine + "Cancelled - too many errors.";
                            return;
                        };
                    }
                }
                TFLog = TFLog + new string('━', 60) + Environment.NewLine;
            }
        }

        [ObservableProperty]
        private string _tFToCulture;

        [ObservableProperty]
        private int _tFTestRepeats;

        [ObservableProperty]
        private string _tFTextToTranslate;

        [ObservableProperty]
        private string _tFTestResult = null;

        [ObservableProperty]
        private bool _tFIsTranslating;

        [ObservableProperty]
        private string _tFLog;

        [ObservableProperty]
        private List<string> _cultureList = CultureInfo
             .GetCultures(CultureTypes.AllCultures)
             .Select(c => c.Name)
             .Where(name => !string.IsNullOrWhiteSpace(name)) // Filter out empty strings.
             .Distinct() // Ensure uniqueness.
             .OrderBy(name => name).ToList();

        [ObservableProperty]
        private string _validHintTokens = TLocalized.ValidHintTokenStr;

        [ObservableProperty]
        private bool _tFIsCancelling = false;

        [RelayCommand]
        private async Task TFCancelTranslate()
        {
            TFIsCancelling = true;
            TFLog = TFLog + "Cancelling..." + Environment.NewLine;
            await Task.Delay(10);
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


