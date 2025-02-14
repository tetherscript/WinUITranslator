using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Translator
{

    public partial class ScanPageVm : ObservableObject
    {
        public ScanPageVm()
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

            });


            CalcState();
        }

        public void CalcState()
        {
            CanScan = (
                !IsScanning
            );

            CanCancel = (
                IsScanning
            );

        }

        [ObservableProperty]
        private string _target;

        [ObservableProperty]
        private string _profile;

         [ObservableProperty]
        private bool _canScan;

        [ObservableProperty]
        private bool _canCancel;

        [ObservableProperty]
        private bool _isScanning;

        private CancellationTokenSource? _cts;

        [RelayCommand]
        private void Cancel()
        {
            _cts.TryReset();
            _cts.Cancel();
        }

        [RelayCommand]
        private async Task Start()
        {
            IsScanning = true;
            WeakReferenceMessenger.Default.Send(new TClearLog(false));
            CalcState();
            _cts = new();
            TTranslatorExProc translatorExProc = new();
            
            //await translatorExProc.TranslateStart(TLog.eLogType.Translate, Target, Profile, _cts, SaveToCache);
            await TScan.Start(TLog.eLogType.Scan, TUtils.TargetRootPath);

            _cts.Dispose();
            await Task.Delay(200);
            IsScanning = false;
            CalcState();
        }

    }
}
