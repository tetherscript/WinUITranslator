using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;
using Windows.Storage;

namespace Translator
{
    public class Entry
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class JsonDatabase
    {
        private const string FileName = "cache.json";
        private Dictionary<string, string> _entries; // Using Dictionary for efficient lookups and uniqueness
        private readonly StorageFolder _folder;
        private StorageFile _file;

        // Constructor
        public JsonDatabase(StorageFolder folder = null)
        {
            _folder = folder ?? ApplicationData.Current.LocalFolder;
            _entries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); // Case-insensitive keys
        }

        /// <summary>
        /// Initializes the database by loading existing entries or creating a new file.
        /// Ensures no duplicate keys are present.
        /// </summary>
        public async Task InitializeAsync()
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

        /// <summary>
        /// Adds a new entry to the database.
        /// Throws an exception if the key already exists.
        /// </summary>
        /// <param name="key">The key for the entry.</param>
        /// <param name="value">The value for the entry.</param>
        public async Task AddEntryAsync(string key, string value)
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

        /// <summary>
        /// Retrieves a value based on the provided key.
        /// Returns null if the key does not exist.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>The corresponding value if found; otherwise, null.</returns>
        public string GetValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            _entries.TryGetValue(key, out string value);
            return value;
        }

        /// <summary>
        /// Updates an existing entry's value.
        /// Throws an exception if the key does not exist.
        /// </summary>
        /// <param name="key">The key of the entry to update.</param>
        /// <param name="newValue">The new value for the entry.</param>
        public async Task UpdateEntryAsync(string key, string newValue)
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

        /// <summary>
        /// Removes an entry from the database.
        /// Returns true if the entry was removed; otherwise, false.
        /// </summary>
        /// <param name="key">The key of the entry to remove.</param>
        public async Task<bool> RemoveEntryAsync(string key)
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

        /// <summary>
        /// Retrieves all entries as a list.
        /// </summary>
        public List<Entry> GetAllEntries()
        {
            return _entries.Select(kvp => new Entry { Key = kvp.Key, Value = kvp.Value }).ToList();
        }

        /// <summary>
        /// Saves the current state of the database to the JSON file.
        /// </summary>
        private async Task SaveAsync()
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



