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
    public class TCacheItem(string key, string value, string resourceName, string profileResult) : ObservableObject
    {

        [JsonPropertyName("key")]
        public string key { get; set; } = key;

        [JsonPropertyName("value")]
        public string value { get; set; } = value;

        //resourceName is the name of the Resources.resw.name property for TLocalized.Get()'s but will be missing the xaml control type suffix ex '_Text' for a Button element.
        [JsonPropertyName("resourceName")]
        public string resourceName { get; set; } = resourceName;
        
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

        //resourceName is the name of the Resources.resw.name property for TLocalized.Get()'s but will be missing the xaml control type suffix ex '_Text' for a Button element.
        [ObservableProperty]
        [property: JsonPropertyName("resourceName")]
        private string _resourceName;

        [ObservableProperty]
        [property: JsonPropertyName("profileResult")]
        private string _profileResult;

        [JsonIgnore]
        [ObservableProperty]
        private string _displayStr;

        [JsonConstructor]
        public TCacheItemEx(string key, string value, string resourceName, string profileResult)
        {
            this.Key = key;
            this.Value = value;
            this.ResourceName = (resourceName == null) ? "" : resourceName; //handles earlier json that didn't have a resourceName field.
            this.ProfileResult = profileResult;
            this.DisplayStr = this.ResourceName + Environment.NewLine +  this.Key;
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

        public async Task AddOrUpdate(string key, string value, string resourceName, string profileResult)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                TCacheItem newEntry = new TCacheItem(key, value, resourceName, profileResult);
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
