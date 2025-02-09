using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Translator
{

    public sealed partial class ProfileSettingPage : Page
    {
        public TVm Vm = new();

        public partial class TVm : ObservableObject
        {
            public void Reset()
            {
                SettingsStr = "";
                _prevSettingsStr = "";
                CanSave = false;
                CanRevert = false;
                SettingsStrModified = false;
                IsSaving = false;
                IsLoading = false;
                IsLoaded = false;
            }

            public void CalcState()
            {
                CanLoad = (
                    (!IsLoading) &&
                    (!IsSaving)
                );
                CanSave = (
                    (IsLoaded) && 
                    (!IsLoading) && 
                    (!IsSaving) &&
                    (SettingsStrModified)
                );
                CanRevert = (
                    (IsLoaded) &&
                    (!IsLoading) &&
                    (!IsSaving) &&
                    (SettingsStrModified)
                );
            }

            #region LOAD
            [ObservableProperty]
            private bool _canLoad;

            [ObservableProperty]
            private bool _isLoading;

            [ObservableProperty]
            private bool _isLoaded;

            [RelayCommand]
            private void Load()
            {
                IsLoading = true;
                CalcState();
                string path = Path.Combine(TUtils.TargetProfilesPath, App.Vm.SelectedProfile + ".prf");
                try
                {
                    string loadedJson = File.ReadAllText(path);
                    SettingsStr = loadedJson;
                    _prevSettingsStr = SettingsStr;
                    SettingsStrModified = false;
                    IsLoaded = true;
                    CalcState();
                }
                catch (Exception ex)
                {

                }
                IsLoading = false;
                CalcState();
            }
            #endregion

            #region SAVE
            private string _prevSettingsStr;

            [ObservableProperty]
            private string _settingsStr;
            partial void OnSettingsStrChanged(string value)
            {
                SettingsStrModified = ((!IsLoading) && (_prevSettingsStr != value));
                CalcState();
            }

            [ObservableProperty]
            private bool _settingsStrModified;

            [ObservableProperty]
            private bool _canSave;

            [ObservableProperty]
            private bool _isSaving;

            [RelayCommand]
            private void Save()
            {
                IsSaving = true;
                CalcState();
                string path = Path.Combine(TUtils.TargetProfilesPath, App.Vm.SelectedProfile + ".prf");
                if (path == null) return;
                try
                {
                    File.WriteAllText(path, SettingsStr);
                    _prevSettingsStr = SettingsStr;
                    SettingsStrModified = false;
                    CalcState();
                }
                catch (Exception ex)
                {

                }
                IsSaving = false;
                CalcState();
            }
            #endregion

            #region REVERT
            [ObservableProperty]
            private bool _canRevert;

            [ObservableProperty]
            private bool _isReverting;

            [RelayCommand]
            private void Revert()
            {
                IsReverting = true;
                CalcState();
                Load();
                IsReverting = false;
                CalcState();
            }
            #endregion

        }

        public ProfileSettingPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            WeakReferenceMessenger.Default.Register<TProfileSelected>(this, (r, m) =>
            {
                Vm.Reset();
            });

            WeakReferenceMessenger.Default.Register<TTargetSelected>(this, (r, m) =>
            {
                Vm.Reset();
            });

            Vm.CalcState();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }
    }

}
        