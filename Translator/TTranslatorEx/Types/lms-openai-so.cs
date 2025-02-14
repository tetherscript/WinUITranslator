﻿using System;
using System.Collections.Generic;
using System.IO;
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
    //https://platform.openai.com/docs/guides/text-generation?example=completions

    public async Task<TTranslatorResult> Translate_LMS_OpenAI_SO(TLog.eLogType mode, string fromCulture, string toCulture, string textToTranslate, string hintToken, Dictionary<string, string> settings, IProgress<ProgressReport> report, CancellationToken cancellationToken)
    {
        List<string> _data = new();
        void Log(TLog.eLogItemType logType, int indent, string msg)
        {
            report?.Report(new ProgressReport(null,
                new TLogItem(mode, logType, indent, msg, _data)));
        }

        int _totalSendTokens = 0;
        int _totalReceiveTokens = 0;
        int _totalTokens = 0;

        //GET APIKEY
        if (!settings.TryGetValue("api-key", out string api_key))
        {
            //invalid Key entry in settings
            Log(TLog.eLogItemType.err, 2, "Settings 'api-key' is missing.");
            return new TTranslatorResult(
                    false,
                    textToTranslate,
                    null,
                    0,
                    "");
        }

        //GET HOST
        if (!settings.TryGetValue("host", out string host))
        {
            //invalid Key entry in settings
            Log(TLog.eLogItemType.err, 2, "Settings 'host' is missing.");
            return new TTranslatorResult(
                    false,
                    textToTranslate,
                    null,
                    0,
                    "");
        }

        //GET PROMPT FOR HINT TOKEN
        if (!settings.TryGetValue(hintToken, out string hintTokenPrompt))
        {
            Log(TLog.eLogItemType.err, 2, "Settings hint token '" + hintToken + "' not found.");
            return new TTranslatorResult(
                     false,
                     textToTranslate,
                     null,
                     0,
                     "");
        }

        //GET MODEL
        if (!settings.TryGetValue("model", out string model))
        {
            Log(TLog.eLogItemType.err, 2, "Settings 'model' not found.");
            return new TTranslatorResult(
                     false,
                     textToTranslate,
                     null,
                     0,
                     "");
        }

        //GET CONFIDENCE
        if (!settings.TryGetValue("min-confidence", out string minConfidenceStr))
        {
            Log(TLog.eLogItemType.err, 2, "Settings 'min-confidence' not found.");
            return new TTranslatorResult(
                     false,
                     textToTranslate,
                     null,
                     0,
                     "");
        }
        if (int.TryParse(minConfidenceStr, out int minConfidence))
        {
            
        }
        else
        {
            Log(TLog.eLogItemType.err, 2, "Invalid 'min-confidence' Value.");
            return new TTranslatorResult(
                     false,
                     textToTranslate,
                     null,
                     0,
                     "");
        }

        using HttpClient httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", api_key);

        //PREPARE PROMPT
        string rawPrompt = hintTokenPrompt;
        string a = textToTranslate.Replace("{{", "[[").Replace("}}", "]]").Encapsulate("'");
        string formattedPrompt = String.Format(rawPrompt, fromCulture, toCulture, a);

        string p = @"C:\1\test.json";
        string rf =  await File.ReadAllTextAsync(p, Encoding.UTF8);

        //https://lmstudio.ai/docs/api/structured-output
        string x = "You are a helpful AI Assistant";
        var requestBody = new
        {
            model = model,
            messages = new[]
            {
                //new { role = "tool", content = rf },
                new { role = "system", content = x },
                new { role = "user", content = formattedPrompt }
            },
            temperature = 0.0,      //keep it at zero so it is deterministic
            response_format = rf,
            //structured = rf
           
        };

        //COMPOSE AND SEND REQUEST
        var requestContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        string jsonRequestBody = JsonSerializer.Serialize(requestBody, options);
        _data.Add("REQUEST_BODY" + Environment.NewLine + jsonRequestBody);


        string jsonResponse = string.Empty;
        try
        {
            try
            {
                using HttpResponseMessage response = await httpClient.PostAsync(host, requestContent, cancellationToken);
                jsonResponse = response.Content.ReadAsStringAsync().Result;
                HttpStatusCode statusCode = response.StatusCode;
                _data.Add("RESPONSE" + Environment.NewLine + jsonResponse);
                if (statusCode != System.Net.HttpStatusCode.OK)
                {
                    Log(TLog.eLogItemType.err, 2, "Error in response: statusCode = " + statusCode.ToString());
                    return new TTranslatorResult(
                        false,
                        textToTranslate,
                        null,
                        0,
                        "");
                }
            }
            catch (OperationCanceledException)
            {
                Log(TLog.eLogItemType.err, 2, "Cancelled.");
                return new TTranslatorResult(
                    false,
                    textToTranslate,
                    null,
                    0,
                    "");
            }
        }
        catch (Exception ex)
        {
            Log(TLog.eLogItemType.err, 2, "httpClient.PostAsync() error: " + ex.Message);
            return new TTranslatorResult(
                    false,
                    textToTranslate,
                    null,
                    0,
                    "");
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
                Log(TLog.eLogItemType.err, 2, formattedPrompt);
                return new TTranslatorResult(
                     false,
                     textToTranslate,
                     null,
                     0,
                     "");
            }

            JsonElement firstChoice = choices[0];
            JsonElement message = firstChoice.GetProperty("message");
            string contentJson = message.GetProperty("content").GetString() ?? string.Empty;

            _data.Insert(0, "RESPONSE.CHOICES[0].MESSAGE.CONTENT" + Environment.NewLine + contentJson);

            // Deserialize to a list of LocalizedEntry
            TContentGeneric content = JsonSerializer.Deserialize<TContentGeneric>(
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

            if (translationConfidence < minConfidence)
            {
                Log(TLog.eLogItemType.wrn, 0, "<!> Confidence " + translationConfidence.ToString() + " < " + minConfidence.ToString());
                translatedText = "<!>" + translatedText;
            }

            //status
            string finish_reason = firstChoice.GetProperty("finish_reason").GetString();
            switch (finish_reason)
            {
                case "stop":
                    return new TTranslatorResult(
                    true,
                    textToTranslate,
                    translatedText,
                    0,
                    "",
                    _data);
                case "length":
                    Log(TLog.eLogItemType.err, 2, "The model hit the maximum request body token limit (max_tokens). Increase max_tokens or reduce userContent length: " + formattedPrompt);
                    return new TTranslatorResult(
                         false,
                         textToTranslate,
                         null,
                         0,
                         "");
                case "content_filter":
                    Log(TLog.eLogItemType.err, 2, "The response was blocked due to safety or policy filters (e.g., violating OpenAI's content guidelines): " + formattedPrompt);
                    return new TTranslatorResult(
                         false,
                         textToTranslate,
                         null,
                         0,
                         "");
                default:
                    return new TTranslatorResult(
                         false,
                         textToTranslate,
                         null,
                         0,
                         "");
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
                 0,
                 "");
        }
    
    }
}
