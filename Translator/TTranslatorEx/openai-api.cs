using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Translator;

public partial class TTranslatorEx
{
    class TContent_openai_api
    {
        public string translated { get; set; }
        public int confidence { get; set; }
    }

    public async Task<TTranslatorResult> Translate_OpenAIAPI(TLog.eMode mode, string fromCulture, string toCulture, string textToTranslate, string hintToken, Dictionary<string, string> settings, IProgress<TranslateProgressReport> report, CancellationToken cancellationToken)
    {

        void Log(TLog.eLogItemType logType, int indent, string msg)
        {
            report?.Report(new TranslateProgressReport(null,
                new TLogItem(TLog.eMode.translate, logType, indent, msg)));
        }

        int _totalSendTokens = 0;
        int _totalReceiveTokens = 0;
        int _totalTokens = 0;

        //GET APIKEY
        if (!settings.TryGetValue("api_key", out string api_key_raw))
        {
            //invalid key entry in settings
            Log(TLog.eLogItemType.err, 2, "Settings 'api_key' is missing.");
            return new TTranslatorResult(
                    false,
                    textToTranslate,
                    null,
                    null,
                    null);
        }
        List<string> api_key_parts = api_key_raw.Split(':').ToList();
        string apiKey;
        if (api_key_parts[0] == "environment-variable")
        {
            apiKey = Environment.GetEnvironmentVariable(api_key_parts[1]);
        }
        else
        {
            apiKey = api_key_raw;
            //invalid key entry in settings
            // Log(TLog.eLogItemType.err, 2, "Settings 'api_key' must start with 'environment-variable:'.");
            //return (null, logItems);
        }

        //GET HOST
        if (!settings.TryGetValue("host", out string host))
        {
            //invalid key entry in settings
            Log(TLog.eLogItemType.err, 2, "Settings 'host' is missing.");
            return new TTranslatorResult(
                    false,
                    textToTranslate,
                    null,
                    null,
                    null);
        }

        //GET PROMPT FOR HINT TOKEN
        if (!settings.TryGetValue(hintToken, out string systemPrompt))
        {
            Log(TLog.eLogItemType.err, 2, "Settings hint token '" + hintToken + "' not found.");
            return new TTranslatorResult(
                     false,
                     textToTranslate,
                     null,
                     null,
                     null);
        }

        //GET MAX_TOKENS
        if (!settings.TryGetValue("max_tokens", out string max_tokens))
        {
            Log(TLog.eLogItemType.err, 2, "Settings 'max_tokens' not found.");
            return new TTranslatorResult(
                    false,
                    textToTranslate,
                    null,
                    null,
                    null);
        }

        //GET MODEL
        if (!settings.TryGetValue("model", out string model))
        {
            Log(TLog.eLogItemType.err, 2, "Settings 'model' not found.");
            return new TTranslatorResult(
                     false,
                     textToTranslate,
                     null,
                     null,
                     null);
        }

        //GET MODEL
        if (!settings.TryGetValue("character_schema", out string character_schema))
        {
            Log(TLog.eLogItemType.err, 2, "Settings 'character_schema' not found.");
            return new TTranslatorResult(
                     false,
                     textToTranslate,
                     null,
                     null,
                     null);
        }


        using HttpClient httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        //PREPARE PROMPT
        string systemContent = String.Format(systemPrompt, fromCulture, toCulture);
        string userContent = "'" + textToTranslate + "'"; //because sending None to the API returns a 'please specify the string response...'

        Log(TLog.eLogItemType.dbg, 2, "hintToken=" + hintToken);
        Log(TLog.eLogItemType.dbg, 2, "systemContent=" + systemContent);
        Log(TLog.eLogItemType.dbg, 2, "userContent=" + userContent);
        Log(TLog.eLogItemType.dbg, 2, "max_tokens=" + max_tokens);

        character_schema = @"{""{\""$schema\"": \""http://json-schema.org/draft-07/schema#\"", \""type\"": \""object\"", \""properties\"": { \""translated\"": { \""type\"": \""string\"", \""description\"": \""The translated text.\"" }, \""confidence\"": { \""type\"": \""integer\"", \""description\"": \""The confidence level of the translation, expressed as an integer.\"" } }, \""required\"": [\""translated\"", \""confidence\""], \""additionalProperties\"": false }""";

        string jsonSchema = @"{ ""type"": ""json"" }";

        //https://platform.openai.com/docs/api-reference/chat/create
        var requestBody = new
        {
            model = "gpt-4",
            messages = new[]
            {
                    new { role = "system", content = systemContent},
                    new { role = "user", content = userContent}
                    },
            temperature = 0.0,      //keep it at zero so it is deterministic
            //max_completion_tokens = max_tokens,
            //response_format = jsonSchema
            //dd = "dd"
        };

        //COMPOSE AND SEND REQUEST
        var requestContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        string jsonResponse = string.Empty;
        try
        {


            using HttpResponseMessage response = httpClient.PostAsync(host, requestContent).Result;
            HttpStatusCode statusCode = response.StatusCode;
            Log(TLog.eLogItemType.dbg, 2, "HttpResponseMessage.StatusCode = " + statusCode.ToString());
            if (statusCode != System.Net.HttpStatusCode.OK)
            {
                Log(TLog.eLogItemType.err, 2, "Error in response: statusCode = " + statusCode.ToString());
                return new TTranslatorResult(
                    false,
                    textToTranslate,
                    null,
                    null,
                    null);
            }
            jsonResponse = response.Content.ReadAsStringAsync().Result;
        }
        catch (Exception ex)
        {
            Log(TLog.eLogItemType.err, 2, "httpClient.PostAsync() error: " + ex.Message);
            return new TTranslatorResult(
                    false,
                    textToTranslate,
                    null,
                    null,
                    null);
        }


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
                Log(TLog.eLogItemType.err, 2, errortype + ": " + errorMsg);
                Log(TLog.eLogItemType.err, 2, userContent);
                return new TTranslatorResult(
                     false,
                     textToTranslate,
                     null,
                     null,
                     null);
            }

            JsonElement firstChoice = choices[0];
            JsonElement message = firstChoice.GetProperty("message");
            string contentJson = message.GetProperty("content").GetString() ?? string.Empty;

            // Deserialize to a list of LocalizedEntry
            TContent_openai_api content = JsonSerializer.Deserialize<TContent_openai_api>(
                contentJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            int translationConfidence = content.confidence;
            string translatedText = content.translated.Trim();
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
                    return new TTranslatorResult(
                    true,
                    textToTranslate,
                    translatedText,
                    null,
                    null);
                    //translatedText.Length - 2), logItems);
                case "length":
                    Log(TLog.eLogItemType.err, 2, "The model hit the maximum request body token limit (max_tokens). Increase max_tokens or reduce userContent length: " + userContent);
                    return new TTranslatorResult(
                         false,
                         textToTranslate,
                         null,
                         null,
                         null);
                case "content_filter":
                    Log(TLog.eLogItemType.err, 2, "The response was blocked due to safety or policy filters (e.g., violating OpenAI's content guidelines): " + userContent);
                    return new TTranslatorResult(
                         false,
                         textToTranslate,
                         null,
                         null,
                         null);
                default:
                    return new TTranslatorResult(
                         false,
                         textToTranslate,
                         null,
                         null,
                         null);
            }
        }
        catch
        {
            // Fallback in case of any unexpected structure
            Log(TLog.eLogItemType.err, 2, "Bad API return structure: " + Environment.NewLine + doctxt);
            return new TTranslatorResult(
                 false,
                 textToTranslate,
                 null,
                 null,
                 null);
        }
    
    }
}
