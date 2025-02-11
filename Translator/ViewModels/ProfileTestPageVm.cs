using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TeeLocalized;
using Windows.Storage;

namespace Translator
{
    public partial class TProfileTestPageVm : ObservableObject
    {
        public const string ProfileTestToCulture = "ProfileTestToCulture";
        public const string ProfileTestTestRepeats = "ProfileTestTestRepeats";
        public const string ProfileTestTextToTranslate = "ProfileTestTextToTranslate";

        public TProfileTestPageVm()
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

            WeakReferenceMessenger.Default.Register<TTestProgress>(this, (r, m) =>
            {
                Progress = m.Value;
            });


            LoadSettings();
            CalcState();
        }

        public void Reset()
        {

        }

        public void CalcState()
        {
            CanTranslate = (
                !IsTranslating
            );
        }

        public void LoadSettings()
        {
            var appData = ApplicationData.Current.LocalSettings;
            ToCulture = (appData.Values.ContainsKey(ProfileTestToCulture)) ?
                (string)appData.Values[ProfileTestToCulture] : "de-DE";
            Repeats = (appData.Values.ContainsKey(ProfileTestTestRepeats)) ?
                (int)appData.Values[ProfileTestTestRepeats] : 1;
            TextToTranslate = (appData.Values.ContainsKey(ProfileTestTextToTranslate)) ?
                (string)appData.Values[ProfileTestTextToTranslate] : "@Close\r//@@Click to save your profile.\r//!Aperture\r//!!Click to adjust white balance.\r//@Loading {0}, please wait...";
        }

        public void SaveSettings()
        {
            var appData = ApplicationData.Current.LocalSettings;
            appData.Values[ProfileTestToCulture] = ToCulture;
            appData.Values[ProfileTestTestRepeats] = Repeats;
            appData.Values[ProfileTestTextToTranslate] = TextToTranslate;
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
        private bool _isTranslating;
        partial void OnIsTranslatingChanged(bool value)
        {
            CalcState();
        }

        [ObservableProperty]
        private string _canCancel;

        [ObservableProperty]
        private List<string> _cultureList = CultureInfo
            .GetCultures(CultureTypes.AllCultures)
            .Select(c => c.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct()
            .OrderBy(name => name).ToList();

        [RelayCommand]
        private async Task Start()
        {
            IsTranslating = true;
            CalcState();
            Progress = 0;
            WeakReferenceMessenger.Default.Send(new TClearLog(false));
            _profileTestCts = new();

            await Task.Delay(2000);
            TTranslatorExProc translatorExProc = new();
            await translatorExProc.ProfileTestStart(TLog.eLogType.ProfileTest, Target, Profile, _profileTestCts, TextToTranslate, Repeats, ToCulture);
            _profileTestCts.Dispose();
            Progress = 100;
            await Task.Delay(200);
            IsTranslating = false;
            CalcState();
        }

        [RelayCommand]
        private void Translate()
        {
            _profileTestCts.TryReset();
            _profileTestCts.Cancel();
        }

        private CancellationTokenSource? _profileTestCts;

        [ObservableProperty]
        private string _toCulture;

        [ObservableProperty]
        private int _repeats;

        [ObservableProperty]
        private string _textToTranslate;

        [ObservableProperty]
        private string _validHintTokens = TLocalized.ValidHintTokenStr;

    }
}
