using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Translator.Log
{
    public partial class TLogItemEx(string lineNumber, TLog.eLogItemType type, bool? isSuccessful, string untranslatedText, string? translatedText, int? confidence, string? reasoning, int indent, string? message, SolidColorBrush textColor, bool hasProfileResults) : ObservableObject
    {
        [ObservableProperty]
        private string _lineNumber = lineNumber;

        [ObservableProperty]
        private DateTime _timeStamp = DateTime.Now;

        [ObservableProperty]
        private TLog.eLogItemType _type = type;

        [ObservableProperty]
        private bool? _isSuccessful = isSuccessful;

        [ObservableProperty]
        private string _untranslatedText = untranslatedText;

        [ObservableProperty]
        private string? _translatedText = translatedText;

        [ObservableProperty]
        private int? _confidence = confidence;

        [ObservableProperty]
        private string? _reasoning = reasoning;

        [ObservableProperty]
        private int? _indent = indent;

        [ObservableProperty]
        private string? _message = message;

        [ObservableProperty]
        private string _filterableStr = untranslatedText + ":" + translatedText + ":" + reasoning + ":" + message;

        [ObservableProperty]
        SolidColorBrush _textColor = textColor;

        [ObservableProperty]
        private bool _hasProfileResults = hasProfileResults;

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

