using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace Translator
{
    public partial class CacheEditorPageExVm : ObservableObject
    {
        public const string CacheEditSearchText = "CacheEditSearchText";

        public CacheEditorPageExVm()
        {

            WeakReferenceMessenger.Default.Register<TTargetChanged>(this, async (r, m) =>
            {
                await Load(TUtils.TargetTranslatorCachePath);
            });

            WeakReferenceMessenger.Default.Register<TCacheUdpated>(this, async (r, m) =>
            {
                await Load(TUtils.TargetTranslatorCachePath);
            });

            WeakReferenceMessenger.Default.Register<TShuttingDown>(this, (r, m) =>
            {
                SaveSettings();
            });

            LoadSettings();
            CalcState();
        }


        public void LoadSettings()
        {
            var appData = ApplicationData.Current.LocalSettings;
            SearchText = (appData.Values.ContainsKey(CacheEditSearchText)) ?
                (string)appData.Values[CacheEditSearchText] : "";
        }

        public void SaveSettings()
        {
            var appData = ApplicationData.Current.LocalSettings;
            appData.Values[CacheEditSearchText] = SearchText;
        }

        private void CalcState()
        {
            CanSearch = (
                !IsEditing
            );

            CanEdit = (
                IsEditing
            );

            CanRevert = (
              (
                (IsEditing) &&
                (EditText != SelectedItem.Value)
              )
            );

            CanDelete = (
                ((!IsEditing) && (SelectedItem != null))
            );
        }

        [ObservableProperty]
        private TCacheItemEx _selectedItem;
        partial void OnSelectedItemChanged(TCacheItemEx? oldValue, TCacheItemEx newValue)
        {
            if (IsEditing)
            {
                SaveEdit(oldValue.Key, EditText).GetAwaiter().GetResult();
                IsEditing = false;
                EditText = "";
            }
            CalcState();
        }

        private async Task SaveEdit(string key, string value)
        {
            await AddOrUpdate(key, value).ConfigureAwait(false);
        }

        [ObservableProperty]
        private bool _canSearch;

        [ObservableProperty]
        private bool _isEditing;

        [ObservableProperty]
        private bool _isSaving;

        [ObservableProperty]
        private bool _isDeleting;

        [ObservableProperty]
        private bool _canSave;

        [ObservableProperty]
        private bool _canRevert;

        [RelayCommand]
        private void Revert()
        {
            EditText = SelectedItem.Value;
            CalcState();
        }

        [ObservableProperty]
        private bool _canEdit;

        [ObservableProperty]
        private bool _canDelete;

        [ObservableProperty]
        private string _editText;
        partial void OnEditTextChanged(string? oldValue, string newValue)
        {
            CalcState();
        }

        [ObservableProperty]
        private string _searchText = string.Empty;

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (FilteredItems == null) { return; }
            using (FilteredItems.DeferRefresh())
            {
                string searchText = SearchText;
                FilteredItems.SortDescriptions.Clear();
                FilteredItems.SortDescriptions.Add(new SortDescription("Key", SortDirection.Ascending));
                List<string> searchFilter = [];
                if (searchText != "") { searchFilter.Add(searchText); }
                FilteredItems.ClearObservedFilterProperties();
                FilteredItems.Filter = x =>
                (
                    ((TCacheItemEx)x).Key.Contains(searchText, StringComparison.CurrentCultureIgnoreCase)
                );
            }
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = string.Empty;
        }

        [RelayCommand]
        private async Task Delete()
        {
            DeleteItem().GetAwaiter().GetResult();
        }

        private async Task DeleteItem()
        {
            await RemoveEntryAsync().ConfigureAwait(false);
        }

        public void EditSelected()
        {
            EditText = SelectedItem.Value;
            IsEditing = true;
            CalcState();
        }

        public ObservableCollection<TCacheItemEx> _items = [];
        public AdvancedCollectionView FilteredItems;
        private string _path;
        public async Task Load(string path)
        {
            _path = path;
            if (File.Exists(path))
            {
                string json = await File.ReadAllTextAsync(_path, Encoding.UTF8);
                if (json != "")
                {
                    List<TCacheItemEx> tmp;
                    tmp = JsonSerializer.Deserialize<List<TCacheItemEx>>(json);
                    _items.Clear();
                    foreach (var item in tmp)
                    {
                        _items.Add(item);
                    }
                    if (FilteredItems == null)
                    {
                        FilteredItems = new AdvancedCollectionView(_items, true);
                    }
                    if (_items.Count > 0)
                    {
                        SelectedItem = _items[0];
                    }
                    ApplyFilter();
                }
            }
        }

        public async Task AddOrUpdate(string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                if (_items.Any(e => e.Key == key))
                {
                    TCacheItemEx existingEntry = _items.First(e => e.Key == key);
                    existingEntry.Value = value;
                }
                await SaveAsync().ConfigureAwait(false); 
            }
        }

        public async Task RemoveEntryAsync()
        {
            if (SelectedItem != null)
            {
                _items.Remove(SelectedItem);
                await SaveAsync().ConfigureAwait(false); ;
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                //_items.Sort((a, b) => string.Compare(a.Key, b.Key));
                string json = JsonSerializer.Serialize(_items, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_path, json, Encoding.UTF8).ConfigureAwait(false); ;

            }
            catch (Exception)
            {
                
            }

        }
        


    }
}
