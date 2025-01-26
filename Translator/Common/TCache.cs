using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;
using Windows.Storage;

namespace Translator
{
    public static class TCache
    {

        public class Entry
        {
            [JsonPropertyName("key")]
            public string Key { get; set; }

            [JsonPropertyName("value")]
            public string Value { get; set; }
        }

        private const string FileName = "Cache.json";
        private static Dictionary<string, string> _entries;
        private static StorageFolder _folder;
        private static StorageFile _file;

        public static void Init(StorageFolder folder = null)
        {
            _folder = folder ?? ApplicationData.Current.LocalFolder;
            _entries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); 
        }

        public static async Task InitializeAsync()
        {
            try
            {
                _file = await _folder.CreateFileAsync(FileName, CreationCollisionOption.OpenIfExists);
                string json = await FileIO.ReadTextAsync(_file, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                if (json != "")
                {

                    var entryList = JsonSerializer.Deserialize<List<Entry>>(json) ?? new List<Entry>();

                    // Populate the dictionary, ensuring no duplicates
                    foreach (var entry in entryList)
                    {
                        if (!_entries.ContainsKey(entry.Key))
                        {
                            _entries.Add(entry.Key, entry.Value);
                        }
                        else
                        {
                            // Handle duplicates as needed
                            // For example, you can log a warning, skip, or overwrite
                            // Here, we'll skip duplicate entries
                            System.Diagnostics.Debug.WriteLine($"Duplicate key '{entry.Key}' found in JSON. Skipping duplicate.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log them)
                System.Diagnostics.Debug.WriteLine($"Error initializing database: {ex.Message}");
                _entries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }

         public static async Task AddEntryAsync(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
                //throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));
            }

            if (_entries.ContainsKey(key))
            {
                return;
                //throw new ArgumentException($"An entry with key '{key}' already exists.");
            }

            _entries.Add(key, value);
            await SaveAsync();
        }

        public static string GetValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            _entries.TryGetValue(key, out string value);
            return value;
        }

        public static async Task UpdateEntryAsync(string key, string newValue)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));
            }

            if (!_entries.ContainsKey(key))
            {
                throw new KeyNotFoundException($"No entry found with key '{key}'.");
            }

            _entries[key] = newValue;
            await SaveAsync();
        }

        public static async Task<bool> RemoveEntryAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            bool removed = _entries.Remove(key);
            if (removed)
            {
                await SaveAsync();
            }

            return removed;
        }

        public static List<Entry> GetAllEntries()
        {
            return _entries.Select(kvp => new Entry { Key = kvp.Key, Value = kvp.Value }).ToList();
        }

        private static async Task SaveAsync()
        {
            try
            {
                var entryList = _entries.Select(kvp => new Entry { Key = kvp.Key, Value = kvp.Value }).ToList();
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(entryList, options);
                await FileIO.WriteTextAsync(_file, json, Windows.Storage.Streams.UnicodeEncoding.Utf8);
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log them)
                System.Diagnostics.Debug.WriteLine($"Error saving database: {ex.Message}");
            }

        }
    }
}
