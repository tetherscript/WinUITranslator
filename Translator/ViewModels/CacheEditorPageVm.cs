using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

    public partial class CacheEditorPageVm : ObservableObject
    {

        public CacheEditorPageVm()
        {

            WeakReferenceMessenger.Default.Register<TTargetChanged>(this, (r, m) =>
            {
                Target = m.Value;
                LoadCache();
            });

            WeakReferenceMessenger.Default.Register<TCacheUdpated>(this, (r, m) =>
            {
                LoadCache();
            });

            WeakReferenceMessenger.Default.Register<TShuttingDown>(this, (r, m) =>
            {

            });


            CalcState();
        }

        public void CalcState()
        {

        }

        [ObservableProperty]
        private string _target;

        [ObservableProperty]
        private string _profile;

        [ObservableProperty]
        private bool _canSearch;

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

        private void SelectedPair_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
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

    }
}
