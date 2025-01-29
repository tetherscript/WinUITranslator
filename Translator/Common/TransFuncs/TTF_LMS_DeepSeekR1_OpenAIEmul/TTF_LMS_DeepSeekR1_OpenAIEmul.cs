﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Translator
{
    public static class TLMS_DeepSeekR1_OpenAIEmul
    {
        private static string _fromCulture = string.Empty;
        private static string _toCulture = string.Empty;
        private static string _logPrefix = "DeepSeekR1_OpenAIEmul";
        private const string _settingsFilename = "TLMS_DeepSeekR1_OpenAIEmul.json";
        private static int _totalSendTokens = 0;
        private static int _totalReceiveTokens = 0;
        private static int _totalTokens = 0;
        private static int _requestBodyMaxTokens = 10;

        private static void Log(string msg, bool isError = false)
        {
            TLog.Log((isError ? TLog.eLogType.err : TLog.eLogType.inf), 0, _logPrefix + ": " + msg);
        }

        public static bool InitGlobal(string fromCulture)
        {
            _totalSendTokens = 0;
            _totalReceiveTokens = 0;
            _totalTokens = 0;
            _fromCulture = fromCulture;
            TLog.Log(TLog.eLogType.inf, 0, String.Format(_logPrefix + ": InitGlobal: fromCulture={0}", _fromCulture));
            TLog.Log(TLog.eLogType.inf, 0, _logPrefix + ": DESCRIPTION: Calls DeepSeek R1 on LM Studio API with OpenAI API chat endpoint emulation");
            TLog.Log(TLog.eLogType.inf, 0, _logPrefix + ": AUTHOR: Tetherscript");
            TLog.Log(TLog.eLogType.inf, 0, _logPrefix + ": CONTACT: support@tetherscript.com");
            TLog.Log(TLog.eLogType.inf, 0, _logPrefix + ": SETTINGS: " + _settingsFilename);
            return LoadSettings();
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
                    Log("Settings loaded.");
                    return true;
                    //string setting1 = Settings["mykey"];
                }
                catch (Exception ex)
                {
                    Log(String.Format(
                        "LoadSettings(): Error when loading settings: {0} : {1}",
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
                    Log("Settings saved.");
                    return true;
                }
                catch (Exception ex)
                {
                    Log(String.Format(
                        "SaveSettings(): Error when saving settings: {0} : {1}",
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
                temperature = 0.0,  //keep it at zero so it is deterministic
                max_tokens = 1000   //=data sent + data returned
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
                        Log("The model hit the maximum request body token limit (max_tokens). Increase max_tokens or reduce userContent length: " + userContent, true);
                        return null;
                    case "content_filter":
                        Log("The response was blocked due to safety or policy filters (e.g., violating OpenAI's content guidelines): " + userContent, true);
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
