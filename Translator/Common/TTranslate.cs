using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;

namespace Translator
{
    public static class TTranslate
    {
        public static bool IsCancelled = false;
        private static string _tsDown = new string('┬', 30);
        private static string _tsNeutral = new string('━', 30);
        private static string _tsUp = new string('┴', 30);

        public class SpecialItem
        {
            public string Key { get; set; }
            public string Value { get; set; }
            public string Culture { get; set; }
        }

        public static async Task Start(string targetRootPath, string translationFunction)
        {
            IsCancelled = false;
            TLog.Reset();
            TLog.Log("Translating...");
            TLog.Log("Translation function: " + translationFunction);
            try
            {
                await TranslateAsync(
                    translationFunction,
                    targetRootPath);

                // Once the operation finishes, you can update UI accordingly
                TLog.Log("Translation complete.");
            }
            catch (Exception ex)
            {
                TLog.Log($"An error occurred: {ex.Message}", true);
            }
            TLog.Save(TUtils.TargetTranslateLogPath);
        }

        public static void Stop()
        {
            IsCancelled = true;
        }

        public static async Task TranslateAsync(
          string translationFunction,
          string targetRootPath)
        {
            TUtils.CalcPaths(targetRootPath);
            App.Vm.TranslateProgress = 0;

            string fromCulture = "en-US";

            if (!TTransFunc.InitGlobal(translationFunction, fromCulture))
            {
                TLog.Log(String.Format("TTransFunc.InitGlobal failed: translationFunction={0}, fromCulture={1}",
                    translationFunction, fromCulture), true);
                return;
            }

            TLog.Log(_tsNeutral);
            TLog.Log("TRANSLATION FUNCTION DETAILS");
            TLog.Log(TTransFunc.GetDescription(translationFunction));
            TLog.Log(_tsNeutral);

            #region CACHE
            StorageFolder x = await StorageFolder.GetFolderFromPathAsync(TUtils.TargetTranslatorPath);
            TCache.Init(x);
            await TCache.InitializeAsync();
            TLog.Log("Cache initialized.");
            #endregion

            #region TRANSLATE
            int _cacheHitCounter = 0;
            int _cacheMissCounter = 0;
            int _failedTranslationCounter = 0;
            int _nonEnUSLanguages = 0;

            if (!File.Exists(TUtils.TargetStrings_enUS_Path))
            {
                TLog.Log(String.Format("File does not exist: {0}", TUtils.TargetStrings_enUS_Path), true);
            }
            XDocument enUSDoc = XDocument.Load(TUtils.TargetStrings_enUS_Path);

            var reswFiles = Directory.GetFiles(TUtils.TargetStringsPath, 
                "Resources.resw", SearchOption.AllDirectories);

            TLog.Log(String.Format("Found {0} Resources.resw files.", reswFiles.Count().ToString()));

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

                    XDocument destDoc = XDocument.Load(destDocFilePath);
                    TLog.Log(String.Format("Processing {0}...", toCulture));

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
                            TLog.Log("Cancelled.", true);
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
                            string cacheKey = String.Format("{0}:{1}:{2}", toCulture, hintToken, textToTranslate);
                            string cachedData = TCache.GetValue(cacheKey);
                            if (cachedData != null)
                            {
                                translatedText = cachedData;
                                TLog.Log(String.Format("  Cache hit: {0}:{1}:{2}", toCulture, hintToken, textToTranslate));
                                _cacheHitCounter++;
                            }
                            else
                            {
                                _cacheMissCounter++;

                                if (!hasAlreadyInittedThisCulture)
                                {
                                    TTransFunc.InitPerCulture(translationFunction, fromCulture, toCulture);
                                    hasAlreadyInittedThisCulture = true;
                                }

                                translatedText = TTransFunc.Translate(translationFunction, fromCulture, toCulture, textToTranslate, hintToken);
                                TLog.Log(String.Format("  Cache miss: {0}:{1}:{2} --> {3}", toCulture, hintToken, textToTranslate, translatedText));
                                if (translatedText == null)
                                {
                                    //null returned, so skip it - could be a bad translation or critical failed attempt?
                                    _failedTranslationCounter++;
                                    TLog.Log(String.Format("TTransFunc.Translate failed: translationFunction={0}, fromCulture={1}," +
                                        "toCulture={2}, hintToken={3}, textToTranslate={4}",
                                        translationFunction, fromCulture, toCulture, hintToken, textToTranslate), true);
                                    continue;
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
                    TLog.Log("  Checking for specials...");
                    foreach (var item in SpecialItems.Where(item => item.Culture == toCulture))
                    {
                        TLog.Log("  Adding special: " + item.Key.ToString() + "=" + item.Value.ToString());

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

            TLog.Log(_tsNeutral);

            TLog.LogInsert(_tsNeutral);

            string smx = string.Empty;
            string sm = "The {0} translateable items in en-US\\Resources.resw were translated to {1} languages with {2} cache hits and {3} misses.";
            if (_cacheMissCounter > 0)
            {
                sm = sm + Environment.NewLine + "There were {4} calls to the Translation Function with {5} failures.";
                smx = String.Format(sm,
                    _translateableCount,
                    _nonEnUSLanguages,
                    _cacheHitCounter,
                    _cacheMissCounter,
                    _cacheMissCounter,
                    _failedTranslationCounter);
            }
            else
            {
                smx = String.Format(sm,
                    _translateableCount,
                    _nonEnUSLanguages,
                    _cacheHitCounter,
                    _cacheMissCounter);
            }
            TLog.LogInsert(smx);
            if (_failedTranslationCounter > 0)
            {
                TLog.LogInsert("Some translations have failed: Examine log closely to troubleshoot.", true);
            }
            TLog.LogInsert("SUMMARY");
            TLog.LogInsert(_tsNeutral);

            #endregion

        }

    }
}
