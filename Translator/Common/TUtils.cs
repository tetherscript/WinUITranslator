﻿using Microsoft.UI.Xaml.Data;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System;
using Microsoft.UI.Xaml.Controls;

namespace Translator
{
    public static class TUtils
    {
        public static string TargetRootPath = string.Empty;
        public static string TargetTranslatorPath = string.Empty;
        public static string TargetTranslatorXamlElementsPath = string.Empty;
        public static string TargetTranslatorTLocalizedGetsPath = string.Empty;
        public static string TargetTranslatorDetectedXamlElementsPath = string.Empty;
        public static string TargetTranslatorHintsPath = string.Empty;
        public static string TargetTranslatorSpecialsPath = string.Empty;
        public static string TargetStringsPath = string.Empty;
        public static string TargetStrings_enUS_Path = string.Empty;
        public static string TargetScanLogPath = string.Empty;
        public static string TargetTranslateLogPath = string.Empty;

        public static bool CalcPaths(string targetRootPath)
        {
            if (targetRootPath != null)
            {
                if (Directory.Exists(targetRootPath.Trim()))
                {
                    TargetRootPath = targetRootPath;
                    TargetTranslatorPath = Path.Combine(TargetRootPath, "Translator");
                    TargetTranslatorXamlElementsPath = Path.Combine(TargetRootPath, @"Translator\XamlElements.json");
                    TargetTranslatorTLocalizedGetsPath = Path.Combine(TargetRootPath, @"Translator\TLocalizedGets.json");
                    TargetTranslatorDetectedXamlElementsPath = Path.Combine(TargetRootPath, @"Translator\DetectedXamlElements.json");
                    TargetTranslatorHintsPath = Path.Combine(TargetRootPath, @"Translator\Hints.json");
                    TargetTranslatorSpecialsPath = Path.Combine(TargetRootPath, @"Translator\Specials.json");
                    TargetStringsPath = Path.Combine(TargetRootPath, "Strings");
                    TargetStrings_enUS_Path = Path.Combine(TargetStringsPath, @"en-US\Resources.resw");
                    TargetScanLogPath = Path.Combine(TargetRootPath, @"Translator\ScanLog.txt");
                    TargetTranslateLogPath = Path.Combine(TargetRootPath, @"Translator\TranslateLog.txt");
                    return true;
                }
                else
                {
                    TargetTranslatorPath = "";
                    TargetTranslatorXamlElementsPath = "";
                    TargetTranslatorTLocalizedGetsPath = "";
                    TargetTranslatorDetectedXamlElementsPath = "";
                    TargetTranslatorHintsPath = "";
                    TargetTranslatorSpecialsPath = "";
                    TargetStringsPath = "";
                    TargetStrings_enUS_Path = "";
                    TargetScanLogPath = "";
                    TargetTranslateLogPath = "";
                    return false;
                }
            }
            else return false;
        }

        public class HintKeyValEntry
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        #region SPECIALS
        private static Dictionary<string, string> Specials = new Dictionary<string, string>();

        public class SpecialItem
        {
            public string Key { get; set; }
            public string Value { get; set; }
            public string Culture { get; set; }
        }

        public static List<SpecialItem> SpecialItems;

        public static void LoadSpecials(string path)
        {
            string loadedJson = File.ReadAllText(path);
            // Deserialize to a list of LocalizedEntry
            var newEntries = JsonSerializer.Deserialize<List<HintKeyValEntry>>(
                loadedJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            SpecialItems = JsonSerializer.Deserialize<List<SpecialItem>>(loadedJson);
        }
        #endregion

    }

    public class InvBoolConv : IValueConverter
    {
        // Convert from source to target (invert the value)
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolean)
                return !boolean;
            return false;
        }

        // Convert back from target to source (optional, invert again)
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolean)
                return !boolean;
            return false;
        }
    }


    public class BoolToOpacityConv : IValueConverter
    {
        // Convert from source to target (invert the value)
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolean)
                return (((bool)value) ? (double)100.0f : (double)0.0f);
            return (double)100.0f;
        }

        // Convert back from target to source (optional, invert again)
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is int integer)
                return ((double)value == (double)100 ? true : false);
            return false;
        }
    }

}
