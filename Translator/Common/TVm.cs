using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;
using TeeLocalized;
using Translator.Log;
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
        private bool _isDarkTheme;
        partial void OnIsDarkThemeChanged(bool oldValue, bool newValue)
        {
            Theme = newValue ? ElementTheme.Dark : ElementTheme.Light;
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
                if (Profiles.Count > 0)
                {
                    SelectedProfile = Profiles[0];
                }
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
        ObservableCollection<string> _profiles = new();

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
        partial void OnSelectedProfileChanged(string value)
        {
            WeakReferenceMessenger.Default.Send(new TProfileSelected(value));
        }



        #endregion

        #region SCAN
        [ObservableProperty]
        int _scanLogSelectionStart = 0;

        [ObservableProperty]
        int _scanLogSelectionLength = 0;

        [RelayCommand]
        private async Task StartScan()
        {
            TLog.Reset(TLog.eLogType.Scan);
            if (TUtils.CalcPaths(Target))
            {
                IsBusy = true;
                IsScanning = true;
                ScanLog = "Scanning...";
                ScanLogScrollToBottom();
                await Task.Delay(100);
                await TScan.Start(TLog.eLogType.Scan, TUtils.TargetRootPath);
                TLog.Save(TLog.eLogType.Scan, TUtils.TargetScanLogPath);
            }
            else
            {
                TLog.Log(TLog.eLogType.Scan, TLog.eLogItemType.err, 0, "Target root path does not exist: " + Target);
            }
            TLog.Flush(TLog.eLogType.Scan);
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

        #region LOG
        public void AddLogItem(TLogItem item)
        {
            int lineNumber = 0;
            switch (item.LogType)
            {
                case TLog.eLogType.Translate: lineNumber = TranslateLogItems.Count; break;
                case TLog.eLogType.ProfileTest: lineNumber = ProfileTestLogItems.Count; break;
                default: break;
            };
            TLogItemEx newItem = new TLogItemEx(
                GetLogTextColor(item.ItemType),
                lineNumber.ToString(),
                item.ItemType,
                item.Indent,
                item.Message,
                (((item.Data == null) || (item.Data.Count == 0)) ? false : true),
                item.Data,
                (((item.Data == null) || (item.Data.Count == 0)) ? "" : string.Join(Environment.NewLine + new string('━', 30) + Environment.NewLine, item.Data)),
            item.ItemType.ToString() + ":" + item.Message
            );
            switch (item.LogType)
            {
                case TLog.eLogType.Translate: TranslateLogItems.Add(newItem); break;
                case TLog.eLogType.ProfileTest: ProfileTestLogItems.Add(newItem); break;
                default: break;
            };
        }

        private SolidColorBrush GetLogTextColor(TLog.eLogItemType type)
        {
            SolidColorBrush GetResourceBrush(string res)
            {
                if (Application.Current.Resources.TryGetValue(res, out var resource) && resource is SolidColorBrush brush)
                {
                    return brush;
                }
                return new SolidColorBrush(Colors.Gray);
            }

            switch (type)
            {
                case TLog.eLogItemType.inf: return GetResourceBrush("InfoTextBrush");
                case TLog.eLogItemType.sum: return GetResourceBrush("SummaryTextBrush");
                case TLog.eLogItemType.dbg: return GetResourceBrush("DebugTextBrush");
                case TLog.eLogItemType.wrn: return GetResourceBrush("WarningTextBrush");
                case TLog.eLogItemType.err: return GetResourceBrush("ErrorTextBrush");
                case TLog.eLogItemType.tra: return GetResourceBrush("ActionTextBrush");
                default: return GetResourceBrush("TextFillColorPrimaryBrush");
            }
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

        public ObservableCollection<TLogItemEx> ProfileTestLogItems = new();

        public ObservableCollection<TLogItemExFilter> ProfileTestLogFilters = new();

        public void SetProfileTestLogFilter(string filter)
        {
            ProfileTestLogFilters.Clear();
            ProfileTestLogFilters.Add(new TLogItemExFilter("Translations", "tra", false));
            ProfileTestLogFilters.Add(new TLogItemExFilter("Summary", "sum", false));
            ProfileTestLogFilters.Add(new TLogItemExFilter("Info", "inf", false));
            ProfileTestLogFilters.Add(new TLogItemExFilter("Error", "err", false));
            ProfileTestLogFilters.Add(new TLogItemExFilter("Warning", "wrn", false));
            ProfileTestLogFilters.Add(new TLogItemExFilter("Debug", "dbg", false));

            string[] activeFilters = filter.Split(',');

            foreach (TLogItemExFilter item in ProfileTestLogFilters)
            {
                item.IsChecked = activeFilters.Contains(item.Value);
            }
        }

        public string GetProfileTestLogFilter()
        {
            List<string> filter = [];
            foreach (TLogItemExFilter item in ProfileTestLogFilters)
            {
                if (item.IsChecked)
                {
                    filter.Add(item.Value);
                }
            }
            return string.Join(",", filter);
        }

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
            await translatorExProc.TranslateStart(TLog.eLogType.Translate, Target, SelectedProfile, _translateCts, TranslateSaveToCache);
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
        #endregion

        #region PROFILE EDITOR

        [ObservableProperty]
        private int _profileTestLastTabIndex = 0;

        [ObservableProperty]
        private List<string> _profileTestCultureList = CultureInfo
            .GetCultures(CultureTypes.AllCultures)
            .Select(c => c.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name)) // Filter out empty strings.
            .Distinct() // Ensure uniqueness.
            .OrderBy(name => name).ToList();

        [RelayCommand]
        private async Task ProfileTestStart()
        {
            IsBusy = true;
            ProfileTestIsTranslating = true;
            ProfileTestLogItems.Clear();
            _profileTestCts = new();
            TTranslatorExProc translatorExProc = new();
            await translatorExProc.ProfileTestStart(TLog.eLogType.ProfileTest, Target, SelectedProfile, _profileTestCts, ProfileTestTextToTranslate, ProfileTestTestRepeats, ProfileTestToCulture);
            _profileTestCts.Dispose();
            ProfileTestIsTranslating = false;
            IsBusy = false;
        }

        [RelayCommand]
        private void ProfileTestCancelTranslate()
        {
            _profileTestCts.TryReset();
            _profileTestCts.Cancel();
        }

        private CancellationTokenSource? _profileTestCts;

        [ObservableProperty]
        private int _profileTestProfileIndex;

        [ObservableProperty]
        private string _profileTestToCulture;

        [ObservableProperty]
        private int _profileTestTestRepeats;

        [ObservableProperty]
        private string _profileTestTextToTranslate;

        [ObservableProperty]
        private bool _profileTestIsTranslating;

        [ObservableProperty]
        private string _profileTestValidHintTokens = TLocalized.ValidHintTokenStr;













        #region SETTINGS
        [ObservableProperty]
        private string _profileTestSettings;
        partial void OnProfileTestSettingsChanged(string value)
        {
            ProfileTestSettingsModified = true;
        }

        [ObservableProperty]
        private bool _profileTestSettingsModified;
        partial void OnProfileTestSettingsModifiedChanged(bool value)
        {
            ProfileTestCloneSettingCommand.NotifyCanExecuteChanged();
            ProfileTestRenameSettingCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        private void ProfileTestLoadSettings()
        {
            TUtils.CalcPaths(Target);
            string path = Path.Combine(TUtils.TargetProfilesPath, SelectedProfile + ".prf");
            if (path == null)
            {
                ProfileTestSettings = "No settings file specified.";
            }
            else
            {
                try
                {
                    string loadedJson = File.ReadAllText(path);
                    ProfileTestSettings = loadedJson;
                    ProfileTestSettingsModified = false;
                }
                catch (Exception ex)
                {

                }
            }
        }

        [RelayCommand]
        private void ProfileTestSaveSettings()
        {
            TUtils.CalcPaths(Target);
            string path = Path.Combine(TUtils.TargetProfilesPath, SelectedProfile + ".prf");
            if (path == null) return;
            try
            {
                File.WriteAllText(path, ProfileTestSettings);
                ProfileTestSettingsModified = false;
            }
            catch (Exception ex)
            {

            }
        }


        private bool ProfiletestCanCloneSetting()
        {
            return (!IsBusy && !ProfileTestSettingsModified);
        } 

        [ObservableProperty]
        private bool _profileTestIsCloningSetting;

        [RelayCommand(CanExecute = nameof(ProfiletestCanCloneSetting))]
        private void ProfileTestCloneSetting()
        {

        }





        private bool ProfiletestCanRenameSetting()
        {
            return (!IsBusy && !ProfileTestSettingsModified);
        }

        [ObservableProperty]
        private bool _profileTestIsRenamingSetting;
        partial void OnProfileTestIsRenamingSettingChanged(bool value)
        {
            ProfileTestCloneSettingCommand.NotifyCanExecuteChanged();
            ProfileTestRenameSettingCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(ProfiletestCanRenameSetting))]
        private void ProfileTestRenameSetting()
        {
            ProfileTestIsRenamingSetting = true;
            IsBusy = true;
        }

        [ObservableProperty]
        private string _profileTestRenamedSettingStr;
        partial void OnProfileTestRenamedSettingStrChanged(string value)
        {
            ProfileTestSettingRenameStrInvalid = TUtils.ValidateFileName(value);
        }

        [ObservableProperty]
        private bool _profileTestSettingRenameStrInvalid;

        [RelayCommand]
        private void ProfileTestRenameSettingSave()
        {
            ProfileTestIsRenamingSetting = false;
            IsBusy = false;
        }

        [RelayCommand]
        private void ProfileTestRenameSettingCancel()
        {
            ProfileTestIsRenamingSetting = false;
            IsBusy = false;
        }





  

        #endregion



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
            if (FilteredEntries.Count > 0)
            {
                SelectedPair = FilteredEntries.First();
            }
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


