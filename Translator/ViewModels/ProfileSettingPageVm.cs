using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.IO;

namespace Translator
{
    public partial class TProfileSettingPageVm : ObservableObject
    {

        public TProfileSettingPageVm()
        {
            WeakReferenceMessenger.Default.Register<TTargetChanged>(this, (r, m) =>
            {
                Target = m.Value;
                //Reset();
            });
            CalcState();

            WeakReferenceMessenger.Default.Register<TProfileChanged>(this, (r, m) =>
            {
                Profile = m.Value;
                Reset();
                Load();

            });
        }

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

        [ObservableProperty]
        private string _target;

        [ObservableProperty]
        private string _profile;

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
            string path = Path.Combine(TUtils.TargetProfilesPath, Profile + ".prf");
            try
            {
                string loadedJson = File.ReadAllText(path);
                SettingsStr = loadedJson;
                _prevSettingsStr = SettingsStr;
                SettingsStrModified = false;
                IsLoaded = true;
                CalcState();
            }
            catch (Exception)
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
            string path = Path.Combine(TUtils.TargetProfilesPath, Profile + ".prf");
            if (path == null) return;
            try
            {
                File.WriteAllText(path, SettingsStr);
                _prevSettingsStr = SettingsStr;
                SettingsStrModified = false;
                CalcState();
            }
            catch (Exception)
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
}
