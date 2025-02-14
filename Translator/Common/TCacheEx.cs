using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Translator
{
    public class TCacheItem(string key, string value, string profileResult) : ObservableObject
    {

        [JsonPropertyName("key")]
        public string key { get; set; } = key;

        [JsonPropertyName("value")]
        public string value { get; set; } = value;

        [JsonPropertyName("profileResult")]
        public string profileResult { get; set; } = profileResult;

    }

    public partial class TCacheItemEx : ObservableObject
    {
        [ObservableProperty]
        [property: JsonPropertyName("key")]
        private string _key;

        [ObservableProperty]
        [property: JsonPropertyName("value")]
        private string _value;

        [ObservableProperty]
        [property: JsonPropertyName("profileResult")]
        private string _profileResult;

        [JsonConstructor]
        public TCacheItemEx(string key, string value, string profileResult)
        {
            this.Key = key;
            this.Value = value;
            this.ProfileResult = profileResult;
        }
    }

    public class TCacheEx
    {
        private List<TCacheItem> _items = [];
        public List<TCacheItem> Items { get { return _items; } }
        
        private string _path;

        public async Task Init(string path)
        {
            _path = path;
            if (File.Exists(path))
            {
                string json = await File.ReadAllTextAsync(_path, Encoding.UTF8);
                if (json != "")
                {
                    _items = JsonSerializer.Deserialize<List<TCacheItem>>(json);
                }
            }
        }

        public async Task AddOrUpdate(string key, string value, string profileResult)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                TCacheItem newEntry = new TCacheItem(key, value, profileResult);
                if (!_items.Any(e => e.key == newEntry.key))
                {
                    _items.Add(newEntry);
                }
                else
                {
                    TCacheItem existingEntry = _items.First(e => e.key == newEntry.key);
                    existingEntry.value = newEntry.value;
                    existingEntry.profileResult = newEntry.profileResult;
                }
                await SaveAsync();
            }
        }

        public string GetValue(string key)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                var item = _items.FirstOrDefault(x => x.key == key);
                if (item != null)
                {
                    return (_items.FirstOrDefault(x => x.key == key)).value;
                }
            }
            return null;
        }

        private async Task SaveAsync()
        {
            try
            {
                string json = JsonSerializer.Serialize(_items, new JsonSerializerOptions { WriteIndented = true } );
                await File.WriteAllTextAsync(_path, json, Encoding.UTF8);
            }
            catch (Exception)
            {

            }

        }
    }
}
