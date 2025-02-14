using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace Translator
{

    public partial class MainPageVm : ObservableObject
    {
        public const string MainPageTarget = "MainPageTarget";
        public const string MainPageTargetList = "MainPageTargetList";
        public const string SelectedProfile = "SelectedProfile";
        public const string LastTabIndexSetting = "LastTabIndexSetting";
        public const string IsShowingProfileSettingsSetting = "IsShowingProfileSettingsSetting";
        public const string LogFilterSetting = "LogFilterSetting";

        public MainPageVm()
        {
            WeakReferenceMessenger.Default.Register<TAddLogItem>(this, (r, m) =>
            {
                AddLogItem(m.Value);
            });

            WeakReferenceMessenger.Default.Register<TClearLog>(this, (r, m) =>
            {
                LogItems.Clear();
            });

            WeakReferenceMessenger.Default.Register<TSaveLog>(this, (r, m) =>
            {
                SaveLog(m.Value);
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
            string p = (appData.Values.ContainsKey(SelectedProfile)) ? (string)appData.Values[SelectedProfile] : null;
            prevSelectedProfile = p;
            LoadTargetList();
            Target = (appData.Values.ContainsKey(MainPageTarget)) ? (string)appData.Values[MainPageTarget] : null;
            Profile = p;

            string logFilterStr = (appData.Values.ContainsKey(LogFilterSetting)) ? (string)appData.Values[LogFilterSetting] : "inf,sum,wrn,err,tra";
            SetLogFilter(logFilterStr);

            LastTabIndex = (appData.Values.ContainsKey(LastTabIndexSetting)) ? (int)appData.Values[LastTabIndexSetting] : 0;

            IsShowingProfileSettings = (appData.Values.ContainsKey(IsShowingProfileSettingsSetting)) ? (bool)appData.Values[IsShowingProfileSettingsSetting] : false;
        }

        public void SaveSettings()
        {
            var appData = ApplicationData.Current.LocalSettings;
            appData.Values[MainPageTarget] = Target;
            SaveTargetList();
            appData.Values[SelectedProfile] = Profile;
            appData.Values[LastTabIndexSetting] = LastTabIndex;
            appData.Values[IsShowingProfileSettingsSetting] = IsShowingProfileSettings;

            string logFilterStr = GetLogFilter();
            appData.Values[LogFilterSetting] = logFilterStr;

        }

        private void LoadTargetList()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.TryGetValue(MainPageTargetList, out object value))
            {
                try
                {
                    string historyJson = value as string;
                    if (!string.IsNullOrEmpty(historyJson))
                    {
                        // Deserialize the JSON string to a List<string>
                        List<string> historyItems = JsonSerializer.Deserialize<List<string>>(historyJson);
                        if (historyItems.Count == 0)
                        {

                        }
                        else
                        {
                            if (historyItems != null)
                            {
                                foreach (var item in historyItems)
                                {
                                    TargetList.Add(item);
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
            else
            {

            }
        }

        private void SaveTargetList()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            try
            {
                string historyJson = JsonSerializer.Serialize(TargetList);
                localSettings.Values[MainPageTargetList] = historyJson;
            }
            catch (Exception)
            {

            }
        }

        public void CalcState()
        {

            CanSelectTarget = (
                (!IsAddingTarget) &&
                (!IsRemovingTarget) &&
                (!IsProfileCloning) &&
                (!IsProfileRenaming) &&
                (!IsProfileDeleting)
            );

            CanAddTarget = (
                (!IsAddingTarget) &&
                (!IsAddingTarget) &&
                (!IsProfileCloning) &&
                (!IsProfileRenaming) &&
                (!IsProfileDeleting)
            );

            CanRemoveTarget = (
                (!IsAddingTarget) &&
                (!IsRemovingTarget) &&
                (!IsProfileCloning) &&
                (!IsProfileRenaming) &&
                (!IsProfileDeleting)
            );

            CanSelectProfile = (
                (!IsAddingTarget) &&
                (!IsRemovingTarget) &&
                (!IsProfileCloning) &&
                (!IsProfileRenaming) &&
                (!IsProfileDeleting)
            );

            CanProfileClone = (
                (!IsProfileCloning) &&
                (!IsProfileRenaming) &&
                (!IsProfileDeleting) &&
                (!IsAddingTarget) &&
                (!IsRemovingTarget)
            );
            CanProfileRename = (
                (!IsProfileCloning) &&
                (!IsProfileRenaming) &&
                (!IsProfileDeleting) &&
                (!IsAddingTarget) &&
                (!IsRemovingTarget)
            );
            CanProfileDelete = (
                (!IsProfileCloning) &&
                (!IsProfileRenaming) &&
                (!IsProfileDeleting) &&
                (Profiles.Count > 1) &&
                (!IsAddingTarget) &&
                (!IsRemovingTarget)
            );

            CanToggleProfileSettings = (
                (!IsProfileCloning) &&
                (!IsProfileRenaming) &&
                (!IsProfileDeleting) &&
                (Profiles.Count > 1) &&
                (!IsAddingTarget) &&
                (!IsRemovingTarget)
            );

            CanSelectTarget = (
                (!IsAddingTarget) &&
                (!IsRemovingTarget) &&
                (!IsProfileCloning) &&
                (!IsProfileRenaming) &&
                (!IsProfileDeleting)
            );

            CanSelectTarget = (
                (!IsAddingTarget) &&
                (!IsRemovingTarget) &&
                (!IsProfileCloning) &&
                (!IsProfileRenaming) &&
                (!IsProfileDeleting)
            );

            CanShowLog = (
                ((LastTabIndex != 3) && (LastTabIndex != 4))
            );

        }

        #region TARGET
        [ObservableProperty]
        private bool _canSelectTarget;

        [ObservableProperty]
        private bool _canAddTarget;

        [ObservableProperty]
        private bool _canRemoveTarget;

        [ObservableProperty]
        private string _target;
        partial void OnTargetChanged(string value)
        {
            if (value == "") { return; }
            Profiles.Clear();
            TUtils.CalcPaths(Target);
            IsTargetPathInvalid = !TUtils.RootPathIsValid;
            TargetNotConfigured = !TUtils.IsConfigured;
            IsValidConfiguredPath = ((!IsTargetPathInvalid) && (!TargetNotConfigured));
            if (IsValidConfiguredPath)
            {
                GetProfiles();
                //TCacheEx.Load(TUtils.TargetTranslatorCachePath);
                WeakReferenceMessenger.Default.Send(new TTargetChanged(value));
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
            CalcState();
        }

        [ObservableProperty]
        private bool _isRemovingTarget;

        [RelayCommand]
        private async Task RemoveTarget()
        {
            IsRemovingTarget = true;
            CalcState();
            var window = App.m_window;
            if (window?.Content is FrameworkElement rootElement)
            {
                var res = await Dialogs.ShowConfirmation(rootElement.FlowDirection, rootElement.RequestedTheme, rootElement.XamlRoot, "Delete this Target?", Target);
                if (res == ContentDialogResult.Primary)
                {
                    int index = TargetList.IndexOf(Target);
                    if (index != -1)
                    {
                        TargetList.RemoveAt(index);
                        if (TargetList.Count > 0)
                        {
                            Target = TargetList[0];
                        }
                    }
                }
            }
            IsRemovingTarget = false;
            CalcState();
        }

        [RelayCommand]
        private void AddTargetCancel()
        {
            IsAddingTarget = false;
            CalcState();
        }

        [RelayCommand]
        private void AddTargetSave()
        {
            if (InputTarget.Trim() != "")
            {
                TargetList.Add(InputTarget.Trim());
                IsAddingTarget = false;
                Target = InputTarget.Trim();
                CalcState();
            }
        }
        #endregion

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
                if (prevSelectedProfile != null)
                {
                    Profile = prevSelectedProfile;
                }

            }
        }

        [ObservableProperty]
        private string _profile;
        partial void OnProfileChanged(string? oldValue, string newValue)
        {
            if ((oldValue != null)) { prevSelectedProfile = oldValue; }
            if ((newValue == null)) { return; }
            WeakReferenceMessenger.Default.Send(new TProfileChanged(newValue));
        }

        [ObservableProperty]
        private bool _canSelectProfile;

        public string prevSelectedProfile;

        #region CLONE

        [ObservableProperty]
        private bool _canProfileClone;

        [ObservableProperty]
        private bool _isProfileCloning = false;
        partial void OnIsProfileCloningChanged(bool value)
        {
            ProfileCloneName = "";
        }

        [ObservableProperty]
        private string _ProfileCloneName;
        partial void OnProfileCloneNameChanged(string value)
        {
            if (IsProposedProfileNameValid(value))
            {
                IsCloneNameInvalid = false;

            }
            else
            {
                IsCloneNameInvalid = true;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                IsCloneNameInvalid = true;
                return;
            }
        }

        private bool IsProposedProfileNameValid(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }
            char[] invalidChars = Path.GetInvalidFileNameChars();
            if (value.Any(c => invalidChars.Contains(c)))
            {
                return false;
            }

            string path = Path.Combine(TUtils.TargetProfilesPath, value + ".prf");
            if (File.Exists(path))
            {
                return false;
            }
            return true;
        }

        [ObservableProperty]
        private bool _isCloneNameInvalid = true;

        [RelayCommand]
        private void ProfileClone()
        {
            IsProfileCloning = true;
            CalcState();
        }

        [RelayCommand]
        private void ProfileCloneSave()
        {
            //copy the .pref and give it the new file the clone name
            string srcPath = Path.Combine(TUtils.TargetProfilesPath, Profile + ".prf");
            string clonePath = Path.Combine(TUtils.TargetProfilesPath, ProfileCloneName + ".prf");
            File.Copy(srcPath, clonePath, true);

            //force a reload of the profiles list
            GetProfiles();

            //select this new clone
            Profile = ProfileCloneName;

            IsProfileCloning = false;
            CalcState();
        }


        [RelayCommand]
        private void ProfileCloneCancel()
        {
            IsProfileCloning = false;
            CalcState();
        }

        #endregion

        #region RENAME
        [ObservableProperty]
        private bool _canProfileRename;

        [ObservableProperty]
        private bool _isProfileRenaming;

        [ObservableProperty]
        private string _ProfileRenameName;
        partial void OnProfileRenameNameChanged(string value)
        {
            if (IsProposedProfileNameValid(value))
            {
                IsProfileRenameNameInvalid = false;

            }
            else
            {
                IsProfileRenameNameInvalid = true;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                IsProfileRenameNameInvalid = true;
                return;
            }
            CalcState();
        }

        [ObservableProperty]
        private bool _isProfileRenameNameInvalid = true;

        [RelayCommand]
        private void ProfileRename()
        {
            ProfileRenameName = Profile;
            IsProfileRenaming = true;
            CalcState();
        }

        [RelayCommand]
        private void ProfileRenameSave()
        {
            string srcPath = Path.Combine(TUtils.TargetProfilesPath, Profile + ".prf");
            string clonePath = Path.Combine(TUtils.TargetProfilesPath, ProfileRenameName + ".prf");
            File.Move(srcPath, clonePath, true);
            GetProfiles();
            Profile = ProfileRenameName;
            IsProfileRenaming = false;
            CalcState();
        }

        [RelayCommand]
        private void ProfileRenameCancel()
        {

            IsProfileRenaming = false;
            CalcState();
        }
        #endregion

        #region DELETE
        [ObservableProperty]
        private bool _canProfileDelete;

        [ObservableProperty]
        private bool _isProfileDeleting;

        [RelayCommand]
        private async Task ProfileDelete()
        {
            IsProfileDeleting = true;
            CalcState();
            var window = App.m_window;
            if (window?.Content is FrameworkElement rootElement)
            {
                var res = await Dialogs.ShowConfirmation(rootElement.FlowDirection, rootElement.RequestedTheme, rootElement.XamlRoot, "Delete this Profile?", Profile);
                if (res == ContentDialogResult.Primary)
                {
                    string srcPath = Path.Combine(TUtils.TargetProfilesPath, Profile + ".prf");
                    File.Delete(srcPath);
                    GetProfiles();
                    Profile = Profiles[0];
                }
            }
            IsProfileDeleting = false;
            CalcState();
        }

        [ObservableProperty]
        private bool _canToggleProfileSettings;

        [ObservableProperty]
        private bool _isShowingProfileSettings;

        [RelayCommand]
        private void ToggleProfileSettings()
        {
            if ((IsShowingProfileSettings) && ((LastTabIndex == 2) || (LastTabIndex == 3)))
            {
                LastTabIndex = 1;
            }
            IsShowingProfileSettings = !IsShowingProfileSettings;
        }

        #endregion
        #endregion

        #region CONTENT
        [ObservableProperty]
        private int _lastTabIndex = -1;
        partial void OnLastTabIndexChanged(int value)
        {
            AdjustTabs();
        }

        private void AdjustTabs()
        {
            if (LastTabIndex == 3 || LastTabIndex == 4)
            {
                SecondRowHeight = new GridLength(1, GridUnitType.Star);
                ThirdRowHeight = new GridLength(1, GridUnitType.Auto);
            }
            else
            {
                SecondRowHeight = new GridLength(1, GridUnitType.Auto);
                ThirdRowHeight = new GridLength(1, GridUnitType.Star);

            }
            //SecondRowHeight = (LastTabIndex == 3) ? new GridLength(1, GridUnitType.Star)
            //    : new GridLength(1, GridUnitType.Auto);
            //ThirdRowHeight = (LastTabIndex == 3) ? new GridLength(1, GridUnitType.Auto)
            //    : new GridLength(1, GridUnitType.Star);
            CalcState();
        }

        [ObservableProperty]
        private GridLength _secondRowHeight;

        [ObservableProperty]
        private GridLength _thirdRowHeight;
        #endregion

        #region LOG
        [ObservableProperty]
        private bool _canShowLog;


        public ObservableCollection<TLogItemEx> LogItems = new();

        public ObservableCollection<TLogItemExFilter> LogFilters = new();

        public void SetLogFilter(string filter)
        {
            LogFilters.Clear();
            LogFilters.Add(new TLogItemExFilter("Translations", "tra", false));
            LogFilters.Add(new TLogItemExFilter("Summary", "sum", false));
            LogFilters.Add(new TLogItemExFilter("Info", "inf", false));
            LogFilters.Add(new TLogItemExFilter("Error", "err", false));
            LogFilters.Add(new TLogItemExFilter("Warning", "wrn", false));
            LogFilters.Add(new TLogItemExFilter("Debug", "dbg", false));

            string[] activeFilters = filter.Split(',');

            foreach (TLogItemExFilter item in LogFilters)
            {
                item.IsChecked = activeFilters.Contains(item.Value);
            }
        }

        public string GetLogFilter()
        {
            List<string> filter = [];
            foreach (TLogItemExFilter item in LogFilters)
            {
                if (item.IsChecked)
                {
                    filter.Add(item.Value);
                }
            }
            return string.Join(",", filter);
        }

        public void AddLogItem(TLogItem item)
        {
            string sep = ucLogHelper.SepMedium;
            TLogItemEx newItem = new TLogItemEx(
                ucLogHelper.GetLogTextColor(item.ItemType),
                LogItems.Count.ToString(),
                item.ItemType,
                item.Indent,
                item.Message,
                (((item.Data == null) || (item.Data.Count == 0)) ? false : true),
                item.Data,
                (((item.Data == null) || (item.Data.Count == 0)) ? "" : sep + Environment.NewLine + string.Join(Environment.NewLine + sep + Environment.NewLine, item.Data)) + Environment.NewLine + sep,
                item.ItemType.ToString() + ":" + item.Message
            );
            LogItems.Add(newItem);
        }

        public void SaveLog(TLog.eLogType mode)
        {
            string sep = ucLogHelper.SepMedium;
            List<string> log = new();
            foreach(TLogItemEx item in LogItems)
            {
                string lineNumber = item.LineNumber.PadLeft(4);
                string type = String.Format("[{0}]", item.Type.ToString()).ToUpper();
                string msg = new string(' ', item.Indent) + item.Message.Trim();
                if ((item.Data != null) && (item.Data.Count >= 0))
                {
                    foreach (string dataItem in item.Data)
                    {
                        msg = msg + Environment.NewLine + new string(' ', 14) + dataItem.Replace(Environment.NewLine, Environment.NewLine + new string(' ', 14));
                    }
                }
                string res = String.Format("{0}: {1} {2}", lineNumber,  type, msg);
                log.Add(res);
            }
            string logStr = string.Join(Environment.NewLine, log);
            //save
            switch (mode)
            {
                case TLog.eLogType.Scan: File.WriteAllText(TUtils.TargetScanLogPath, logStr); break;
                case TLog.eLogType.Translate: File.WriteAllText(TUtils.TargetTranslateLogPath, logStr); break;
                case TLog.eLogType.ProfileTest: File.WriteAllText(TUtils.TargetProfileTestLogPath, logStr); break;
            }

        }

        #endregion

    }

}