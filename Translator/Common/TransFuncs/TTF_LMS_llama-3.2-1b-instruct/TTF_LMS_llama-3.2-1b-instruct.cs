using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Net;

namespace Translator
{
    public static class TTF_LMS_llama_3_2_1b_instruct
    {
        private static string _fromCulture = string.Empty;
        private static string _toCulture = string.Empty;
        private static string _logPrefix = "TTF_LMS_llama-3.2-1b-instruct";
        private const string _settingsFilename = "TTF_LMS_llama-3.2-1b-instruct.json";
        private static int _totalSendTokens = 0;
        private static int _totalReceiveTokens = 0;
        private static int _totalTokens = 0;
        private static int _requestBodyMaxTokens = 1000;

        private static void Log(TLog.eLogType type, int indent, string msg, bool isError = false)
        {
            TLog.Log(type, indent, _logPrefix + ": " + msg);
        }

        public static bool InitGlobal(string fromCulture)
        {
            _totalSendTokens = 0;
            _totalReceiveTokens = 0;
            _totalTokens = 0;
            _fromCulture = fromCulture;
            TLog.Log(TLog.eLogType.inf, 0, String.Format(_logPrefix + ": InitGlobal: fromCulture={0}", _fromCulture));
            TLog.Log(TLog.eLogType.inf, 0, _logPrefix + ": DESCRIPTION: The model is 'llama-3.2-1b-instruct' hosted on LM Studio.");
            TLog.Log(TLog.eLogType.inf, 0, _logPrefix + ": AUTHOR: Tetherscript");
            TLog.Log(TLog.eLogType.inf, 0, _logPrefix + ": CONTACT: support@tetherscript.com");
            TLog.Log(TLog.eLogType.inf, 0, _logPrefix + ": SETTINGS: " + _settingsFilename);
            bool res = LoadSettings();
            foreach (var item in Settings)
            {
                string s = item.Key + ": " + item.Value;
                TLog.Log(TLog.eLogType.inf, 0, _logPrefix + ": SETTINGS: " + s);
            }
            return res;
        }

        public static bool InitPerCulture(string fromCulture, string toCulture)
        {
            _fromCulture = fromCulture;
            _toCulture = toCulture;
            TLog.Log(TLog.eLogType.inf, 2, String.Format(_logPrefix + ": InitPerCulture: fromCulture={0}, toCulture={1}", _fromCulture, _toCulture));
            return true;
        }

        public static bool DeInitGlobal()
        {
            string s = String.Format("Summary: Used {0} prompt tokens + {1} completion_tokens = {2} total tokens.",
                _totalSendTokens, _totalReceiveTokens, _totalTokens);
            TLog.Log(TLog.eLogType.inf, 0, s);
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
                    Log(TLog.eLogType.inf, 0, "SETTINGS: loaded.");
                    return true;
                    //string setting1 = Settings["mykey"];
                }
                catch (Exception ex)
                {
                    Log(TLog.eLogType.err, 0, String.Format(
                        "LoadSettings(): Error when loading settings: {0} : {1}",
                        ex.Message, path));
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
                    Log(TLog.eLogType.inf, 0, "Settings saved.");
                    return true;
                }
                catch (Exception ex)
                {
                    Log(TLog.eLogType.err, 0, String.Format(
                        "SaveSettings(): Error when saving settings: {0} : {1}",
                        ex.Message, path));
                    return false;
                }
            }
            else
                return false;
        }

        public static string Translate(string fromCulture, string toCulturestring,
            string textToTranslate, string hintToken)
        {
            //returning null means this function has failed. It could be due to a bad return data structure.

            //However, the API may not understand the question and returns something like a confused 'I do not understand',
            //which is a valid return data structure, but a failed translation.  

            //Worst case is some obscure string has this confused bad translation that is rarely seen by users.
            //Best thing to do is to review the Translate log where you can see the API responses for translations.

            string openAiApiKey = "lm-studio";

            //const string openAiChatEndpoint = "https://api.openai.com/v1/chat/completions";
            const string openAiChatEndpoint = "http://localhost:1234/v1/chat/completions";

            using HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);

            //string systemContent = String.Format(Settings[hintToken], _toCulture);

            string z = @"You are a professional language translator.  Translate the following text from American English (en - US) to ({0}).";

            string userContent = "'" + textToTranslate + "'"; //because sending None to the API returns a 'please specify the string response...'
            string systemContent = String.Format(z, _toCulture);

          
           if (!Settings.TryGetValue("max_tokens", out string mt))
            {
                Log(TLog.eLogType.inf, 0, "max_tokens not defined in " + _settingsFilename + " defaulting to 1000.");
                _requestBodyMaxTokens = 1000;
            }

            var requestBody = new
            {
                model = "gpt-4",
                messages = new[]
                {
                    new { role = "system", content = systemContent},
                    new { role = "user", content = userContent}
                    },
                temperature = 0.0,  //keep it at zero so it is deterministic
                max_tokens = _requestBodyMaxTokens   //=data sent + data returned
            };

            //COMPOSE AND SEND REQUEST
            var requestContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );
            using HttpResponseMessage response = httpClient.PostAsync(openAiChatEndpoint, requestContent).Result;
            HttpStatusCode statusCode = response.StatusCode;
            if (statusCode != System.Net.HttpStatusCode.OK)
            {
                Log(TLog.eLogType.err, 0, "Error in response: statusCode = " + statusCode.ToString());
                return null;
            }
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
                JsonElement error;
                if (root.TryGetProperty("error", out error))
                {
                    string errorMsg = error.GetProperty("message").GetString() ?? string.Empty;
                    string errortype = error.GetProperty("type").GetString() ?? string.Empty;
                    TLog.Log(TLog.eLogType.err, 0, errortype + ": " + errorMsg);
                    TLog.Log(TLog.eLogType.err, 0, userContent);
                    return null;
                }

                JsonElement firstChoice = choices[0];
                JsonElement message = firstChoice.GetProperty("message");
                string translatedText = message.GetProperty("content").GetString() ?? string.Empty;
                doctxt = root.GetRawText();

                //usage
                JsonElement usage = root.GetProperty("usage");
                int promptSendTokens = usage.GetProperty("prompt_tokens").GetInt32();
                int completion_tokens = usage.GetProperty("completion_tokens").GetInt32();
                int total_tokens = usage.GetProperty("total_tokens").GetInt32();
                _totalSendTokens = _totalSendTokens + promptSendTokens;
                _totalReceiveTokens = _totalReceiveTokens + completion_tokens;
                _totalTokens = _totalTokens + total_tokens;

                //status
                string finish_reason = firstChoice.GetProperty("finish_reason").GetString();
                switch (finish_reason)
                {
                    case "stop":
                        translatedText = translatedText.Substring(1, translatedText.Length - 2);
                        return translatedText.Trim();
                    case "length":
                        Log(TLog.eLogType.err, 0, "The model hit the maximum request body token limit (max_tokens). Increase max_tokens or reduce userContent length: " + userContent);
                        return null;
                    case "content_filter":
                        Log(TLog.eLogType.err, 0, "The response was blocked due to safety or policy filters (e.g., violating OpenAI's content guidelines): " + userContent);
                        return null;
                    default: return null;
                }
            }
            catch
            {
                // Fallback in case of any unexpected structure
                TLog.Log(TLog.eLogType.err, 0, _logPrefix + ": Bad API return structure: " + Environment.NewLine + doctxt);
                return null;
            }
        }
    }
}
