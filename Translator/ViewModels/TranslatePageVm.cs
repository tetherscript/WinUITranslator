using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Translator
{
    public partial class TranslatePageVm : ObservableObject
    {
        public const string TranslateSaveToCacheSetting = "TranslateSaveToCacheSetting";

        public TranslatePageVm()
        {
            WeakReferenceMessenger.Default.Register<TProfileChanged>(this, (r, m) =>
            {
                Profile = m.Value;

            });

            WeakReferenceMessenger.Default.Register<TTargetChanged>(this, (r, m) =>
            {
                Target = m.Value;
            });

            WeakReferenceMessenger.Default.Register<TShuttingDown>(this, (r, m) =>
            {
                SaveSettings();

            });

            WeakReferenceMessenger.Default.Register<TTranslateProgress>(this, (r, m) =>
            {
                Progress = m.Value;
            });


            LoadSettings();
            CalcState();
        }

        public void CalcState()
        {
            CanTranslate = (
                !IsTranslating
            );

            CanCancel = (
                IsTranslating
            );

            CanChangeSaveToCache = (
                !IsTranslating
            );
        }

        public void LoadSettings()
        {
            var appData = ApplicationData.Current.LocalSettings;
            SaveToCache = (appData.Values.ContainsKey(TranslateSaveToCacheSetting)) ?
                (bool)appData.Values[TranslateSaveToCacheSetting] : false;
        }

        public void SaveSettings()
        {
            var appData = ApplicationData.Current.LocalSettings;
            appData.Values[TranslateSaveToCacheSetting] = SaveToCache;
        }

        [ObservableProperty]
        private string _target;

        [ObservableProperty]
        private string _profile;

        [ObservableProperty]
        private int _progress;

        [ObservableProperty]
        private bool _canTranslate;

        [ObservableProperty]
        private bool _canCancel;

        [ObservableProperty]
        private bool _canChangeSaveToCache;

        [ObservableProperty]
        private bool _saveToCache;

        [ObservableProperty]
        private bool _isTranslating;

        private CancellationTokenSource? _cts;

        [RelayCommand]
        private void Cancel()
        {
            _cts.TryReset();
            _cts.Cancel();
        }

        [RelayCommand]
        private async Task Translate()
        {
            IsTranslating = true;
            Progress = 0;
            WeakReferenceMessenger.Default.Send(new TClearLog(false));
            CalcState();
            _cts = new();
            TTranslatorExProc translatorExProc = new();
            await translatorExProc.TranslateStart(TLog.eLogType.Translate, Target, Profile, _cts, SaveToCache);
            _cts.Dispose();
            Progress = 100;
            await Task.Delay(200);
            IsTranslating = false;
            CalcState();
            if (SaveToCache)
            {
                WeakReferenceMessenger.Default.Send(new TCacheUdpated(false));
            }
        }
    }
}
