using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using TeeLocalized;
using Windows.Storage;

namespace Translator;

public class TContent_openai_api
{
    public string translated { get; set; }
    public int confidence { get; set; }
}

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

public class TTranslatorResult(bool isSuccessful, string untranslatedText, string translatedText, int confidence, string reasoning, List<string> data = null)
{
    public bool IsSuccessful { get; set; } = isSuccessful;
    public string UntranslatedText { get; set; } = untranslatedText;
    public string TranslatedText { get; set; } = translatedText;
    public int Confidence { get; set; } = confidence;
    public string Reasoning { get; set; } = reasoning;
    public List<string> Data { get; set; } = data;
}



public class ProgressReport
{
    public int? PercentComplete { get; set; }
    public TLogItem LogItem { get; set; }
    public ProgressReport(int? percentComplete, TLogItem logItem)
    {
        PercentComplete = percentComplete;
        LogItem = logItem;
    }
}

public class TTranslatorExProc
{
    private CancellationTokenSource? cts = new();

    public async Task TranslateStart(TLog.eLogType mode, string target, string profile, CancellationTokenSource cancellationToken, bool saveToCache)
    {

        string s = TUtils.TargetTranslatorPath;
        var progressReporter = new Progress<ProgressReport>(report =>
        {
            if (report.PercentComplete != null)
            {
                App.Vm.TranslateProgress = (int)report.PercentComplete;
            }
            App.Vm.AddLogItem(report.LogItem);
        });

        try
        {
            var TranslatorEx = new TTranslatorEx();
            await TranslatorEx.RunBackgroundTaskAsyncTranslate(mode, target, profile, progressReporter, cancellationToken.Token, saveToCache);
        }
        catch (OperationCanceledException)
        {

        }
    }

    public async Task ProfileTestStart(TLog.eLogType mode, string target, string profile, CancellationTokenSource cancellationToken, string textToTranslate, int repeats, string toCulture)
    {

        string s = TUtils.TargetTranslatorPath;
        var progressReporter = new Progress<ProgressReport>(report =>
        {
            if (report.PercentComplete != null)
            {
                if (report.LogItem.LogType == TLog.eLogType.Translate)
                {
                    WeakReferenceMessenger.Default.Send(new TTranslateProgress((int)report.PercentComplete));
                }
                else
                if (report.LogItem.LogType == TLog.eLogType.ProfileTest)
                {
                    WeakReferenceMessenger.Default.Send(new TTestProgress((int)report.PercentComplete));
                }
            };
                
            WeakReferenceMessenger.Default.Send(new TAddProfileTestLogItem(report.LogItem));
        });

        try
        {
            var TranslatorEx = new TTranslatorEx();
            await TranslatorEx.RunBackgroundTaskAsyncProfileTest(mode, target, profile, progressReporter, cancellationToken.Token, textToTranslate, repeats, toCulture);
        }
        catch (OperationCanceledException)
        {

        }
    }
}

public partial class TTranslatorEx
{
    public async Task RunBackgroundTaskAsyncTranslate(TLog.eLogType mode, string target, string profile, IProgress<ProgressReport> progressReport, CancellationToken cancellationToken, bool saveToCache)
    {
        TTranslateBatchResult result = await Task.Run(async () =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new TTranslateBatchResult { IsSuccessful = false };
            }

            bool result = false;
            result = await TranslateBatch(target, profile, progressReport, cancellationToken, saveToCache);
            return new TTranslateBatchResult { IsSuccessful = result };
        }, cancellationToken);
        return;
    }

    public async Task RunBackgroundTaskAsyncProfileTest(TLog.eLogType mode, string target, string profile, IProgress<ProgressReport> progressReport, CancellationToken cancellationToken, string textToTranslate, int repeats, string toCulture)
    {
        TTranslateBatchResult result = await Task.Run(async () =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new TTranslateBatchResult { IsSuccessful = false };
            }

            bool result = false;
            result = await ProfileTestTranslateBatch(target, profile, progressReport, cancellationToken, textToTranslate, repeats, toCulture);
            return new TTranslateBatchResult { IsSuccessful = result };
        }, cancellationToken);
        return;
    }




    private async Task<bool> TranslateBatch(string target, string profile, IProgress<ProgressReport> report, CancellationToken cancellationToken, bool saveToCache)
    {

        int _progress = 0;

        void Log(TLog.eLogItemType logType, int indent, string msg, List<string> details)
        {
            report?.Report(new ProgressReport(_progress,
                new TLogItem(TLog.eLogType.Translate, logType, indent, msg, details)));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            Log(TLog.eLogItemType.err, 0, "Cancelled.", null);
            return false;
        }


        if (!TUtils.CalcPaths(target))
        {
            Log(TLog.eLogItemType.err, 0, "Target root path does not exist: " + Environment.NewLine + target, null);
            return false;
        }
        else
        {
            Log(TLog.eLogItemType.inf, 0, "Target: " + target, null);
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
                Log(TLog.eLogItemType.inf, 0, "Profile: " + profile, null);
            }
            catch (Exception ex)
            {
                Log(TLog.eLogItemType.err, 0, String.Format(
                    "Error loading profile settings: {0} : {1}", ex.Message, path), null);
                return false;
            }
        }


        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        TUtils.CalcPaths(target);
        string fromCulture = "en-US";
        Log(TLog.eLogItemType.inf, 0, "fromCulture: " + fromCulture, null);


        #region CACHE
        StorageFolder x = await StorageFolder.GetFolderFromPathAsync(TUtils.TargetTranslatorPath);
        TCache.Init(x);
        TCache.InitializeAsync().Wait();
        Log(TLog.eLogItemType.dbg, 0, "Cache initialized.", null);
        #endregion

        #region TRANSLATE
        int _cacheHitCounter = 0;
        int _cacheMissCounter = 0;
        int _failedTranslationCounter = 0;
        int _nonEnUSLanguages = 0;

        if (!File.Exists(TUtils.TargetStrings_enUS_Path))
        {
            Log(TLog.eLogItemType.err, 0, String.Format("File does not exist: {0}", TUtils.TargetStrings_enUS_Path), null);
        }
        XDocument enUSDoc = XDocument.Load(TUtils.TargetStrings_enUS_Path);

        var reswFiles = Directory.GetFiles(TUtils.TargetStringsPath, "Resources.resw", SearchOption.AllDirectories);

        Log(TLog.eLogItemType.inf, 0, String.Format("Found {0} Resources.resw files.", reswFiles.Count().ToString()), null);

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

        int _translateableCount = 0;

        foreach (var destDocFilePath in reswFiles)
        {
            string toCulture = Path.GetFileName(Path.GetDirectoryName(destDocFilePath)); //en-US, ar-SA etc
            if (toCulture != "en-US")
            {
                _nonEnUSLanguages++;

                XDocument destDoc = XDocument.Load(destDocFilePath);
                Log(TLog.eLogItemType.tra, 0, String.Format("Processing {0}...", toCulture), null);

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
                        Log(TLog.eLogItemType.err, 0, "Cancelled.", null);
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
                            Log(TLog.eLogItemType.dbg, 2, "Cache hit: " + cacheKey, null);
                            _cacheHitCounter++;
                        }
                        else
                        {
                            _cacheMissCounter++;

                            string s1, s2;
                            s1 = "Cache miss: Translating...";
                            Log((TSettings.Debug ? TLog.eLogItemType.dbg : TLog.eLogItemType.dbg), 2, s1, null);
                            s2 = hintToken + textToTranslate;
                            Log(TLog.eLogItemType.tra, 1, s2, null);

                            TTranslatorResult translatorResult;
                            textToTranslate = TUtils.EscapePlaceholders(textToTranslate);
                            translatorResult = await Translate(TLog.eLogType.Translate, profile, fromCulture, toCulture, textToTranslate, hintToken, Settings, report, cancellationToken);


                            if (!translatorResult.IsSuccessful)
                            {
                                translatedText = "--- FAILED ---";
                                _failedTranslationCounter++;
                                if (_failedTranslationCounter >= 5)
                                {
                                    Log(TLog.eLogItemType.err, 2, "Too many translation errors (max 5).  Cancelling...", translatorResult.Data);
                                    return false;
                                }
                                continue;
                            }
                            else
                            {
                                translatedText = TUtils.UnescapePlaceholders(translatorResult.TranslatedText.Trim());
                                Log(TLog.eLogItemType.tra, 2, translatedText, translatorResult.Data);
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

                    _progValue++;
                    _progress = (int)((float)_progValue / (float)_progMax * 100.0f);

                }

                //add the specials
                Log(TLog.eLogItemType.inf, 2, "Checking for specials...", null);
                if (SpecialItems.Count == 0)
                {
                    Log(TLog.eLogItemType.inf, 4, SpecialItems.Count.ToString() + " specials found.", null);
                }
                foreach (var item in SpecialItems.Where(item => item.Culture == toCulture))
                {
                    Log(TLog.eLogItemType.inf, 4, "Adding special: " + item.Key.ToString() + "=" + item.Value.ToString(), null);

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
        Log(TLog.eLogItemType.sep, 0, TLog.SepWide, null);
        if (_unTranslateables.Count > 0)
        {
            Log(TLog.eLogItemType.sum, 0, "Missing the following xaml element property hint tokens: " + _unTranslateables.Count().ToString(), null);
            foreach (var item in _unTranslateables)
            {
                Log(TLog.eLogItemType.sum, 2, "  " + item, null);
            }
        }
        Log(TLog.eLogItemType.sum, 0, String.Format(@"Found {0} translateable items in en-US\Resources.resw", _translateableCount), null);
        Log(TLog.eLogItemType.sum, 0, String.Format("{0} cache hits", _cacheHitCounter), null);
        Log(TLog.eLogItemType.sum, 0, String.Format("{0} cache misses", _cacheMissCounter), null);
        Log(TLog.eLogItemType.sum, 0, String.Format("{0} translation attempts", _cacheMissCounter), null);
        Log(TLog.eLogItemType.sum, 0, String.Format("{0} failed translations", _failedTranslationCounter), null);

        #endregion

        //DONE
        Log(TLog.eLogItemType.inf, 0, TLog.SepWide, null);
        Log(TLog.eLogItemType.inf, 0, "Translation complete.", null);
        stopwatch.Stop();
        TimeSpan elapsed = stopwatch.Elapsed;
        string elapsedCustomFormat = elapsed.ToString(@"hh\:mm\:ss");
        Log(TLog.eLogItemType.inf, 0, "Elapsed Time: " + elapsedCustomFormat, null);

        return true;
    }

    private async Task<bool> ProfileTestTranslateBatch(string target, string profile, IProgress<ProgressReport> report, CancellationToken cancellationToken, string textToTranslate, int repeats, string toCulture)
    {
        int _progress = 0;

        void Log(TLog.eLogItemType logType, int indent, string msg, List<string> details)
        {
            report?.Report(new ProgressReport(_progress,
                new TLogItem(TLog.eLogType.ProfileTest, logType, indent, msg, details)));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            Log(TLog.eLogItemType.err, 0, "Cancelled.", null);
            return false;
        }


        if (!TUtils.CalcPaths(target))
        {
            Log(TLog.eLogItemType.err, 0, "Target root path does not exist: " + Environment.NewLine + target, null);
            return false;
        }
        else
        {
            Log(TLog.eLogItemType.inf, 0, "Target: " + target, null);
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
                Log(TLog.eLogItemType.inf, 0, "Profile: " + profile, null);
            }
            catch (Exception ex)
            {
                Log(TLog.eLogItemType.err, 0, String.Format(
                    "Error loading profile settings: {0} : {1}", ex.Message, path), null);
                return false;
            }
        }

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        string fromCulture = "en-US";
        Log(TLog.eLogItemType.inf, 0, "fromCulture: " + fromCulture, null);

        #region TRANSLATE
        int _failedTranslationCounter = 0;

        int _progValue = 0;
        int _progMax = 100;

        List<string> tranItems = new();
        string[] lines = textToTranslate.Split(new[] { "\r" }, StringSplitOptions.RemoveEmptyEntries);
        _progMax = lines.Count() * repeats;
        foreach (string item in lines)
        {

            if (item.StartsWith("//"))
            {
                _progValue++;
                _progress = (int)((float)_progValue / (float)_progMax * 100.0f);
                continue;
            }
            else
            {
                Log(TLog.eLogItemType.tra, 0, item, null);
                (string hintToken, string content) = TLocalized.SplitPrefix(item);
                if (TLocalized.IsValidHintToken(hintToken))
                {
                    for (int i = 0; i < repeats; i++)
                    {
                        _progValue++;
                        _progress = (int)((float)_progValue / (float)_progMax * 100.0f);

                        if (cancellationToken.IsCancellationRequested)
                        {
                            Log(TLog.eLogItemType.err, 0, "Cancelled.", null);
                            return false;
                        }

                        TTranslatorResult translatorResult;
                        string escapedTextToTranslate = TUtils.EscapePlaceholders(content);
                        translatorResult = await Translate(TLog.eLogType.ProfileTest, profile, fromCulture, toCulture, escapedTextToTranslate, hintToken, Settings, report, cancellationToken);
                        if (!translatorResult.IsSuccessful)
                        {
                            _failedTranslationCounter++;
                            if (_failedTranslationCounter >= 5)
                            {
                                Log(TLog.eLogItemType.err, 1, "Too many translation errors (max 5).  Cancelling...", translatorResult.Data);
                                return false;
                            }
                            continue;
                        }
                        else
                        {
                            string translatedText = TUtils.UnescapePlaceholders(translatorResult.TranslatedText.Trim());
                            Log(TLog.eLogItemType.tra, 1, translatedText, translatorResult.Data);
                        }
                    }
                }
            }
            Log(TLog.eLogItemType.sep, 0, TLog.SepWide, null);
        }
        #endregion

        stopwatch.Stop();
        TimeSpan elapsed = stopwatch.Elapsed;
        string elapsedCustomFormat = elapsed.ToString(@"hh\:mm\:ss");
        _progress = 100;
        Log(TLog.eLogItemType.inf, 0, "Elapsed Time: " + elapsedCustomFormat, null);

        return true;
    }

}