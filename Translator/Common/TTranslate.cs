using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;
using System.Diagnostics;

namespace Translator
{
    public static class TTranslate
    {
        public static bool IsCancelled = false;

        public class SpecialItem
        {
            public string Key { get; set; }
            public string Value { get; set; }
            public string Culture { get; set; }
        }

        public static async Task StartTest(TLog.eMode mode, string targetRootPath, string translationFunction, string textToTranslate)
        {
            IsCancelled = false;
            TLog.Reset(TLog.eMode.tfTranslate);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                await TranslateTestAsync(
                    mode,
                    translationFunction,
                    targetRootPath,
                    textToTranslate);

                stopwatch.Stop();
                TimeSpan elapsed = stopwatch.Elapsed;
                string elapsedCustomFormat = elapsed.ToString(@"hh\:mm\:ss");
                TLog.Log(mode, TLog.eLogItemType.inf, 0, "Elapsed Time: " + elapsedCustomFormat);
            }
            catch (Exception ex)
            {
                TLog.Log(mode, TLog.eLogItemType.err, 0, $"An error occurred: {ex.Message}");
            }
            TLog.Save(TLog.eMode.tfTranslate, TUtils.TargetTranslateLogPath);
        }

        public static async Task TranslateTestAsync(TLog.eMode mode, string translationFunction, string targetRootPath, string textToTranslate)
        {
            TUtils.CalcPaths(targetRootPath);
            App.Vm.TranslateProgress = 0;

            string fromCulture = "en-US";

            await Task.Delay(1000);

        }















        
        public static async Task Start(TLog.eMode mode, string targetRootPath, string translationFunction)
        {
            IsCancelled = false;
            TLog.Reset(TLog.eMode.translate);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            TLog.Log(TLog.eMode.translate, TLog.eLogItemType.inf, 0, "Translation Started.");
            TLog.Log(TLog.eMode.translate, TLog.eLogItemType.inf, 0, "Translation function: " + translationFunction);
            try
            {
                await TranslateAsync(
                    mode,
                    translationFunction,
                    targetRootPath);

                TLog.LogSeparator(TLog.eMode.translate, TLog.eLogSeparatorType.lineWide);
                TLog.Log(TLog.eMode.translate, TLog.eLogItemType.inf, 0, "Translation complete.");
                stopwatch.Stop();
                TimeSpan elapsed = stopwatch.Elapsed;
                string elapsedCustomFormat = elapsed.ToString(@"hh\:mm\:ss");
                TLog.Log(TLog.eMode.translate, TLog.eLogItemType.inf, 0, "Elapsed Time: " + elapsedCustomFormat);
            }
            catch (Exception ex)
            {
                TLog.Log(TLog.eMode.translate, TLog.eLogItemType.err, 0, $"An error occurred: {ex.Message}");
            }
            TLog.Save(TLog.eMode.translate, TUtils.TargetTranslateLogPath);
        }

        public static void Stop()
        {
            IsCancelled = true;
        }

        public static async Task TranslateAsync(
          TLog.eMode mode,
          string translationFunction,
          string targetRootPath)
        {
            TUtils.CalcPaths(targetRootPath);
            App.Vm.TranslateProgress = 0;

            string fromCulture = "en-US";

            if (!TTransFunc.InitGlobal(mode, translationFunction, fromCulture))
            {
                TLog.Log(TLog.eMode.translate, TLog.eLogItemType.err, 0, String.Format("TTransFunc.InitGlobal failed: translationFunction={0}, fromCulture={1}",
                    translationFunction, fromCulture));
                return;
            }

            #region CACHE
            StorageFolder x = await StorageFolder.GetFolderFromPathAsync(TUtils.TargetTranslatorPath);
            TCache.Init(x);
            await TCache.InitializeAsync(); 
            TLog.Log(TLog.eMode.translate, TLog.eLogItemType.inf, 0, "Cache initialized.");


            #endregion

            #region TRANSLATE
            int _cacheHitCounter = 0;
            int _cacheMissCounter = 0;
            int _failedTranslationCounter = 0;
            int _nonEnUSLanguages = 0;

            int _debugRetranslateCounter = 0;
            bool _debugRetranslateBypass = false;

            if (!File.Exists(TUtils.TargetStrings_enUS_Path))
            {
                TLog.Log(TLog.eMode.translate, TLog.eLogItemType.err, 0, String.Format("File does not exist: {0}", TUtils.TargetStrings_enUS_Path));
            }
            XDocument enUSDoc = XDocument.Load(TUtils.TargetStrings_enUS_Path);

            var reswFiles = Directory.GetFiles(TUtils.TargetStringsPath, 
                "Resources.resw", SearchOption.AllDirectories);

            TLog.Log(TLog.eMode.translate, TLog.eLogItemType.inf, 0, String.Format("Found {0} Resources.resw files.", reswFiles.Count().ToString()));

            //load specials
            List<SpecialItem> SpecialItems;
            string loadedJson = File.ReadAllText(TUtils.TargetTranslatorSpecialsPath);
            var newEntries = JsonSerializer.Deserialize<List<TUtils.HintKeyValEntry>>(
                loadedJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            SpecialItems = JsonSerializer.Deserialize<List<SpecialItem>>(loadedJson);
            
            int _progValue = 0;
            int _progMax = 100;
            DateTime _lastASync = DateTime.Now;

            int _translateableCount = 0;

            bool hasAlreadyInittedThisCulture = false;
            foreach (var destDocFilePath in reswFiles)
            {
                string toCulture = Path.GetFileName(Path.GetDirectoryName(destDocFilePath)); //en-US, ar-SA etc
                if (toCulture != "en-US")
                {
                    _nonEnUSLanguages++;
                    hasAlreadyInittedThisCulture = false;

                    _debugRetranslateCounter = 0;

                    XDocument destDoc = XDocument.Load(destDocFilePath);
                    TLog.Log(TLog.eMode.translate, TLog.eLogItemType.inf, 0, String.Format("Processing {0}...", toCulture));

                    //reset destDoc
                    destDoc.Descendants("data")
                        .Where(x => 1 == 1)
                        .Remove();

                    var enUSDocDataElements = enUSDoc.Descendants("data").ToList();

                    _translateableCount = enUSDocDataElements.Count();

                    _progMax = enUSDocDataElements.Count() * (reswFiles.Count() - 1);

                    foreach (var item in enUSDocDataElements)
                    {
                        if (IsCancelled)
                        {
                            TLog.Log(TLog.eMode.translate, TLog.eLogItemType.err, 0, "Cancelled.");
                            return;
                        }

                        //get translation from cache or call api
                        string translatedText = string.Empty;
                        string cacheIndicator = string.Empty;
                        string textToTranslate = item.Element("value").Value;
                        string hintToken = item.Element("comment").Value;

                        //reject names with periods in it 'close.btn' or 'loading....';

                        if ((hintToken != "!") && (hintToken != "@") && (hintToken != "@@") && (hintToken != "!!"))
                        {
                            //if there's a x:Uid, we usuall expect hint tokens
                            //if translationHint is not !, !!, @ or @@ prefix, so we won't translate or cache it
                        }
                        else
                        {
                            if (TSettings.Debug)
                            {
                                if (TSettings.DebugRetranslate)
                                {
                                   _debugRetranslateCounter++;
                                    _debugRetranslateBypass = (_debugRetranslateCounter <= TSettings.DebugRetranslateItemsCount);
                                }
                            }
                            
                            string cacheKey = String.Format("{0}:{1}:{2}", toCulture, hintToken, textToTranslate);
                            string cachedData = TCache.GetValue(cacheKey);
                            if ((cachedData != null) && (!_debugRetranslateBypass))
                            {
                                translatedText = cachedData;
                                TLog.Log(TLog.eMode.translate, TLog.eLogItemType.inf, 2, String.Format("Cache hit: {0}:{1}:{2}", toCulture, hintToken, textToTranslate));
                                _cacheHitCounter++;
                            }
                            else
                            {
                                _cacheMissCounter++;

                                if (!hasAlreadyInittedThisCulture)
                                {
                                    TTransFunc.InitPerCulture(mode, translationFunction, fromCulture, toCulture);
                                    hasAlreadyInittedThisCulture = true;
                                }


                                string s1, s2;
                                s1 = "Cache miss: Translating...";
                                TLog.Log(TLog.eMode.translate, (TSettings.Debug ? TLog.eLogItemType.dbg : TLog.eLogItemType.inf), 2, s1);
                                s2 = String.Format(
                                     "Untranslated: {0}:{1}{2}",
                                     toCulture,
                                     hintToken,
                                     textToTranslate
                                     );
                                TLog.Log(TLog.eMode.translate, TLog.eLogItemType.inf, 4, s2);
                                translatedText = TTransFunc.Translate(mode, translationFunction, fromCulture, toCulture, textToTranslate, hintToken);
                                if (translatedText == null)
                                {
                                    translatedText = "--- FAILED ---";
                                    _failedTranslationCounter++;
                                    if (_failedTranslationCounter >= 5)
                                    {
                                        TLog.Log(TLog.eMode.translate, TLog.eLogItemType.err, 4, "Too many translation errors (max 5).  Cancelling...");
                                        IsCancelled = true;
                                    }
                                    continue;
                                }
                                else
                                {
                                    String.Format("Result: " + translatedText);
                                    TLog.Log(TLog.eMode.translate, TLog.eLogItemType.inf, 4, s2);
                                }

                            await TCache.AddEntryAsync(cacheKey, translatedText);
                            }
                            var desDocDataElement = new XElement("data",
                                new XAttribute("name", item.Attribute("name").Value),
                                new XAttribute(XNamespace.Xml + "space", "preserve"),
                                new XElement("value", translatedText),
                                new XElement("comment", item.Element("comment").Value));

                            destDoc.Root.Add(desDocDataElement);
                        }

                        TimeSpan ts = DateTime.Now - _lastASync;
                        if (ts.TotalMilliseconds > (1000 / 60)) //120hz
                        {
                            _lastASync = DateTime.Now;
                            await Task.Delay(1);
                        }
                        
                        _progValue++;
                        App.Vm.TranslateProgress = (int)((float)_progValue/(float)_progMax * 100.0f);

                    }

                    //add the specials
                    TLog.Log(TLog.eMode.translate, TLog.eLogItemType.inf, 0, "  Checking for specials...");
                    foreach (var item in SpecialItems.Where(item => item.Culture == toCulture))
                    {
                        TLog.Log(TLog.eMode.translate, TLog.eLogItemType.inf, 0, "  Adding special: " + item.Key.ToString() + "=" + item.Value.ToString());

                        var desDocDataElement1 = new XElement("data",
                            new XAttribute("name", item.Key.ToString()),
                            new XAttribute(XNamespace.Xml + "space", "preserve"),
                            new XElement("value", item.Value.ToString()),
                            new XElement("comment", item.Culture.ToString()));

                        destDoc.Root.Add(desDocDataElement1);

                    }
                    destDoc.Save(destDocFilePath);

                }
            }

            TLog.LogSeparator(TLog.eMode.translate, TLog.eLogSeparatorType.lineWide);
            TLog.Log(TLog.eMode.translate, TLog.eLogItemType.inf, 0, String.Format(@"Summary: Found {0} translateable items in en-US\Resources.resw", _translateableCount));
            TLog.Log(TLog.eMode.translate, TLog.eLogItemType.inf, 0, String.Format("Summary: {0} cache hits", _cacheHitCounter));
            TLog.Log(TLog.eMode.translate, TLog.eLogItemType.inf, 0, String.Format("Summary: {0} cache misses", _cacheMissCounter));
            TLog.Log(TLog.eMode.translate, TLog.eLogItemType.inf, 0, String.Format("Summary: {0} translation attempts", _cacheMissCounter));
            TLog.Log(TLog.eMode.translate, TLog.eLogItemType.inf, 0, String.Format("Summary: {0} failed translations", _failedTranslationCounter));
            if (TSettings.Debug)
            {
                if (_debugRetranslateCounter > 0)
                {
                    string s = String.Format("Summary: Retranslated the top {0} items for each toCulture.",
                        TSettings.DebugRetranslateItemsCount.ToString());
                    TLog.Log(TLog.eMode.translate, TLog.eLogItemType.dbg, 0, s);
                }
            }
            TTransFunc.DeInitGlobal(mode, translationFunction);

            #endregion

        }

    }
}
