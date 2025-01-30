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
    public static class TTF_OpenAI_1
    {
        private static string _fromCulture = string.Empty;
        private static string _toCulture = string.Empty;
        private static string _logPrefix = "OpenAI_1";
        private const string _settingsFilename = "TTF_OpenAI_1.json";
        private static int _totalSendTokens = 0;
        private static int _totalReceiveTokens = 0;
        private static int _totalTokens = 0;
        private static int _requestBodyMaxTokens = 1000;

        private static void Log(TLog.eMode mode, TLog.eLogItemType type, int indent, string msg)
        {
            TLog.Log(mode, type, indent, _logPrefix + ": " + msg);
        }

        public static bool InitGlobal(TLog.eMode mode, string fromCulture)
        {
            bool res = true;
            _totalSendTokens = 0;
            _totalReceiveTokens = 0;
            _totalTokens = 0;
            _fromCulture = fromCulture;
            try
            {
                bool settingsLoaded = LoadSettings(mode);
                if (settingsLoaded)
                {
                    if (mode == TLog.eMode.translate)
                    {
                        Log(mode, TLog.eLogItemType.inf, 0, String.Format("InitGlobal: fromCulture={0}", _fromCulture));
                        Log(mode, TLog.eLogItemType.inf, 0, "DESCRIPTION: Calls the OpenAI API chat endpoint");
                        Log(mode, TLog.eLogItemType.inf, 0, "AUTHOR: Tetherscript");
                        Log(mode, TLog.eLogItemType.inf, 0, "CONTACT: support@tetherscript.com");
                        Log(mode, TLog.eLogItemType.inf, 0, "SETTINGS: " + _settingsFilename);
                        foreach (var item in Settings)
                        {
                            string s = item.Key + ": " + item.Value;
                            Log(mode, TLog.eLogItemType.inf, 0, "SETTING: " + s);
                        }
                    }
                }
                else
                {
                    Log(mode, TLog.eLogItemType.err, 2, "InitGlobal LoadSettings failed: " + _settingsFilename);
                }


                return res;
            }
            catch (Exception ex)
            {
                TLog.Log(mode, TLog.eLogItemType.err, 2, "InitGlobal Exception: " + ex.Message);
            }
            return res;
        }

        public static bool InitPerCulture(TLog.eMode mode, string fromCulture, string toCulture)
        {
            bool res = true;
            _fromCulture = fromCulture;
            _toCulture = toCulture;
            try
            {
                if (mode == TLog.eMode.translate)
                {
                    Log(mode, TLog.eLogItemType.inf, 2, String.Format("InitPerCulture: fromCulture={0}, toCulture={1}", _fromCulture, _toCulture));
                }
            }
            catch (Exception ex)
            {
                Log(mode, TLog.eLogItemType.err, 2, "InitPerCulture Exception: " + ex.Message);
            }
            return res;
        }

        public static bool DeInitGlobal(TLog.eMode mode)
        {
            string s = String.Format("Summary: Used {0} prompt tokens + {1} completion_tokens = {2} total tokens.",
                _totalSendTokens, _totalReceiveTokens, _totalTokens);
            Log(mode, TLog.eLogItemType.inf, 0, s);
            return true;
        }

        public static string GetSettingsPath()
        {
            return Path.Combine(TUtils.TargetTranslatorPath, _settingsFilename);
        }

        private static Dictionary<string, string> Settings = new Dictionary<string, string>();
        private static bool LoadSettings(TLog.eMode mode)
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
                    Log(mode, TLog.eLogItemType.dbg, 2, "SETTINGS: loaded.");
                    return true;
                    //string setting1 = Settings["mykey"];
                }
                catch (Exception ex)
                {
                    Log(mode, TLog.eLogItemType.err, 2, String.Format(
                        "LoadSettings(): Error when loading settings: {0} : {1}",
                        ex.Message, path));
                    return false;
                }
            }
            else 
                return false;
        }

        public static bool SaveSettings(TLog.eMode mode)
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
                    Log(mode, TLog.eLogItemType.dbg, 2, "Settings saved.");
                    return true;
                }
                catch (Exception ex)
                {
                    Log(mode, TLog.eLogItemType.err, 2, String.Format(
                        "SaveSettings(): Error when saving settings: {0} : {1}",
                        ex.Message, path));
                    return false;
                }
            }
            else
                return false;
        }

        //OPENAI API TRANSLATION FUNCTION
        public static string Translate(TLog.eMode mode, string fromCulture, string toCulturestring, 
            string textToTranslate, string hintToken)
        {
            //returning null means this function has failed, but it could have several causes.

            //However, the API may not understand the question and returns something like a confused 'I do not understand',
            //which is a valid return data structure, but a failed translation.  This will require tweaking the System message so that it's instructions are more clear.

            //Unfortunately, there's no way to detect this.  You'll need to visually scan the log to see if any of these confused
            //results were returned.

            string openAiApiKey = Environment.GetEnvironmentVariable("OpenAIAPIKey1");

            const string openAiChatEndpoint = "https://api.openai.com/v1/chat/completions";

            using HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);
            if (!Settings.TryGetValue(hintToken, out string token))
            {
                Log(mode, TLog.eLogItemType.err, 2, "Hint token not found in: " + _settingsFilename);
                return null;
            }
            
            string systemContent = String.Format(token, _toCulture);
            string userContent = "'" + textToTranslate + "'"; //because sending None to the API returns a 'please specify the string response...'

            if (!Settings.TryGetValue("max_tokens", out string mt))
            {
                Log(mode, TLog.eLogItemType.err, 2, "max_tokens not found in: " + _settingsFilename);
                return null;
            }
            if (!int.TryParse(mt, out _requestBodyMaxTokens))
            {
                _requestBodyMaxTokens = 1000;
            }

            Log(mode, TLog.eLogItemType.dbg, 2, "hintToken=" + hintToken);
            Log(mode, TLog.eLogItemType.dbg, 2, "systemContent=" + systemContent);
            Log(mode, TLog.eLogItemType.dbg, 2, "userContent=" + userContent);
            Log(mode, TLog.eLogItemType.dbg, 2, "max_tokens=" + _requestBodyMaxTokens.ToString());


            var requestBody = new
            {
                model = "gpt-4",
                messages = new[]
                {
                    new { role = "developer", content = systemContent},
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
            Log(mode, TLog.eLogItemType.dbg, 2, "HttpResponseMessage.StatusCode = " + statusCode.ToString());
            if (statusCode != System.Net.HttpStatusCode.OK)
            {
                Log(mode, TLog.eLogItemType.err, 2, "Error in response: statusCode = " + statusCode.ToString());
                return null;
            }
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
                    Log(mode, TLog.eLogItemType.err, 2, errortype + ": " + errorMsg);
                    Log(mode, TLog.eLogItemType.err, 2, userContent);
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
                string finish_reason =  firstChoice.GetProperty("finish_reason").GetString();
                switch (finish_reason) {
                    case "stop":
                        translatedText = translatedText.Substring(1, translatedText.Length - 2);
                        return translatedText.Trim();
                    case "length": Log(mode, TLog.eLogItemType.err, 2, "The model hit the maximum request body token limit (max_tokens). Increase max_tokens or reduce userContent length: " + userContent);
                        return null;
                    case "content_filter": Log(mode, TLog.eLogItemType.err, 2, "The response was blocked due to safety or policy filters (e.g., violating OpenAI's content guidelines): " + userContent); 
                        return null;
                    default: return null;
                }
            }
            catch
            {
                // Fallback in case of any unexpected structure
                Log(mode, TLog.eLogItemType.err, 2, _logPrefix + ": Bad API return structure: " + Environment.NewLine + doctxt);
                return null;
            }
        }
    }
}
