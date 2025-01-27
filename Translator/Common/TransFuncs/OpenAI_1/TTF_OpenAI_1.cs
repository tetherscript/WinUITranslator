using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Translator
{
    public static class TTF_OpenAI_1
    {
        private static string _fromCulture = string.Empty;
        private static string _toCulture = string.Empty;
        public const string _settingsFilename = "TF_OpenAI_1.json";

        public static string Description(string funcType)
        {
            //Include a TYPE and DESCRIPTION.  You can add optional info. 
            string s = string.Empty;
            foreach (var item in Settings)
            {
                s = s + item.Key + ": " + item.Value + Environment.NewLine;
            }
            return
                "TYPE: " + funcType + Environment.NewLine +
                "DESCRIPTION: Calls API chat endpoint with a prompt that depends on the hint token used." + Environment.NewLine +
                "AUTHOR: Tetherscript" + Environment.NewLine +
                "CONTACT: support@tetherscript.com" + Environment.NewLine +
                "SETTINGS:" + _settingsFilename + Environment.NewLine +
                s;
        }

        public static bool InitGlobal(string fromCulture)
        {
            _fromCulture = fromCulture;
            TLog.Log(String.Format("TTransFunc_OpenAI_1: InitGlobal: fromCulture={0}", _fromCulture));
            return LoadSettings();
        }

        public static bool InitPerCulture(string fromCulture, string toCulture)
        {
            _fromCulture = fromCulture;
            _toCulture = toCulture;
            TLog.Log(String.Format("  TTransFunc_OpenAI_1: InitPerCulture: fromCulture={0}, toCulture={1}", _fromCulture, _toCulture));
            return true;
        }

        private static Dictionary<string, string> Settings = new Dictionary<string, string>();
        private static bool LoadSettings()
        {
            string path = Path.Combine(TUtils.TargetTranslatorPath, _settingsFilename);
            if (File.Exists(path))
            {
                try
                {
                    string loadedJson = File.ReadAllText(path);
                    // Deserialize to a list of LocalizedEntry
                    var newEntries = JsonSerializer.Deserialize<List<TUtils.HintKeyValEntry>>(
                        loadedJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                    Settings.Clear();
                    foreach (var entry in newEntries)
                    {
                        Settings[entry.Key] = entry.Value;
                    }
                    TLog.Log("TTransFunc_OpenAI_1: Settings loaded.");
                    return true;
                    //string setting1 = Settings["mykey"];
                }
                catch (Exception ex)
                {
                    TLog.Log(String.Format(
                        "TTransFunc_OpenAI_1.LoadSettings(): Error when loading settings: {0} : {1}",
                        ex.Message, path), true);
                    return false;
                }
            }
            else 
                return false;
        }

        public static bool SaveSettings()
        {
            string path = Path.Combine(TUtils.TargetTranslatorPath, _settingsFilename);
            if (File.Exists(path))
            {
                try
                {
                    var entries = Settings.Select(kvp => new TUtils.HintKeyValEntry
                    {
                        Key = kvp.Key,
                        Value = kvp.Value
                    }).ToList();
                    var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                    string jsonOutput = JsonSerializer.Serialize(entries, jsonOptions);
                    File.WriteAllText(path, jsonOutput);
                    TLog.Log("TTransFunc_OpenAI_1: Settings saved.");
                    return true;
                    //string setting1 = Settings["mykey"];
                }
                catch (Exception ex)
                {
                    TLog.Log(String.Format(
                        "TTransFunc_OpenAI_1.SaveSettings(): Error when saving settings: {0} : {1}",
                        ex.Message, path), true);
                    return false;
                }
            }
            else
                return false;
        }

        //OPENAI API TRANSLATION FUNCTION
        public static string Translate(string fromCulture, string toCulturestring, 
            string textToTranslate, string hintToken)
        {
            //returning null means this function has failed. It could be due to a bad return data structure.

            //However, the API may not understand the question and returns something like a confused 'I do not understand',
            //which is a valid return data structure, but a failed translation.  In this OpenAI API,
            //it would do that if we send None ....so we add single quotes going to, and remove the quotes on return.

            //Worst case is some obscure string has this confused bad translation that is rarely seen by users.
            //Best thing to do is to review the Translate log where you can see the API responses for translations.

            //this Translation Function is the original one used on this translator app.  It is not optimized.
            string openAiApiKey = Environment.GetEnvironmentVariable("OpenAIAPIKey1");

            const string openAiChatEndpoint = "https://api.openai.com/v1/chat/completions";

            using HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);

            string systemContent = String.Format(Settings[hintToken], _toCulture);
            string userContent = "'" + textToTranslate + "'"; //because sending None to the API returns a 'please specify the string response...'

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
            string doctxt = string.Empty;
            try
            {
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                JsonElement root = doc.RootElement;
                JsonElement choices = root.GetProperty("choices");
                JsonElement firstChoice = choices[0];
                JsonElement message = firstChoice.GetProperty("message");
                string translatedText = message.GetProperty("content").GetString() ?? string.Empty;
                doctxt = root.GetRawText();
                //remove the quotes
                translatedText = translatedText.Substring(1, translatedText.Length - 2);
                return translatedText.Trim();
            }
            catch
            {
                // Fallback in case of any unexpected structure
                TLog.Log("TTransFunc_OpenAI_1: Bad API return structure: " + Environment.NewLine + doctxt, true);
                return null;
            }
        }
    }
}
