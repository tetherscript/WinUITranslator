using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Translator;

//https://learn.microsoft.com/en-us/dotnet/ai/semantic-kernel-dotnet-overview
//https://devblogs.microsoft.com/semantic-kernel/using-json-schema-for-structured-output-in-net-for-openai-models/
//https://platform.openai.com/docs/guides/structured-outputs

public partial class TTranslatorEx
{
    private class TSOTransResult
    {
        //order of these fields is important.  ex. total_tokens is last because it isn't known at the beginning.
        public required string originalText { get; set; }
        public required string reasoning { get; set; }
        public required string translatedText { get; set; }
        public required int confidence { get; set; }
    }

    public async Task<TTranslatorResult> Translate_OpenAIAPI_SO(TLog.eLogType mode, string fromCulture, string toCulture, string textToTranslate, string hintToken, Dictionary<string, string> settings, IProgress<ProgressReport> report, CancellationToken cancellationToken)
    {
        List<string> _data = new();
        void Log(TLog.eLogItemType logType, int indent, string msg)
        {
            report?.Report(new ProgressReport(null,
                new TLogItem(mode, logType, indent, msg, _data)));
        }

        //GET APIKEY
        if (!settings.TryGetValue("api-key-environment-variable", out string api_key_environment_variable))
        {
            Log(TLog.eLogItemType.err, 2, "Settings 'api-key-environment-variable' is missing.");
            return new TTranslatorResult(
                    false,
                    textToTranslate,
                    null,
                    0);
        }

        //GET MODEL
        if (!settings.TryGetValue("model", out string model))
        {
            Log(TLog.eLogItemType.err, 2, "Settings 'model' not found.");
            return new TTranslatorResult(
                     false,
                     textToTranslate,
                     null,
                     0); 
        }

        //GET CONFIDENCE
        if (!settings.TryGetValue("min-confidence", out string minConfidenceStr))
        {
            Log(TLog.eLogItemType.err, 2, "Settings 'min-confidence' not found.");
            return new TTranslatorResult(
                     false,
                     textToTranslate,
                     null,
                     0);
        }
        if (!int.TryParse(minConfidenceStr, out int minConfidence))
        {
            Log(TLog.eLogItemType.err, 2, "Invalid 'min-confidence' value.");
            return new TTranslatorResult(
                     false,
                     textToTranslate,
                     null,
                     0);
        }

        //GET PROMPT FOR HINT TOKEN
        if (!settings.TryGetValue(hintToken, out string hintTokenPrompt))
        {
            Log(TLog.eLogItemType.err, 2, "Settings hint token '" + hintToken + "' not found.");
            return new TTranslatorResult(
                     false,
                     textToTranslate,
                     null,
                     0);
        }

        try
        {
            Kernel kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: model,
                apiKey: Environment.GetEnvironmentVariable(api_key_environment_variable)
            )
            .Build();

            #pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                Temperature = 0,
                ResponseFormat = typeof(TSOTransResult)
            };
            #pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            string rawPrompt = hintTokenPrompt;

            //escape String.Replace() placeholders one level because ex. {0} is a structured output function placeholder so we can't send that.
            string a = textToTranslate.Replace("{{", "[[").Replace("}}", "]]").Encapsulate("'");
            
            string formattedPrompt = String.Format(rawPrompt, fromCulture, toCulture, a);
            
            _data.Add("UNFORMATTED PROMPT" + Environment.NewLine + rawPrompt);
            _data.Insert(0, "FORMATTED PROMPT" + Environment.NewLine + formattedPrompt);
            var result = await kernel.InvokePromptAsync(formattedPrompt, new(executionSettings), null, null, cancellationToken);


            _data.Insert(0, "RAW RESULT" + Environment.NewLine + result.ToString());
            TSOTransResult translationResult = JsonSerializer.Deserialize<TSOTransResult>(result.ToString());

            //unescape String.Replace() placeholders one level
            string translatedText = translationResult.translatedText.Replace("[[", "{{").Replace("]]", "}}");

            if (translationResult.confidence < minConfidence)
            {
                Log(TLog.eLogItemType.wrn, 0, "<!> Confidence " + translationResult.confidence.ToString() + " < " + minConfidence.ToString());
                return new TTranslatorResult(
                    true,
                    translationResult.originalText,
                    "<!>" + translatedText,
                    translationResult.confidence,
                    _data);
            }
            else
            {
                _data.Insert(0, "REASONING" + Environment.NewLine + translationResult.reasoning);
                return new TTranslatorResult(
                    true,
                    translationResult.originalText,
                    translatedText,
                    translationResult.confidence,
                    _data);
            }
        }
        catch (Exception ex)
        {
            _data.Insert(0, "ERROR DETAILS" + Environment.NewLine + ex.Message);
            Log(TLog.eLogItemType.err, 2, "Translation failed.");
            return new TTranslatorResult(
                false,
                "",
                "<ERROR>",
                0,
                _data);
        }

    }
}
