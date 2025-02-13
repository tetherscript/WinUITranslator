using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System;
using Microsoft.Windows.ApplicationModel.Resources;

namespace TeeLocalized
{
    public static partial class TLocalized
    {
        static TLocalized()
        {

            try
            {
                ResourceManager = new ResourceManager();
            }
            catch
            {

            }
        }

        #region README
        //You can add this file to your project, or add a link to it at \common\TLocalized.cs in the Translator project.
        //Linking is recommended, especially if you have multiple projects and are grabbing the latest Translator version.
        //This class won't check the version of Translator, so it's important to keep them in sync.

        //TO CAPTURE DYNAMIC TRANSLATION REQUESTS:
        //1) On App Startup, call .LoadKeyVals(), then .TrackKeyVals=true.
        //2) Run the app in debug mode.
        //3) Exercise the newly added functionality that calls the .Get() function. New .Get() call data
        //   are appended to \Translator\TLocalizedGets.json, so you just need to test the new .Get()'s;  
        //4) On App Shutdown, call .SaveKeyVals().  \Translator\TLocalizedGets.json now contains info about those .Get() calls.
        //5) To see the translations in your app, run the Translator app and scan, then translate.  The \Strings\???\Resources.res for each language will be updated.
        //6) Run your app again, in debug, or non-debug mode.  You should see the translations.
        //Note: you can manually edit, or clear \Translator\TLocalizedGets.json.
        //Note: Your .Get() calls needs to include the en-US American English string to be translated.  

        //TO STOP CAPTURING DYNAMIC TRANSLATION REQUESTS
        //1) On App Startup, set TrackKeyVals=false.

        //TO GET A RESOURCE STRING
        //Use the .Get() for a translated string, or .GetSpecial() for a non-translated string, like an icon filename, color etc.

        //HOW TO TEST TRANSLATIONS WHILE DEVELOPING
        //1) It depends on whether you have a packaged or unpackaged app.
        //2) Packaged - See the packaged sample app to see how you can override the language and set the flowdirection
        //     without needing to log in/out and change the system language.
        //3) Unpackaged - See the unpackaged sample app.  You will need to log in/out while changing system languages
        //     to see the translations. 
        //THE EASIEST WAY - is to login to ex. en-DE German and run your app in Visual Studio, and develop and
        //  run Translator there.  
        //BE SURE TO TEST A RTL LANGUAGE TOO, such as ar-SA.  Or override the FlowDirection to RTL in app startup.

        //DEPLOYING YOUR APP
        //This class is Trimming-friendly.
        //Don't deploy or distribute the \Translator folder and it's contents.  That's only used by the Translator app.
        //Dont' call .LoadKeyVals() or .SaveKeyVals() since the file is no longer there.  
        //Set .TrackKeyVals=false.
        //The translator app will have updated your \Strings\???\Resource.resw's so you don't need to touch them.
        //The only Translator-related code that will run in your app are the TLocalized .Get() and .GetSpecial() calls.
        //Build your app, and deploy.
        #endregion

        #region DECLARATIONS
        public class KeyValEntry
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        public static bool TrackKeyVals = false;

        public static bool ThrowExceptionsOnErrors = false;

        private static Dictionary<string, string> KeyVals = new Dictionary<string, string>();

        private static readonly ResourceManager ResourceManager;
        #endregion

        #region READING RESOURCES
        public static string Get(string name, string hint, string value)
        {
            //On .Get() errors, .ThrowExceptionsOnErrors is used to  throw exceptions on errors or display an error code in the returned string.
            //If you have a way to log exceptions, this can be useful. If not, it's a pain in the IDE to break
            //on this exception every time.  Maybe a visual indicator ex: '>>1' is enough.

            //use .Get() to specify a Resources.resw name and return a Value which holds translated text.
            //you'll also need to specify a hint '@', '@@', '!', '!!' for the the translator app.
            name = name.Trim();
            hint = hint.Trim();
            value = value.Trim();
            string result = string.Empty;

            if ((name == null) || (name == "") || (!IsValidXamlIdentifier(name)))
            {
                if (ThrowExceptionsOnErrors)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "TLocalizer.Get() invalid name: " + name);
                }
                else
                {
                    //will return >>1 as visual indication that somethign is wrong with the name, which is invalid
                    //we won't translate this or save it to TLocalizedgets.json.
                    result = ">>1";
                }
            }
            else
            //if ((hint == null) || (hint == "") || ((hint != "!") && (hint != "@") && (hint != "@@") && (hint != "!!")))
            if (!IsValidHintToken(hint))
            {
                if (ThrowExceptionsOnErrors)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "TLocalizer.Get() invalid hint: " + hint);
                }
                else
                {
                    //will return >>2 as visual indication that somethign is wrong with the hint, which is invalid
                    //we won't translate this or save it to TLocalizedgets.json.
                    result = ">>2";
                }
            }
            else
            {
                if (TrackKeyVals)
                {
                    if (hint != "*")
                    {
                        KeyVals[name] = hint + value;
                    }
                }

                try
                {
                    string translatedText = ResourceManager?.MainResourceMap.TryGetValue(name)?.ValueAsString;
                    if (string.IsNullOrEmpty(translatedText))
                    {
                        var r = ResourceManager?.MainResourceMap.TryGetSubtree("Resources")?.TryGetValue(name);
                        if (r != null)
                        {
                            translatedText = r.ValueAsString;
                        }
                    }
                    if (translatedText == null)
                    {
                        //did not find translated Value, so show the hint + original text to indicate it hasn't been translated yet
                        translatedText = hint + value;
                    }
                    result = translatedText;
                }
                catch (Exception ex)
                {
                    if (ThrowExceptionsOnErrors)
                    {
                        throw new ArgumentOutOfRangeException(nameof(value), "TLocalizer.Get() cannot get resource Value: " + ex.Message);
                    }
                    else
                    {
                        //will return >>3 as visual indication that somethign is wrong with the hint, which is invalid
                        //we won't translate this or save it to TLocalizedgets.json.
                        result = ">>3";
                    }
                }

            }
            return result;
        }

        public static string GetSpecial(string name)
        {
            //gets a non-translated Value, like a localized icon filename, glyph or color
            //Just add these 'Specials' in the \Translator\Specials.json so they are included in the Resources.resw files'
            name = name.Trim();
            string result = string.Empty;

            if ((name == null) || (name == "") || (!IsValidXamlIdentifier(name)))
            {
                if (ThrowExceptionsOnErrors)
                {
                    throw new ArgumentOutOfRangeException(nameof(name), "TLocalizer.GetSpecial() invalid name: " + name);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                try
                {
                    string value = ResourceManager?.MainResourceMap.TryGetValue(name)?.ValueAsString;
                    if (string.IsNullOrEmpty(value))
                    {
                        var r = ResourceManager?.MainResourceMap.TryGetSubtree("Resources")?.TryGetValue(name);
                        if (r != null)
                        {
                            value = r.ValueAsString;
                        }
                    }
                    return value;
                }
                catch (Exception ex)
                {
                    if (ThrowExceptionsOnErrors)
                    {
                        throw new ArgumentOutOfRangeException(nameof(name), "TLocalizer.GetSpecial() cannot get resource Value: " + ex.Message);
                    }
                    else
                    {
                        return null;
                    }
                }

            }
        }
        #endregion

        #region KEYVALS FOR TRANSLATING
        public static string GetKeyVals()
        {
            //returns a string of the current tracked KeyVals
            if (!TrackKeyVals) { return ""; }
            var entries = KeyVals.Select(kvp => new KeyValEntry
            {
                Key = kvp.Key,
                Value = kvp.Value
            }).ToList();
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            string jsonOutput = JsonSerializer.Serialize(entries, jsonOptions);
            return jsonOutput;
        }

        private static string _localizedDataPath;

        public static void LoadKeyVals(string path)
        {
            //loads /translator/TLocalizedGets.json into Keyvals
            _localizedDataPath = path;
            if (!TrackKeyVals) { return; }
            string loadedJson = File.ReadAllText(_localizedDataPath);
            // Deserialize to a list of LocalizedEntry
            var newEntries = JsonSerializer.Deserialize<List<KeyValEntry>>(
                loadedJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            foreach (var entry in newEntries)
            {
                KeyVals[entry.Key] = entry.Value;
            }
        }

        public static void SaveKeyVals()
        {
            //saves KeyVals into /translator/TLocalizedGets.json
            if (!TrackKeyVals) { return; }
            var entries = KeyVals.Select(kvp => new KeyValEntry
            {
                Key = kvp.Key,
                Value = kvp.Value
            }).ToList();
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            string jsonOutput = JsonSerializer.Serialize(entries, jsonOptions);
            File.WriteAllText(_localizedDataPath, jsonOutput);
        }

        #region ARCHIVE
        //---ALTERNATE LOAD/SAVE FUNCTIONS IN CASE TRIMMING IS AN ISSUE
        //public static void SaveKeyVals()
        //{
        //    //saves KeyVals into /translator/TLocalizedGets.json
        //    if (!TrackKeyVals) { return; }
        //    using FileStream fs = File.Open(_localizedDataPath, FileMode.Create, FileAccess.Write);
        //    using Utf8JsonWriter writer = new(fs, new JsonWriterOptions { Indented = true });
        //    writer.WriteStartArray();

        //    var entries = KeyVals.Select(kvp => new KeyValEntry
        //    {
        //        Key = kvp.Key,
        //        Value = kvp.Value
        //    }).ToList();

        //    foreach (var entry in entries)
        //    {
        //        writer.WriteStartObject();
        //        writer.WriteString("Key", entry.Key);
        //        writer.WriteString("Value", entry.Value);
        //        writer.WriteEndObject();
        //    }

        //    writer.WriteEndArray();
        //    writer.Flush();
        //}

        //public static void LoadKeyVals(string path)
        //{
        //    if (!TrackKeyVals) { return; }
        //    _localizedDataPath = path;
        //    byte[] jsonBytes = File.ReadAllBytes(_localizedDataPath);
        //    var entries = new List<KeyValEntry>();

        //    var jsonReader = new Utf8JsonReader(jsonBytes, new JsonReaderOptions
        //    {
        //        // Adjust if you want to allow trailing commas or comments
        //        // for example: CommentHandling = JsonCommentHandling.Skip
        //    });

        //    // We expect the JSON to be an array of objects
        //    if (!jsonReader.Read() || jsonReader.TokenType != JsonTokenType.StartArray)
        //    {
        //        throw new FormatException("JSON must start with an array.");
        //    }

        //    // Read each object in the array
        //    while (true)
        //    {
        //        if (!jsonReader.Read())
        //        {
        //            throw new FormatException("Unexpected end of JSON while reading array elements.");
        //        }

        //        // If we've reached the end of the array, we're done
        //        if (jsonReader.TokenType == JsonTokenType.EndArray)
        //        {
        //            break;
        //        }

        //        if (jsonReader.TokenType != JsonTokenType.StartObject)
        //        {
        //            throw new FormatException("Expected a JSON object.");
        //        }

        //        // Read the object's properties: "Key" and "Value"
        //        string keyValue = null;
        //        string valueValue = null;

        //        while (true)
        //        {
        //            if (!jsonReader.Read())
        //            {
        //                throw new FormatException("Unexpected end of JSON while reading an object.");
        //            }

        //            if (jsonReader.TokenType == JsonTokenType.EndObject)
        //            {
        //                // End of this object
        //                break;
        //            }

        //            if (jsonReader.TokenType == JsonTokenType.PropertyName)
        //            {
        //                string propertyName = jsonReader.GetString();

        //                // Move to the Value
        //                if (!jsonReader.Read())
        //                {
        //                    throw new FormatException("Unexpected end of JSON while reading a property Value.");
        //                }

        //                // Match property names
        //                if (propertyName == "Key")
        //                {
        //                    if (jsonReader.TokenType != JsonTokenType.String)
        //                    {
        //                        throw new FormatException("Expected string for 'Key'.");
        //                    }
        //                    keyValue = jsonReader.GetString();
        //                }
        //                else if (propertyName == "Value")
        //                {
        //                    if (jsonReader.TokenType != JsonTokenType.String)
        //                    {
        //                        throw new FormatException("Expected string for 'Value'.");
        //                    }
        //                    valueValue = jsonReader.GetString();
        //                }
        //                else
        //                {
        //                    // If there is an unknown property, skip it
        //                    jsonReader.Skip();
        //                }
        //            }
        //            else
        //            {
        //                // If we encounter anything else, we can skip or throw depending on needs
        //                jsonReader.Skip();
        //            }
        //        }

        //        entries.Add(new KeyValEntry
        //        {
        //            Key = keyValue,
        //            Value = valueValue
        //        });
        //    }

        //    foreach (var entry in entries)
        //    {
        //        KeyVals[entry.Key] = entry.Value;
        //    }

        //}
        #endregion
        #endregion

        #region VALID XML IDENTIFIERS
        // Checks if a string is a valid XAML identifier. 
        // It must:
        // 1. Not be null or empty.
        // 2. Start with a letter or underscore.
        // 3. Contain only letters, digits, or underscores in subsequent characters.
        public static bool IsValidXamlIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return false;
            }

            // Check the first character (must be letter or underscore).
            if (!IsValidFirstChar(identifier[0]))
            {
                return false;
            }

            // Check the rest of the characters (must be letter, digit, or underscore).
            for (int i = 1; i < identifier.Length; i++)
            {
                if (!IsValidSubsequentChar(identifier[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsValidFirstChar(char c)
        {
            // XAML/C# rules: The first character must be a letter or underscore.
            return char.IsLetter(c) || c == '_';
        }

        private static bool IsValidSubsequentChar(char c)
        {
            // Subsequent characters can be letters, digits, or underscores.
            return char.IsLetterOrDigit(c) || c == '_';
        }
        #endregion

        #region HINT TOKENS
        //order by length descending
        public static string[] ValidHintTokens = { "@@", "@", "!!", "!", "##", "#" };

        public static string ValidHintTokenStr = string.Join(", ", ValidHintTokens);

        //get the hint token prefix, if any, from a string
        public static (string Prefix, string Suffix) SplitPrefix(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return (string.Empty, input);
            }

            // Requires list of possible tokens to be ordered by length descending to match longer prefixes first
            foreach (var prefix in ValidHintTokens)
            {
                if (input.StartsWith(prefix))
                {
                    string suffix = input.Substring(prefix.Length);
                    return (prefix, suffix);
                }
            }

            return (string.Empty, input);
        }

        //get the hint token prefix, if any, from a string
        public static bool IsValidHintToken(string input)
        {
            return (ValidHintTokens.Contains(input));
        }
        #endregion
    }
}
