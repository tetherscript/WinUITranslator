using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Windows.Storage;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using System.Diagnostics;
using TeeLocalized;
using System.Reflection;

namespace Translator
{

    public sealed partial class MainWindow : Window
    {

        #region LIFECYCLE
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr LoadImage(IntPtr hInstance, string lpIconName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

        private const int WM_SETICON = 0x0080;
        private const uint IMAGE_ICON = 1;
        private const uint LR_LOADFROMFILE = 0x00000010;

        public MainWindow()
        {
            this.InitializeComponent();
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Closing += AppWindow_Closing;

            nint MainWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
            ExtendsContentIntoTitleBar = true;

            // Load the icon from file
            AppWindow.SetIcon("Assets\\app.ico");
            IntPtr hIcon = LoadImage(IntPtr.Zero, "Assets\\app.ico", IMAGE_ICON, 256, 256, LR_LOADFROMFILE);
            if (hIcon != IntPtr.Zero)
            {
                // Set the small and large icons for the taskbar and title bar
                SendMessage(MainWindowHandle, WM_SETICON, new IntPtr(0), hIcon); // Small icon
                SendMessage(MainWindowHandle, WM_SETICON, new IntPtr(1), hIcon); // Large icon
            }

            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            tbTitle.Text = "Translator " + versionInfo.FileVersion;

        }

        private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
           SaveWindowState(this);
        }

        private void GrdMain_Loaded(object sender, RoutedEventArgs e)
        {
            grdMain.FlowDirection = (App.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight);
            RestoreWindowState(this);
            CalcPaths();
            SpecialItems = new();
        }
        #endregion

        #region PATHS
        private string _targetRootPath = string.Empty;
        private string _targetTranslatorPath = string.Empty;
        private string _targetTranslatorXamlElementsPath = string.Empty;
        private string _targetTranslatorTLocalizedGetsPath = string.Empty;
        private string _targetTranslatorDetectedXamlElementsPath = string.Empty;
        private string _targetTranslatorHintsPath = string.Empty;
        private string _targetTranslatorSpecialsPath = string.Empty;
        private string _targetStringsPath = string.Empty;
        private string _targetStrings_enUS_Path = string.Empty;
        private bool CalcPaths()
        {
            if (Directory.Exists(tbScanPath.Text.Trim()))
            {
                _targetRootPath = tbScanPath.Text.Trim();
                _targetTranslatorPath = Path.Combine(_targetRootPath, "Translator");
                _targetTranslatorXamlElementsPath = Path.Combine(_targetRootPath, @"Translator\XamlElements.json");
                _targetTranslatorTLocalizedGetsPath = Path.Combine(_targetRootPath, @"Translator\TLocalizedGets.json");
                _targetTranslatorDetectedXamlElementsPath = Path.Combine(_targetRootPath, @"Translator\DetectedXamlElements.json");
                _targetTranslatorHintsPath = Path.Combine(_targetRootPath, @"Translator\Hints.json");
                _targetTranslatorSpecialsPath = Path.Combine(_targetRootPath, @"Translator\Specials.json");
                _targetStringsPath = Path.Combine(_targetRootPath, "Strings");
                _targetStrings_enUS_Path = Path.Combine(_targetStringsPath, @"en-US\Resources.resw");

                LoadHints(_targetTranslatorHintsPath);
                return true;
            }
            else
            {
                LogScan("Invalid Target Path");
                LogTranslate("Invalid Target Path");
                return false;
            }

        }
        #endregion

        #region LOG
        private int _logScanCounter = 0;
        private int _logTranslateCounter = 0;

        private void LogScan(string msg)
        {
            tbScanLog.Text = tbScanLog.Text.Insert(0,
                String.Format(@"{0}: {1}{2}", _logScanCounter.ToString(), msg, Environment.NewLine));
            _logScanCounter++;
        }

        private void LogTranslate(string msg)
        {
            tbTranslateLog.Text = tbTranslateLog.Text.Insert(0,
                String.Format(@"{0}: {1}{2}", _logTranslateCounter.ToString(), msg, Environment.NewLine));
            _logTranslateCounter++;
        }

        #endregion

        #region SETTINGS
        public static class WindowSettingsKeys
        {
            public const string Left = "WindowLeft";
            public const string Top = "WindowTop";
            public const string Width = "WindowWidth";
            public const string Height = "WindowHeight";
            public const string Scale = "WindowScale";
            public const string Path = "Path";
            public const string TranslationFunctionIndex = "TranslationFunctionIndex";
            public const string PivotIndex = "PivotIndex";
        }

        private void SaveWindowState(Window window)
        {
            var appData = ApplicationData.Current.LocalSettings;
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            var size = appWindow.Size;
            var position = appWindow.Position;

            appData.Values[WindowSettingsKeys.Left] = position.X;
            appData.Values[WindowSettingsKeys.Top] = position.Y;
            appData.Values[WindowSettingsKeys.Width] = size.Width;
            appData.Values[WindowSettingsKeys.Height] = size.Height;
            appData.Values[WindowSettingsKeys.Scale] = grdMain.XamlRoot.RasterizationScale;
            appData.Values[WindowSettingsKeys.Path] = tbScanPath.Text;
            appData.Values[WindowSettingsKeys.TranslationFunctionIndex] = cbTranslationFunction.SelectedIndex;
            appData.Values[WindowSettingsKeys.PivotIndex] = pvtInfo.SelectedIndex;
        }

        private void RestoreWindowState(Window window)
        {
            var appData = ApplicationData.Current.LocalSettings;

            //appData.Values.Clear(); //wipe settings in case it gets messed up

            pvtInfo.SelectedIndex = (appData.Values.ContainsKey(WindowSettingsKeys.PivotIndex)) ? 
               (int)appData.Values[WindowSettingsKeys.PivotIndex] : 0;

            cbTranslationFunction.SelectedIndex = (appData.Values.ContainsKey(WindowSettingsKeys.TranslationFunctionIndex)) ?
                (int)appData.Values[WindowSettingsKeys.TranslationFunctionIndex] : 0;

            tbScanPath.Text = (string)appData.Values[WindowSettingsKeys.Path];
            if (!Directory.Exists(tbScanPath.Text))
            {
                tbScanPath.Text = "";
            }

            if (appData.Values.ContainsKey(WindowSettingsKeys.Left) &&
                appData.Values.ContainsKey(WindowSettingsKeys.Top) &&
                appData.Values.ContainsKey(WindowSettingsKeys.Width) &&
                appData.Values.ContainsKey(WindowSettingsKeys.Height) &&
                appData.Values.ContainsKey(WindowSettingsKeys.Scale))
            {
                var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
                var appWindow = AppWindow.GetFromWindowId(windowId);

                var left = (int)appData.Values[WindowSettingsKeys.Left];
                var top = (int)appData.Values[WindowSettingsKeys.Top];
                var width = (int)appData.Values[WindowSettingsKeys.Width];
                var height = (int)appData.Values[WindowSettingsKeys.Height];
                var scale = (double)appData.Values[WindowSettingsKeys.Scale];

                double InitialRasterizationScale = grdMain.XamlRoot.RasterizationScale;
                double InitialScaleFactor = InitialRasterizationScale / scale;

                appWindow.Move(new PointInt32 { X = left, Y = top });
                appWindow.Resize(new SizeInt32 { 
                    Width = (int)Math.Truncate(width * InitialScaleFactor),
                    Height = (int)Math.Truncate(height * InitialScaleFactor) });
            }
            else
            {
                // Set default size and position if no settings are found
                var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
                var appWindow = AppWindow.GetFromWindowId(windowId);
                appWindow.Move(new PointInt32 { X = 100, Y = 100 });
                appWindow.Resize(new SizeInt32 { Width = 800, Height = 600 });
            }
        }
        #endregion

        #region SPECIALS
        private Dictionary<string, string> Specials = new Dictionary<string, string>();

        public class SpecialItem
        {
            public string Key { get; set; }
            public string Value { get; set; }
            public string Culture { get; set; }
        }

        private List<SpecialItem> SpecialItems;

        public void LoadSpecials(string path)
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

        #region HINTS
        private Dictionary<string, string> Hints = new Dictionary<string, string>();

        public class HintKeyValEntry
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        public void LoadHints(string path)
        {
            string loadedJson = File.ReadAllText(path);
            // Deserialize to a list of LocalizedEntry
            var newEntries = JsonSerializer.Deserialize<List<HintKeyValEntry>>(
                loadedJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            foreach (var entry in newEntries)
            {
                Hints[entry.Key] = entry.Value;
            }
            tb1.Text = Hints["@"];
            tb2.Text = Hints["@@"];
            tb3.Text = Hints["!"];
            tb4.Text = Hints["!!"];
        }

        private void BtnSaveHints_Click(object sender, RoutedEventArgs e)
        {
            SaveHints(_targetTranslatorHintsPath);
        }

        public void SaveHints(string path)
        {
            Hints["@"] = tb1.Text;
            Hints["@@"] = tb2.Text;
            Hints["!"] = tb3.Text;
            Hints["!!"] = tb4.Text;

            var entries = Hints.Select(kvp => new HintKeyValEntry
            {
                Key = kvp.Key,
                Value = kvp.Value
            }).ToList();
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            string jsonOutput = JsonSerializer.Serialize(entries, jsonOptions);
            File.WriteAllText(path, jsonOutput);
        }
        #endregion

        #region SCAN
        private Dictionary<string, string[]> elementTextProperties = new Dictionary<string, string[]>
        {
            //{ "Button",       new[] { "Content" } },
            //{ "TextBlock",    new[] { "Text" } },
            //{ "CheckBox",     new[] { "Content" } },
            //{ "ToggleSwitch",     new[] { "Header", "OffContent", "OnContent" } },
            //{ "RadioButtons",  new[] { "Header" } },
            //{ "RadioButton",  new[] { "Content" } },
            //{ "ToggleButton", new[] { "Content" } }
        };

        public class LocalizedEntry
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        private async void btnScan_Click(object sender, RoutedEventArgs e)
        {
            btnScan.IsEnabled = false;
            await Task.Delay(1000);
            if (CalcPaths())
            {
                await LoadScanDictionaryAsync();
                ScanXaml();
            }
            await Task.Delay(1000);
            btnScan.IsEnabled = true;
        }

        public async Task LoadScanDictionaryAsync()
        {
            try
            {
                if (!File.Exists(_targetTranslatorXamlElementsPath))
                {
                    LogScan(String.Format("Cannot find {0}", _targetTranslatorXamlElementsPath));
                    return;
                }

                string jsonString = await File.ReadAllTextAsync(_targetTranslatorXamlElementsPath);
                elementTextProperties = JsonSerializer.Deserialize<Dictionary<string, string[]>>(jsonString);
            }
            catch (Exception ex)
            {
                LogScan($"Error XamlElements.json from JSON: {ex.Message}");
            }
        }

        public async Task SaveScanDictionaryAsync()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true // For pretty-printing
                };

                string jsonString = JsonSerializer.Serialize(elementTextProperties, options);
                await File.WriteAllTextAsync(_targetTranslatorXamlElementsPath, jsonString);
                Console.WriteLine("Dictionary successfully saved to JSON.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving dictionary to JSON: {ex.Message}");
            }
        }

        private class PageInfo
        {
            public string Link { get; set; }
            public string PhysicalPath { get; set; }
        }

        private void ScanXaml()
        {
            LogScan("---------------------------------------");
            LogScan("Scanning target project: " + _targetRootPath);

            var entriesMap = new Dictionary<string, string>();

            var xamlFiles = Directory.GetFiles(_targetRootPath, "*.xaml", SearchOption.AllDirectories);
            foreach (var filePath in xamlFiles)
            {
                if (!filePath.Contains(@"\obj\"))
                {
                    LogScan("Found: " + filePath);
                    ProcessXamlFile(filePath, entriesMap);
                }

            }

            //----------------------------------------------------
            //Add linked project files, like a .xaml that exists in a /common folder not under the project folder
            string projName = Path.GetFileName(_targetRootPath);
            string csprojPath = Path.Combine(_targetRootPath, String.Format("{0}.csproj", projName));

            if (string.IsNullOrWhiteSpace(csprojPath))
                throw new ArgumentException("csprojPath cannot be null or empty.", nameof(csprojPath));

            if (!File.Exists(csprojPath))
                throw new FileNotFoundException("The specified .csproj file does not exist.", csprojPath);

            // Load the .csproj XML
            LogScan("Checking .csproj for linked .xaml's...");
            XDocument csprojXml;
            try
            {
                csprojXml = XDocument.Load(csprojPath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to load the .csproj file.", ex);
            }

            // Define the XML namespace if present (commonly the default MSBuild namespace)
            XNamespace ns = csprojXml.Root.Name.Namespace;

            // Query all <Page> elements with Include attribute ending with .xaml
            var pageElements = csprojXml.Descendants(ns + "Page")
                                        .Where(e =>
                                            e.Attribute("Include") != null &&
                                            e.Attribute("Include").Value.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase));

            var pages = new List<PageInfo>();
            string csprojDirectory = Path.GetDirectoryName(csprojPath);

            foreach (var page in pageElements)
            {
                string includePath = page.Attribute("Include").Value;
                string linkPath = page.Attribute("Link")?.Value;

                if (string.IsNullOrEmpty(linkPath))
                {
                    // If Link attribute is missing, you might want to handle it differently
                    // For now, we'll skip such entries
                    continue;
                }

                // Resolve the physical path
                string physicalPath = Path.GetFullPath(Path.Combine(csprojDirectory, includePath));
                

                // Optionally, verify if the physical file exists
                if (!File.Exists(physicalPath))
                {
                    Console.WriteLine($"Warning: The file '{physicalPath}' does not exist.");
                    // You can choose to skip or include it regardless
                }

                pages.Add(new PageInfo
                {
                    Link = linkPath,
                    PhysicalPath = physicalPath
                });

                LogScan("Found linked .xaml: " + physicalPath);
                ProcessXamlFile(physicalPath, entriesMap);
            }


            //merge with manual entries
            LogScan("Merging TLocalizedGets.json");
            if (!File.Exists(_targetTranslatorTLocalizedGetsPath))
            {
                LogScan($"Merge file not found: {_targetTranslatorTLocalizedGetsPath}");
                return;
            }

            try
            {
                string loadedJson = File.ReadAllText(_targetTranslatorTLocalizedGetsPath);

                // Deserialize to a list of LocalizedEntry
                var newEntries = JsonSerializer.Deserialize<List<LocalizedEntry>>(
                    loadedJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (newEntries == null)
                {
                    LogScan("Scanning: " + "No entries found in " + _targetTranslatorTLocalizedGetsPath);
                    return;
                }

                // Merge them into entriesMap
                foreach (var entry in newEntries)
                {
                    if (!entriesMap.ContainsKey(entry.Key))
                    {
                        // Add the new entry
                        entriesMap[entry.Key] = entry.Value;
                    }
                }
                LogScan($"Merged {newEntries.Count} entries from {_targetTranslatorTLocalizedGetsPath}");
            }
            catch (Exception ex)
            {
                LogScan($"Scanning: Error merging JSON file: {ex.Message}");
            }
            
            SaveAsJson(_targetTranslatorDetectedXamlElementsPath, entriesMap);
            LogScan("Updated: " + _targetStrings_enUS_Path);
            LogScan("Updated: " + _targetTranslatorDetectedXamlElementsPath);
            LogScan("Scan complete.");

            UpdateEnUSReswFile(entriesMap, _targetStrings_enUS_Path);
        }

        private void ProcessXamlFile(string filePath, Dictionary<string, string> entriesMap)
        {
            try
            {
                XDocument xdoc = XDocument.Load(filePath);

                // The XAML namespace for x: attributes (usually "http://schemas.microsoft.com/winfx/2006/xaml")
                XNamespace xNs = "http://schemas.microsoft.com/winfx/2006/xaml";

                // Get all elements that have an x:Uid attribute
                var elementsWithUid = xdoc.Descendants()
                    .Where(e => e.Attributes(xNs + "Uid").Any());

                foreach (var element in elementsWithUid)
                {
                    // Read the x:Uid value
                    string uid = element.Attribute(xNs + "Uid")?.Value;
                    if (string.IsNullOrWhiteSpace(uid))
                        continue;

                    //reject items that have invalid XAM/ c# identifiers in the key as this will cause app start fails
                    if (!TLocalized.IsValidXamlIdentifier(uid))
                    {
                        LogScan(String.Format("  ***Rejected {0}:x:Uid='{1}' as it is an invalid XAML/C# resource identifier.", filePath, uid));
                        continue;
                        //// XAML/C# rules: The first character must be a letter or underscore.
                        ///// Subsequent characters can be letters, digits, or underscores.
                    }

                    // The element's local name (e.g. "Button", "TextBlock", etc.)
                    string localName = string.Empty;
                    if (element.Name.NamespaceName.StartsWith("using:"))
                    {
                        localName = String.Format("{0}:{1}", element.Name.NamespaceName, element.Name.LocalName);
                    }
                    else
                    {
                        localName = element.Name.LocalName;
                    }
                    
                    // If the element is in our dictionary, we check those text properties
                    if (elementTextProperties.TryGetValue(localName, out string[] textProps))
                    {
                        foreach (var prop in textProps)
                        {
                            // Look for an attribute like Content="Hello" or Text="Hello"
                            var textAttribute = element.Attribute(prop);
                            if (textAttribute != null)
                            {
                                string value = textAttribute.Value;
                                if (!string.IsNullOrWhiteSpace(value))
                                {
                                    //don't map x:Bind's
                                    if (!value.StartsWith("{x:Bind"))
                                    {
                                        // Combine x:Uid + "." + Property -> "MyButton.Content"
                                        string resourceKey = $"{uid}.{prop}";

                                        // Only add if not already present in dictionary (avoid duplicates)
                                        if (!entriesMap.ContainsKey(resourceKey))
                                        {
                                            entriesMap[resourceKey] = value;
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogScan($"Error processing {filePath}: {ex.Message}");
            }
        }

        private void SaveAsJson(string path, Dictionary<string, string> entriesMap)
        {
            var entries = entriesMap.Select(kvp => new LocalizedEntry
            {
                Key = kvp.Key,
                Value = kvp.Value
            }).ToList();
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            string jsonOutput = JsonSerializer.Serialize(entries, jsonOptions);
            //this file isn't used for anything other than it is useful open in a text editor to see what the scan found
            File.WriteAllText(path, jsonOutput);
        }

        public static (string Prefix, string Suffix) SplitPrefix(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return (string.Empty, input);
            }

            // Define the possible prefixes, ordered by length descending to match longer prefixes first
            string[] possiblePrefixes = { "@@", "@", "!!", "!" };

            foreach (var prefix in possiblePrefixes)
            {
                if (input.StartsWith(prefix))
                {
                    string suffix = input.Substring(prefix.Length);
                    return (prefix, suffix);
                }
            }

            return (string.Empty, input);
        }

        private void UpdateEnUSReswFile(Dictionary<string, string> entriesMap, string path)
        {
            LogScan(String.Format("Updating {0}", path));
            if (!File.Exists(path))
            {
                LogScan(String.Format("File does not exist: {0}", path));
                return;
            }
            XDocument doc = XDocument.Load(path);
            
            LogScan("Clearing old entries from en-US/Resources.resw...");

            doc.Descendants("data")
                .Where(x => 1 == 1)
                .Remove();

            var dataElements = doc.Descendants("data").ToList();

            LogScan("Adding entries to en-US/Resources.resw...");
            foreach (var item in entriesMap)
            {
                (string valuePrefix, string valueVal) = SplitPrefix(item.Value.Substring(0));
                var dataElement = new XElement("data",
                    new XAttribute("name", item.Key),
                    new XAttribute(XNamespace.Xml + "space", "preserve"),
                    new XElement("value", valueVal),
                    new XElement("comment", valuePrefix)
                );
                doc.Root.Add(dataElement);
                LogScan(String.Format("  Added {0}", item.Key));
            }

            LogScan("Checking for specials...");
            LoadSpecials(_targetTranslatorSpecialsPath);
            foreach (var item in SpecialItems.Where(item => item.Culture == "en-US"))
            {
                LogScan("Adding special: " + item.Key.ToString() + "=" + item.Value.ToString());

                var desDocDataElement1 = new XElement("data",
                    new XAttribute("name", item.Key.ToString()),
                    new XAttribute(XNamespace.Xml + "space", "preserve"),
                    new XElement("value", item.Value.ToString()),
                    new XElement("comment", item.Culture.ToString()));

                doc.Root.Add(desDocDataElement1);
            }

            doc.Save(path);
            LogScan("Saved.");
        }
        #endregion

        #region TRANSLATE
        JsonDatabase _cache;

        private async void btnTranslate_Click(object sender, RoutedEventArgs e)
        {
            btnTranslate.IsEnabled = false;
            cbTranslationFunction.IsEnabled = false;
            await Task.Delay(1000);

            if (CalcPaths())
            {
                LogTranslate("---------------------------------------");

                if (cbTranslationFunction.SelectedValue != null)
                {
                    LogTranslate("Translation function: " + cbTranslationFunction.SelectedValue.ToString());
                }
                else
                {
                    LogTranslate("Translation function not selected.");
                    return;
                }

                StorageFolder x = await StorageFolder.GetFolderFromPathAsync(_targetTranslatorPath);
                _cache = null;
                _cache = new(x);
                await _cache.InitializeAsync();
 
                TranslateReswFiles();

            }
            await Task.Delay(1000);
            cbTranslationFunction.IsEnabled = true;
            btnTranslate.IsEnabled = true;
        }

        private async void TranslateReswFiles()
        {
            LogTranslate("Translating...");
            int _cacheHitCounter = 0;
            int _cacheMissCounter = 0;
            int _failedTranslationCounter = 0;

            if (!File.Exists(_targetStrings_enUS_Path))
            {
                LogTranslate(String.Format("File does not exist: {0}", _targetStrings_enUS_Path));
                return;
            }
            XDocument enUSDoc = XDocument.Load(_targetStrings_enUS_Path);

            //_targetRootStringsPath
            var reswFiles = Directory.GetFiles(_targetStringsPath, "Resources.resw", SearchOption.AllDirectories);
            LogTranslate(String.Format("Found {0} Resources.resw files.", reswFiles.Count().ToString()));
            pbTranslate.Maximum = (reswFiles.Count() - 1) * enUSDoc.Descendants("data").Count();
            pbTranslate.Value = 0;

            LoadSpecials(_targetTranslatorSpecialsPath);

            foreach (var destDocFilePath in reswFiles)
            {
                string culture_Name = Path.GetFileName(Path.GetDirectoryName(destDocFilePath)); //en-US, ar-SA etc
                if (culture_Name != "en-US")
                {
                    XDocument destDoc = XDocument.Load(destDocFilePath);
                    LogTranslate(String.Format("Processing {0}...", culture_Name));

                    //reset destDoc
                    destDoc.Descendants("data")
                        .Where(x => 1 == 1)
                        .Remove();

                    var enUSDocDataElements = enUSDoc.Descendants("data").ToList();
                    foreach (var item in enUSDocDataElements)
                    {
                        //get translation from cache or call api
                        string translatedText = string.Empty;
                        string cacheIndicator = string.Empty;
                        string originalText = item.Element("value").Value;
                        string translationHint = item.Element("comment").Value;

                        //reject names with periods in it 'close.btn' or 'loading....';

                        if ((translationHint != "!") && (translationHint != "@") && (translationHint != "@@") && (translationHint != "!!"))
                        {
                            //if there's a x:Uid, we expect hint tokens
                            //translationHint is not !, !!, @ or @@ prefix, so we won't translate or cache it
                            //LogTranslate(String.Format("  !Missing hint token: {0}", item.Attribute("name").Value));
                        }
                        else
                        {
                            string cacheKey = String.Format("{0}:{1}:{2}", culture_Name, translationHint, originalText);
                            string cachedData= _cache.GetValue(cacheKey);
                            if (cachedData != null)
                            {
                                translatedText = cachedData;
                                LogTranslate(String.Format("  Cache hit: {0}:{1}:{2}", culture_Name, translationHint, originalText));
                                _cacheHitCounter++;
                            }
                            else
                            {
                                _cacheMissCounter++;
                                translatedText = Translate(originalText, culture_Name, translationHint);
                                LogTranslate(String.Format("  Cache miss: {0}:{1}:{2} --> Translated", culture_Name, translationHint, originalText));
                                if (translatedText == null)
                                {
                                    //null returned, so skip it - could be a bad translation or critical failed attempt?
                                    //it is important to know of these, as it could be a missing translation like on an obscure control on a seldom-used page.
                                    _failedTranslationCounter++;
                                    //have the translation function add diagnostics to log
                                    continue;
                                }
                                await _cache.AddEntryAsync(cacheKey, translatedText);
                            }
                            var desDocDataElement = new XElement("data",
                                new XAttribute("name", item.Attribute("name").Value),
                                new XAttribute(XNamespace.Xml + "space", "preserve"),
                                new XElement("value", translatedText),
                                new XElement("comment", item.Element("comment").Value));

                            destDoc.Root.Add(desDocDataElement);
                        }

                        pbTranslate.Value = pbTranslate.Value + 1;
                        await Task.Delay(20);
                    }

                    //add the specials
                    LogTranslate("  Checking for specials...");
                    foreach (var item in SpecialItems.Where(item => item.Culture == culture_Name))
                    {
                        LogTranslate("  Adding special: " + item.Key.ToString() + "=" + item.Value.ToString());

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
            LogTranslate("****************");
            LogTranslate(String.Format("Summary: {0} cache hits", _cacheHitCounter));
            LogTranslate(String.Format("Summary: {0} cache misses", _cacheMissCounter));
            LogTranslate(String.Format("Summary: {0} failed translations", _failedTranslationCounter));
            if (_failedTranslationCounter > 0)
            {
                LogTranslate("Some translations have failed: Examine log closely to troubleshoot.");
            }
            LogTranslate("****************");
            LogTranslate("Translation complete.");
        }
        #endregion

        #region TRANSLATION FUNCTIONS
        private string Translate(string text, string cultureName, string hint)
        {
            //string mode = "OpenAI_API";
            if (cbTranslationFunction.SelectedValue != null)
            {
                string mode = cbTranslationFunction.SelectedValue.ToString();
                if (mode == "OpenAI_API")
                {
                    return TranslateFunction_OpenAI(text, cultureName, hint);
                }
                else if (mode == "MyTranslationFunction")
                {
                    //call your translation function here to your custom translation calling code
                    return MyTranslationFunction(text, cultureName, hint);
                }
                else
                {
                    LogTranslate("Translation Function is unknown: " + mode);
                    return null;
                }
            }
            else
            {
                LogTranslate("Translation Function not selected.");
                return null;
            }
        }

        //A SAMPLE TRANSLATION FUNCTION
        private string MyTranslationFunction(string text, string cultureName, string hint)
        {
            //put your code here to translate the text from en-US to culturename.  Use the hint to tweak your prompts.
            //let's just return a null, which means that the translationfailed.
            LogTranslate("    MyTranslationFunction returned null");
            return null;
        }

        //OPENAI API TRANSLATION FUNCTION
        private string TranslateFunction_OpenAI(string text, string cultureName, string hint)
        {
            //this Translation Function is the original one used on this translator app.  It is not optimized.
            string openAiApiKey = Environment.GetEnvironmentVariable("OpenAIAPIKey1");

            const string openAiChatEndpoint = "https://api.openai.com/v1/chat/completions";

            using HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);

            string systemContent = string.Empty;
            string userContent = string.Empty;

            systemContent = String.Format(Hints[hint], cultureName);
            userContent = text;

            var requestBody = new
            {
                model = "gpt-4",
                messages = new[]
                {
                    new { role = "developer", content = systemContent},
                    new { role = "user", content = userContent}
                },
                temperature = 0.0,
                max_tokens = 1000
            };

            var requestContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            using HttpResponseMessage response = httpClient.PostAsync(openAiChatEndpoint, requestContent).Result;
            response.EnsureSuccessStatusCode();

            string jsonResponse = response.Content.ReadAsStringAsync().Result;

            // The response JSON includes an array of "choices"; we want the "message.content"
            // of the first choice. For structure details, see:
            // https://platform.openai.com/docs/guides/chat/introduction
            try
            {
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                JsonElement root = doc.RootElement;
                JsonElement choices = root.GetProperty("choices");
                JsonElement firstChoice = choices[0];
                JsonElement message = firstChoice.GetProperty("message");
                string translatedText = message.GetProperty("content").GetString() ?? string.Empty;

                return translatedText.Trim();
            }
            catch
            {
                // Fallback in case of any unexpected structure
                LogTranslate("    **Bad API return structure.");
                return null;
            }
        }
        #endregion

    }
}
