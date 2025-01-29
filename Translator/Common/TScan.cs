using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using TeeLocalized;

namespace Translator
{
    public static class TScan
    {
        public static int ProgressPerc = 0;
        public static bool IsCancelled = false;
        private static int _delay = 500;

        private class PageInfo
        {
            public string Link { get; set; }
            public string PhysicalPath { get; set; }
        }

        public class LocalizedEntry
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        static Dictionary<string, string[]> elementTextProperties = new Dictionary<string, string[]>
        {
            //{ "Button",       new[] { "Content" } },
            //{ "TextBlock",    new[] { "Text" } },
            //{ "CheckBox",     new[] { "Content" } },
            //{ "ToggleSwitch",     new[] { "Header", "OffContent", "OnContent" } },
            //{ "RadioButtons",  new[] { "Header" } },
            //{ "RadioButton",  new[] { "Content" } },
            //{ "ToggleButton", new[] { "Content" } }
        };

        public static async Task Start(string targetRootPath)
        {
            IsCancelled = false;
            TLog.ClearAllFiles();
            TLog.Reset();
            try
            {
                TLog.Log(TLog.eLogType.inf, 0, "Scan started.");

                await ScanAsync(
                    targetRootPath);

                TLog.LogSeparator(TLog.eLogSeparatorType.lineWide);
                TLog.Log(TLog.eLogType.inf, 0, "Scan complete.");
            }
            catch (Exception ex)
            {
                TLog.Log(TLog.eLogType.err, 0, $"An error occurred: {ex.Message}");
            }
            TLog.Save(TUtils.TargetScanLogPath);
        }

        public static void Stop()
        {
            IsCancelled = true;
        }

        public static async Task ScanAsync(string targetRootPath)
        {
            TUtils.CalcPaths(targetRootPath);
            ProgressPerc = 0;

            try
            {
                if (!File.Exists(TUtils.TargetTranslatorXamlElementsPath))
                {
                    TLog.Log(TLog.eLogType.inf, 0, String.Format("Cannot find {0}", TUtils.TargetTranslatorXamlElementsPath));
                    return;
                }

                string jsonString = await File.ReadAllTextAsync(TUtils.TargetTranslatorXamlElementsPath);
                elementTextProperties = JsonSerializer.Deserialize<Dictionary<string, string[]>>(jsonString);
            }
            catch (Exception ex)
            {
                TLog.Log(TLog.eLogType.err, 0, $"Error XamlElements.json from JSON: {ex.Message}");
            }

            TLog.Log(TLog.eLogType.inf, 0, "Scanning target project: " + targetRootPath);

            var entriesMap = new Dictionary<string, string>();

            var xamlFiles = Directory.GetFiles(targetRootPath, "*.xaml", SearchOption.AllDirectories);

            foreach (var filePath in xamlFiles)
            {
                if (!filePath.Contains(@"\obj\"))
                {
                    TLog.Log(TLog.eLogType.inf, 2, "Found: " + filePath);
                    ProcessXamlFile(filePath, entriesMap);
                }
            }

            await Task.Delay(_delay);

            //----------------------------------------------------
            //Add linked project files, like a .xaml that exists in a /common folder not under the project folder
            string projName = Path.GetFileName(targetRootPath);
            string csprojPath = Path.Combine(targetRootPath, String.Format("{0}.csproj", projName));

            if (string.IsNullOrWhiteSpace(csprojPath))
                throw new ArgumentException("csprojPath cannot be null or empty.", nameof(csprojPath));

            if (!File.Exists(csprojPath))
                throw new FileNotFoundException("The specified .csproj file does not exist.", csprojPath);

            // Load the .csproj XML
            TLog.Log(TLog.eLogType.inf, 0, "Checking .csproj for linked .xaml's...");
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

                TLog.Log(TLog.eLogType.inf, 0, "Found linked .xaml: " + physicalPath);
                ProcessXamlFile(physicalPath, entriesMap);
            }

            await Task.Delay(_delay);

            //merge with manual entries
            TLog.Log(TLog.eLogType.inf, 2, "Merging " + TUtils.TargetTranslatorTLocalizedGetsPath + "...");
            if (!File.Exists(TUtils.TargetTranslatorTLocalizedGetsPath))
            {
                TLog.Log(TLog.eLogType.err, 2, $"Merge file not found: {TUtils.TargetTranslatorTLocalizedGetsPath}");
                return;
            }

            try
            {
                string loadedJson = File.ReadAllText(TUtils.TargetTranslatorTLocalizedGetsPath);

                // Deserialize to a list of LocalizedEntry
                var newEntries = JsonSerializer.Deserialize<List<LocalizedEntry>>(
                    loadedJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (newEntries == null)
                {
                    TLog.Log(TLog.eLogType.inf, 2, "Merging: " + "No entries found in " + TUtils.TargetTranslatorTLocalizedGetsPath + ".");
                    return;
                }
                else
                {
                    TLog.Log(TLog.eLogType.inf, 2, $"Found {newEntries.Count} mergeable entries.");
                }

                // Merge them into entriesMap
                foreach (var entry in newEntries)
                {
                    if (!entriesMap.ContainsKey(entry.Key))
                    {
                        // Add the new entry
                        entriesMap[entry.Key] = entry.Value;
                        TLog.Log(TLog.eLogType.inf, 4, $"Merged {entry.Key}: {entry.Value}");
                    }
                }
                TLog.Log(TLog.eLogType.inf, 2, $"Merge complete.");
            }
            catch (Exception ex)
            {
                TLog.Log(TLog.eLogType.err, 2, $"Scanning: Error merging JSON file: {ex.Message}");
            }

            SaveAsJson(TUtils.TargetTranslatorDetectedXamlElementsPath, entriesMap);

            await Task.Delay(_delay);

            UpdateEnUSReswFile(entriesMap, TUtils.TargetStrings_enUS_Path);

            TLog.Log(TLog.eLogType.inf, 0, "Summary: Updated " + TUtils.TargetStrings_enUS_Path);
            TLog.Log(TLog.eLogType.inf, 0, "Summary: Updated " + TUtils.TargetTranslatorDetectedXamlElementsPath);

        }

        private static void ProcessXamlFile(string filePath, Dictionary<string, string> entriesMap)
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
                        TLog.Log(TLog.eLogType.err, 4, String.Format("Rejected {0}:x:Uid='{1}' as it is an invalid XAML/C# resource identifier.", filePath, uid));
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
                TLog.Log(TLog.eLogType.err, 4, $"Error processing {filePath}: {ex.Message}");
            }
        }

        private static void SaveAsJson(string path, Dictionary<string, string> entriesMap)
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

        private static void UpdateEnUSReswFile(Dictionary<string, string> entriesMap, string path)
        {
            int _counter = 0;

            TLog.Log(TLog.eLogType.inf, 0, String.Format("Updating {0}", path));
            if (!File.Exists(path))
            {
                TLog.Log(TLog.eLogType.err, 0, String.Format("File does not exist: {0}", path));
                return;
            }
            XDocument doc = XDocument.Load(path);

            TLog.Log(TLog.eLogType.inf, 2, "Clearing old entries from en-US/Resources.resw...");

            doc.Descendants("data")
                .Where(x => 1 == 1)
                .Remove();

            var dataElements = doc.Descendants("data").ToList();

            TLog.Log(TLog.eLogType.inf, 4, "Adding entries to en-US/Resources.resw...");
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
                _counter++;
                TLog.Log(TLog.eLogType.inf, 6, String.Format("Added {0}", item.Key));
            }

            TLog.Log(TLog.eLogType.inf, 0, "Checking for specials...");
            TUtils.LoadSpecials(TUtils.TargetTranslatorSpecialsPath);
            foreach (var item in TUtils.SpecialItems.Where(item => item.Culture == "en-US"))
            {
                TLog.Log(TLog.eLogType.inf, 2, "Adding special: " + item.Key.ToString() + "=" + item.Value.ToString());

                var desDocDataElement1 = new XElement("data",
                    new XAttribute("name", item.Key.ToString()),
                    new XAttribute(XNamespace.Xml + "space", "preserve"),
                    new XElement("value", item.Value.ToString()),
                    new XElement("comment", item.Culture.ToString()));

                doc.Root.Add(desDocDataElement1);
                _counter++;
            }

            doc.Save(path);
            TLog.Log(TLog.eLogType.inf, 0, "en-US/Resources.resw updated.");
            TLog.LogSeparator(TLog.eLogSeparatorType.lineWide);
            TLog.Log(TLog.eLogType.inf, 0, String.Format("Summary: {0} errors", TLog.ErrorCounter));
            TLog.Log(TLog.eLogType.inf, 0, String.Format("Summary: {0} translateable items found", _counter));
        }

    }

}