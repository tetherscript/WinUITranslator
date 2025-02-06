using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using TeeLocalized;
using Windows.Storage;

namespace Translator;

public class SpecialItem
{
    public string Key { get; set; }
    public string Value { get; set; }
    public string Culture { get; set; }
}

public class TTranslateBatchResult()
{
    public bool IsSuccessful { get; set; }
}


public class TTranslatorResult(bool isSuccessful, string UntranslatedText, string? TranslatedText, 
    int? Confidence, string? Reasoning)
{
    public bool IsSuccessful { get; set; } = isSuccessful;
    public string UntranslatedText { get; set; } = UntranslatedText;
    public string? TranslatedText { get; set; } = TranslatedText;
    public int? Confidence { get; set; } = Confidence;
    public string? Reasoning { get; set; } = Reasoning;
}


public class TLogItem(TLog.eMode logType, TLog.eLogItemType itemType, int indent, string message)
{
    public DateTime timeStamp { get; set; }
    public TLog.eMode logType { get; set; } = logType;
    public TLog.eLogItemType itemType { get; set; } = itemType;
    public int indent { get; set; } = indent;
    public string Message { get; set; } = message;
}

public class TranslateProgressReport
{
    public int? PercentComplete { get; set; }
    public TLogItem LogItem { get; set; }
    public TTranslatorResult TranslatorResult { get; set; }
    public TranslateProgressReport(int? percentComplete, TLogItem logItem, TTranslatorResult? translatorResult = null)
    {
        PercentComplete = percentComplete;
        LogItem = logItem;
        TranslatorResult = translatorResult;
    }
}

public class TTranslatorExProc
{
    private CancellationTokenSource? cts = new();

    public async Task Start(TLog.eMode mode, string target, string profile, CancellationTokenSource cancellationToken, bool saveToCache)
    {

        string s = TUtils.TargetTranslatorPath;
        var progressReporter = new Progress<TranslateProgressReport>(report =>
        {
            if (report.PercentComplete != null)
            {
                App.Vm.TranslateProgress = (int)report.PercentComplete;
            }
            //App.Vm.TranslateLog = App.Vm.TranslateLog + report.LogItem.Message + Environment.NewLine;
            //TLog.Log(mode, report.LogItem.itemType, report.LogItem.indent, report.LogItem.Message);
            App.Vm.AddTranslateLogItem(report);
            //App.Vm.FilteredTranslateLog.Add(new TTranslateLogItem(
            //    report.LogItem.itemType,
            //    report.LogItem.
            //    report.LogItem.Message
            //    ));


        });

        try
        {
            var TranslatorEx = new TTranslatorEx();
            await TranslatorEx.RunBackgroundTaskAsync(mode, target, profile, progressReporter, cancellationToken.Token, saveToCache);
        }
        catch (OperationCanceledException)
        {

        }
    }
}


public partial class TTranslatorEx
{
    public async Task RunBackgroundTaskAsync(TLog.eMode mode, string target, string profile, IProgress<TranslateProgressReport> progressReport, CancellationToken cancellationToken, bool saveToCache)
    {
        TTranslateBatchResult result = await Task.Run(async () =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new TTranslateBatchResult { IsSuccessful = false };
            }
            bool result = await TranslateBatch(target, profile, progressReport, cancellationToken, saveToCache);
            return new TTranslateBatchResult { IsSuccessful = result };
        }, cancellationToken);
        return;
    }

    private async Task<bool> TranslateBatch(string target, string profile, IProgress<TranslateProgressReport> report, CancellationToken cancellationToken, bool saveToCache)
    {

        int _progress = 0;

        void Log(TLog.eLogItemType logType, int indent, string msg, TTranslatorResult translatorResult = null)
        {
            if (translatorResult == null)
            {

            }
            report?.Report(new TranslateProgressReport(_progress,
                new TLogItem(TLog.eMode.translate, logType, indent, msg), translatorResult));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            Log(TLog.eLogItemType.err, 0, "Cancelled.");
            return false;
        }

        Dictionary<string, string> Settings = new Dictionary<string, string>();
        string path = Path.Combine(TUtils.TargetProfilesPath, profile + ".prf");
        if (File.Exists(path))
        {
            try
            {
                string loadedSettingsJson = File.ReadAllText(path);
                // Deserialize to a list of LocalizedEntry
                var SettingsnewEntries = JsonSerializer.Deserialize<List<TUtils.SettingsKeyValEntry>>(
                    loadedSettingsJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                Settings.Clear();
                foreach (var entry in SettingsnewEntries)
                {
                    Settings[entry.Key] = entry.Value;
                }
                Log(TLog.eLogItemType.dbg, 2, "Profile settings loaded.");
            }
            catch (Exception ex)
            {
                Log(TLog.eLogItemType.err, 2, String.Format(
                    "Error loading settings: {0} : {1}", ex.Message, path));
                return false;
            }
        }


        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        TUtils.CalcPaths(target);
        string fromCulture = "en-US";

        #region CACHE
        StorageFolder x = await StorageFolder.GetFolderFromPathAsync(TUtils.TargetTranslatorPath);
        TCache.Init(x);
        TCache.InitializeAsync().Wait();
        Log(TLog.eLogItemType.inf, 0, "Cache initialized.");
        #endregion

        #region TRANSLATE
        int _cacheHitCounter = 0;
        int _cacheMissCounter = 0;
        int _failedTranslationCounter = 0;
        int _nonEnUSLanguages = 0;

        if (!File.Exists(TUtils.TargetStrings_enUS_Path))
        {
            Log(TLog.eLogItemType.err, 0, String.Format("File does not exist: {0}", TUtils.TargetStrings_enUS_Path));
        }
        XDocument enUSDoc = XDocument.Load(TUtils.TargetStrings_enUS_Path);

        var reswFiles = Directory.GetFiles(TUtils.TargetStringsPath, "Resources.resw", SearchOption.AllDirectories);

        Log(TLog.eLogItemType.inf, 0, String.Format("Found {0} Resources.resw files.", reswFiles.Count().ToString()));

        List<string> _unTranslateables = [];

        //load specials
        List<SpecialItem> SpecialItems;
        string loadedJson = File.ReadAllText(TUtils.TargetTranslatorSpecialsPath);
        var newEntries = JsonSerializer.Deserialize<List<TUtils.SettingsKeyValEntry>>(
            loadedJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        SpecialItems = JsonSerializer.Deserialize<List<SpecialItem>>(loadedJson);

        int _progValue = 0;
        int _progMax = 100;
        DateTime _lastASync = DateTime.Now;

        int _translateableCount = 0;

        foreach (var destDocFilePath in reswFiles)
        {
            string toCulture = Path.GetFileName(Path.GetDirectoryName(destDocFilePath)); //en-US, ar-SA etc
            if (toCulture != "en-US")
            {
                _nonEnUSLanguages++;

                XDocument destDoc = XDocument.Load(destDocFilePath);
                Log(TLog.eLogItemType.inf, 0, String.Format("Processing {0}...", toCulture));

                //reset destDoc
                destDoc.Descendants("data")
                    .Where(x => 1 == 1)
                    .Remove();

                var enUSDocDataElements = enUSDoc.Descendants("data").ToList();

                _translateableCount = enUSDocDataElements.Count();

                _progMax = enUSDocDataElements.Count() * (reswFiles.Count() - 1);

                foreach (var item in enUSDocDataElements)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Log(TLog.eLogItemType.err, 0, "Cancelled.");
                        return false;
                    }

                    //get translation from cache or call api
                    string translatedText = string.Empty;
                    string cacheIndicator = string.Empty;
                    string textToTranslate = item.Element("value").Value;
                    string hintToken = item.Element("comment").Value;

                    //reject names with periods in it 'close.btn' or 'loading....';

                    if (!TLocalized.IsValidHintToken(hintToken))
                    {
                        //if there's a x:Uid, only properties with hint token prefixes will be translated
                        XElement dataElement = new XElement(item);
                        string nameValue = dataElement.Attribute("name")?.Value;
                        if (!_unTranslateables.Contains(nameValue))
                        {
                            _unTranslateables.Add(nameValue);
                        }
                    }
                    else
                    {

                        string cacheKey = toCulture + ":" + hintToken + ":" + textToTranslate;
                        string cachedData = TCache.GetValue(cacheKey);
                        if (cachedData != null)
                        {
                            translatedText = cachedData;
                            Log(TLog.eLogItemType.inf, 2, "Cache hit: " + cacheKey);
                            _cacheHitCounter++;
                        }
                        else
                        {
                            _cacheMissCounter++;

                            string s1, s2;
                            s1 = "Cache miss: Translating...";
                            Log((TSettings.Debug ? TLog.eLogItemType.dbg : TLog.eLogItemType.inf), 2, s1);
                            s2 = "Untranslated: " + hintToken + textToTranslate;
                            Log(TLog.eLogItemType.inf, 4, s2);

                            TTranslatorResult translatorResult;
                            textToTranslate = TUtils.EscapePlaceholders(textToTranslate);
                            translatorResult = await Translate(TLog.eMode.translate, profile, fromCulture, toCulture, textToTranslate, hintToken, Settings, report, cancellationToken);


                            if (!translatorResult.IsSuccessful)
                            {
                                translatedText = "--- FAILED ---";
                                _failedTranslationCounter++;
                                if (_failedTranslationCounter >= 5)
                                {
                                    Log(TLog.eLogItemType.err, 4, "Too many translation errors (max 5).  Cancelling...", translatorResult);
                                    return false;
                                }
                                continue;
                            }
                            else
                            {
                                string r = translatorResult.TranslatedText.Trim();
                                translatedText = TUtils.UnescapePlaceholders(translatedText.Trim());
                                s2 = "Translated: " + translatedText;
                                Log(TLog.eLogItemType.inf, 4, s2);
                            }

                            if ((profile != "loopback") && (saveToCache))
                            {
                                await TCache.AddEntryAsync(cacheKey, translatedText);
                            }
                        }
                        var desDocDataElement = new XElement("data",
                            new XAttribute("name", item.Attribute("name").Value),
                            new XAttribute(XNamespace.Xml + "space", "preserve"),
                            new XElement("value", translatedText),
                            new XElement("comment", item.Element("comment").Value));

                        destDoc.Root.Add(desDocDataElement);
                    }

                    TimeSpan ts = DateTime.Now - _lastASync;
                    if (ts.TotalMilliseconds > (1000 / 30)) //30hz
                    {
                        _lastASync = DateTime.Now;
                        await Task.Delay(1);
                    }

                    _progValue++;
                    _progress = (int)((float)_progValue / (float)_progMax * 100.0f);

                }

                //add the specials
                Log(TLog.eLogItemType.inf, 0, "  Checking for specials...");
                foreach (var item in SpecialItems.Where(item => item.Culture == toCulture))
                {
                    Log(TLog.eLogItemType.inf, 0, "  Adding special: " + item.Key.ToString() + "=" + item.Value.ToString());

                    var desDocDataElement1 = new XElement("data",
                        new XAttribute("name", item.Key.ToString()),
                        new XAttribute(XNamespace.Xml + "space", "preserve"),
                        new XElement("value", item.Value.ToString()),
                        new XElement("comment", item.Culture.ToString()));

                    destDoc.Root.Add(desDocDataElement1);

                    _unTranslateables.Remove(item.Key.ToString());

                }

                destDoc.Save(destDocFilePath);

            }
        }
        await Task.Delay(300);
        Log(TLog.eLogItemType.inf, 0, TLog.SepWide);
        Log(TLog.eLogItemType.inf, 0, "Missing Xaml element property hint tokens: " + _unTranslateables.Count().ToString());
        foreach (var item in _unTranslateables)
        {
            Log(TLog.eLogItemType.inf, 2, item);
        }

        Log(TLog.eLogItemType.inf, 0, TLog.SepWide);

        Log(TLog.eLogItemType.inf, 0, String.Format(@"Summary: Found {0} translateable items in en-US\Resources.resw", _translateableCount));
        Log(TLog.eLogItemType.inf, 0, String.Format("Summary: {0} cache hits", _cacheHitCounter));
        Log(TLog.eLogItemType.inf, 0, String.Format("Summary: {0} cache misses", _cacheMissCounter));
        Log(TLog.eLogItemType.inf, 0, String.Format("Summary: {0} translation attempts", _cacheMissCounter));
        Log(TLog.eLogItemType.inf, 0, String.Format("Summary: {0} failed translations", _failedTranslationCounter));

        #endregion























        //DONE
        Log(TLog.eLogItemType.inf, 0, TLog.SepWide);
        Log(TLog.eLogItemType.inf, 0, "Translation complete.");
        stopwatch.Stop();
        TimeSpan elapsed = stopwatch.Elapsed;
        string elapsedCustomFormat = elapsed.ToString(@"hh\:mm\:ss");
        Log(TLog.eLogItemType.inf, 0, "Elapsed Time: " + elapsedCustomFormat);

        return true;
    }

}