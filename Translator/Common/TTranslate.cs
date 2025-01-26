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
        public static int ProgressPerc = 0;
        public static bool IsCancelled = false;

        public class SpecialItem
        {
            public string Key { get; set; }
            public string Value { get; set; }
            public string Culture { get; set; }
        }

        public static async Task Start(string targetRootPath, string translationFunction, Dictionary<string, string> hints)
        {
            IsCancelled = false;
            TLog.Reset();
            TLog.Log("Translating...");
            TLog.Log("Translation function: " + translationFunction);
            try
            {
                await TranslateAsync(
                    translationFunction,
                    targetRootPath,
                    hints);

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
          string targetRootPath,
          Dictionary<string, string> hints)
        {
            TUtils.CalcPaths(targetRootPath);
            ProgressPerc = 0;

            #region CACHE
            JsonDatabase cache;
            StorageFolder x = await StorageFolder.GetFolderFromPathAsync(TUtils.TargetTranslatorPath);
            cache = null;
            cache = new(x);
            await cache.InitializeAsync();
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

            foreach (var destDocFilePath in reswFiles)
            {
                string culture_Name = Path.GetFileName(Path.GetDirectoryName(destDocFilePath)); //en-US, ar-SA etc
                if (culture_Name != "en-US")
                {
                    _nonEnUSLanguages++;

                    XDocument destDoc = XDocument.Load(destDocFilePath);
                    TLog.Log(String.Format("Processing {0}...", culture_Name));

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
                        string originalText = item.Element("value").Value;
                        string translationHint = item.Element("comment").Value;

                        //reject names with periods in it 'close.btn' or 'loading....';

                        if ((translationHint != "!") && (translationHint != "@") && (translationHint != "@@") && (translationHint != "!!"))
                        {
                            //if there's a x:Uid, we usuall expect hint tokens
                            //if translationHint is not !, !!, @ or @@ prefix, so we won't translate or cache it
                        }
                        else
                        {
                            string cacheKey = String.Format("{0}:{1}:{2}", culture_Name, translationHint, originalText);
                            string cachedData = cache.GetValue(cacheKey);
                            if (cachedData != null)
                            {
                                translatedText = cachedData;
                                TLog.Log(String.Format("  Cache hit: {0}:{1}:{2}", culture_Name, translationHint, originalText));
                                _cacheHitCounter++;
                            }
                            else
                            {
                                _cacheMissCounter++;
                                translatedText = TTransFunc.Translate(hints, translationFunction, originalText, culture_Name, translationHint);
                                TLog.Log(String.Format("  Cache miss: {0}:{1}:{2} --> Translated", culture_Name, translationHint, originalText));
                                if (translatedText == null)
                                {
                                    //null returned, so skip it - could be a bad translation or critical failed attempt?
                                    //it is important to know of these, as it could be a missing translation like on an obscure control on a seldom-used page.
                                    _failedTranslationCounter++;
                                    //have the translation function add diagnostics to log
                                    TLog.Log("Failed translation function call.", true);
                                    continue;
                                }
                                await cache.AddEntryAsync(cacheKey, translatedText);
                            }
                            var desDocDataElement = new XElement("data",
                                new XAttribute("name", item.Attribute("name").Value),
                                new XAttribute(XNamespace.Xml + "space", "preserve"),
                                new XElement("value", translatedText),
                                new XElement("comment", item.Element("comment").Value));

                            destDoc.Root.Add(desDocDataElement);
                        }

                        TimeSpan ts = DateTime.Now - _lastASync;
                        if (ts.TotalMilliseconds > (1000 / 120)) //120hz
                        {
                            _lastASync = DateTime.Now;
                            await Task.Delay(1);
                        }
                        
                        _progValue++;
                        ProgressPerc = (int)((float)_progValue/(float)_progMax * 100.0f);

                    }

                    //add the specials
                    TLog.Log("  Checking for specials...");
                    foreach (var item in SpecialItems.Where(item => item.Culture == culture_Name))
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
            TLog.Log("*****************************");
            
            TLog.LogInsert("*****************************");

            //TLog.LogInsert(String.Format(@"Summary: {0} translateable items found in en-US\Resources.resw", _translateableCount));
            //TLog.LogInsert(String.Format("Summary: translated to {0} non-en-US languages", _nonEnUSLanguages));
            //TLog.LogInsert(String.Format("Summary: {0} cache hits", _cacheHitCounter));
            //TLog.LogInsert(String.Format("Summary: {0} cache misses", _cacheMissCounter));
            //TLog.LogInsert(String.Format("Summary: {0} failed translations", _failedTranslationCounter));
            //if (_failedTranslationCounter > 0)
            //{
            //    TLog.LogInsert("Some translations have failed: Examine log closely to troubleshoot.", true);
            //}
            //TLog.LogInsert("****************");

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
            TLog.LogInsert("********** SUMMARY **********");

            #endregion

        }

    }
}
