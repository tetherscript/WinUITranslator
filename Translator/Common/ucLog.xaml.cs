using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.Widgets.Notifications;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.ViewManagement;

namespace Translator.Log
{
    public partial class TLogItemEx(SolidColorBrush textColor, string lineNumber, TLog.eLogItemType type, int indent, string message,
        bool hasDetail, List<string> details, string filterableStr) : ObservableObject
    {
        [ObservableProperty]
        private string _lineNumber = lineNumber;

        [ObservableProperty]
        private DateTime _timeStamp = DateTime.Now;

        [ObservableProperty]
        private TLog.eLogItemType _type = type;

        [ObservableProperty]
        private int _indent = indent;

        [ObservableProperty]
        private Thickness _margin = new Thickness(indent * 10, 2, 4, 2);  //Margin="4,2,4,2"

        [ObservableProperty]
        private string _message = message;

        [ObservableProperty]
        private bool _hasDetail = hasDetail;

        [ObservableProperty]
        private List<string> _details = details;

        [ObservableProperty]
        private string _filterableStr = filterableStr;

        [ObservableProperty]
        SolidColorBrush _textColor = textColor;
    }

    public partial class TLogItemExFilter(string display, string value, bool isChecked) : ObservableObject
    {
        public string Display = display;
        public string Value = value;

        [ObservableProperty]
        private bool _isChecked = isChecked;
    }

    public partial class TVm : ObservableObject
    {
        private void Filter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TLogItemExFilter.IsChecked))
            {
                AdjustLogFilter();
            }
        }

        [ObservableProperty]
        private string _searchStr = string.Empty;
        partial void OnSearchStrChanged(string value)
        {
            AdjustLogFilter();
        }

        public ObservableCollection<TLogItemExFilter> LogFilters;

        public AdvancedCollectionView FilteredLogItems;

        public void LinkFilterEvents()
        {
            foreach (TLogItemExFilter filter in LogFilters)
            {
                filter.PropertyChanged -= Filter_PropertyChanged;
                filter.PropertyChanged += Filter_PropertyChanged;
            }
        }

        public void AdjustLogFilter()
        {
            if (FilteredLogItems == null) { return; }
            if (LogFilters == null) { return; }
            using (FilteredLogItems.DeferRefresh())
            {
                string searchText = SearchStr;
                FilteredLogItems.SortDescriptions.Clear();
                FilteredLogItems.SortDescriptions.Add(new SortDescription("TimeStamp", SortDirection.Ascending));
                List<string> typeFilter = [];
                foreach (TLogItemExFilter item in LogFilters)
                {
                    if (item.IsChecked) { typeFilter.Add(item.Value); }
                }
                if (searchText != "") { typeFilter.Add(searchText); }
                FilteredLogItems.ClearObservedFilterProperties();
                FilteredLogItems.Filter = x =>
                (
                    typeFilter.Contains
                    (
                        ((TLogItemEx)x).Type.ToString()
                    )
                    &&
                    ((TLogItemEx)x).FilterableStr.Contains(searchText, StringComparison.CurrentCultureIgnoreCase)
                );
            }
        }
    }

    public sealed partial class ucLog : UserControl
    {
        private readonly TVm _vm = new();
        public TVm Vm { get => _vm; }

        public ucLog()
        {
            this.InitializeComponent();
        }

        #region LOGITEMS
        public static readonly DependencyProperty LogItemsProperty =
            DependencyProperty.Register(
                nameof(LogItems),
                typeof(ObservableCollection<TLogItemEx>),
                typeof(ucLog),
                null
            );

        public ObservableCollection<TLogItemEx> LogItems
        {
            get { return (ObservableCollection<TLogItemEx>)GetValue(LogItemsProperty); }
            set { 
                SetValue(LogItemsProperty, value); 
                _vm.FilteredLogItems = new(LogItems, true);
                _vm.AdjustLogFilter();
            }
        }
        #endregion

        #region FILTERS
        public static readonly DependencyProperty LogFiltersProperty =
            DependencyProperty.Register(
                nameof(LogFilters),
                typeof(ObservableCollection<TLogItemExFilter>),
                typeof(ucLog),
                null

            );

        public ObservableCollection<TLogItemExFilter> LogFilters
        {
            get { return (ObservableCollection<TLogItemExFilter>)GetValue(LogFiltersProperty); }
            set { 
                SetValue(LogFiltersProperty, value);
                _vm.LogFilters = value;
                _vm.LinkFilterEvents();
                _vm.AdjustLogFilter();
            }
        }
        #endregion

    }
}

