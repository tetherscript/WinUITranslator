using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.IO;
using System.Linq;

namespace Translator
{
    public sealed partial class ProfilePage : Page
    {
        public TVm GVm = App.Vm;

        public TPageVm Vm = new();

        public partial class TPageVm : ObservableObject
        {
            public void CalcState()
            {
                CanClone = (
                    (!IsCloning) &&
                    (!IsRenaming) &&
                    (!IsDeleting)
                );
                CanRename = (
                    (!IsCloning) &&
                    (!IsRenaming) &&
                    (!IsDeleting)
                );
                CanDelete = (
                    (!IsCloning) &&
                    (!IsRenaming) &&
                    (!IsDeleting) && 
                    (App.Vm.Profiles.Count > 1)
                );

            }

            #region CLONE
            [ObservableProperty]
            private bool _canClone;

            [ObservableProperty]
            private bool _isCloning = false;
            partial void OnIsCloningChanged(bool value)
            {
                CloneName = "";
            }

            [ObservableProperty]
            private string _cloneName;
            partial void OnCloneNameChanged(string value)
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
            private async void Clone()
            {
                IsCloning = true;
                CalcState();
            }

            [RelayCommand]
            private async void CloneSave()
            {
                //copy the .pref and give it the new file the clone name
                string srcPath = Path.Combine(TUtils.TargetProfilesPath, App.Vm.SelectedProfile + ".prf");
                string clonePath = Path.Combine(TUtils.TargetProfilesPath, CloneName + ".prf");
                File.Copy(srcPath, clonePath, true);

                //force a reload of the profiles list
                App.Vm.GetProfiles();

                //select this new clone
                App.Vm.SelectedProfile = CloneName;

                IsCloning = false;
                CalcState();
            }


            [RelayCommand]
            private async void CloneCancel()
            {
                IsCloning = false;
                CalcState();
            }

            #endregion

            #region RENAME
            [ObservableProperty]
            private bool _canRename;

            [ObservableProperty]
            private bool _isRenaming;

            [ObservableProperty]
            private string _renameName;
            partial void OnRenameNameChanged(string value)
            {
                if (IsProposedProfileNameValid(value))
                {
                    IsRenameNameInvalid = false;

                }
                else
                {
                    IsRenameNameInvalid = true;
                }

                if (string.IsNullOrWhiteSpace(value))
                {
                    IsRenameNameInvalid = true;
                    return;
                }
            }

            [ObservableProperty]
            private bool _isRenameNameInvalid = true;

            [RelayCommand]
            private async void Rename()
            {
                RenameName = App.Vm.SelectedProfile;
                IsRenaming = true;
                CalcState();
            }

            [RelayCommand]
            private async void RenameSave()
            {
                string srcPath = Path.Combine(TUtils.TargetProfilesPath, App.Vm.SelectedProfile + ".prf");
                string clonePath = Path.Combine(TUtils.TargetProfilesPath, RenameName + ".prf");
                File.Move(srcPath, clonePath, true);
                App.Vm.GetProfiles();
                App.Vm.SelectedProfile = RenameName;
                IsRenaming = false;
                CalcState();
            }

            [RelayCommand]
            private async void RenameCancel()
            {

                IsRenaming = false;
                CalcState();
            }
            #endregion

            #region DELETE
            [ObservableProperty]
            private bool _canDelete;

            [ObservableProperty]
            private bool _isDeleting;

            [RelayCommand]
            private async void Delete()
            {
                IsDeleting = true;
                CalcState();
            }

            [RelayCommand]
            private async void DeleteYes()
            {
                string srcPath = Path.Combine(TUtils.TargetProfilesPath, App.Vm.SelectedProfile + ".prf");
                File.Delete(srcPath);
                App.Vm.GetProfiles();
                App.Vm.SelectedProfile = App.Vm.Profiles[0];
                IsDeleting = false;
                CalcState();
            }

            [RelayCommand]
            private async void DeleteNo()
            {
                IsDeleting = false;
                CalcState();
            }
            #endregion
        }

        public ProfilePage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            Vm.CalcState();
        }

    }
}
