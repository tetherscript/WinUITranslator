using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Translator;

public partial class TTranslatorEx
{

    public class TContentGeneric1
    {
        public string translated { get; set; }
        public int confidence { get; set; }
    }

    public class TContentGeneric2
    {
        public string originalText { get; set; }
        public string translatedText { get; set; }
        public string reasoning { get; set; }
        public int confidence { get; set; }
    }

    public async Task<TTranslatorResult> Translate(TLog.eLogType mode, string profile, string fromCulture, string toCulture, string textToTranslate, string hintToken, Dictionary<string, string> settings, IProgress<ProgressReport> report, CancellationToken cancellationToken)
    {
        string type = settings["type"];
        switch (type)
        {
            case "loopback": return await Translate_Loopback(mode, fromCulture, toCulture, textToTranslate, hintToken, settings, report, cancellationToken);
            case "openai-api": return await Translate_OpenAIAPI(mode, fromCulture, toCulture, textToTranslate, hintToken, settings, report, cancellationToken);
            case "openai-api-so": return await Translate_OpenAIAPI_SO(mode, fromCulture, toCulture, textToTranslate, hintToken, settings, report, cancellationToken);
            case "lms-so": return await Translate_LMS_OpenAI_SO(mode, fromCulture, toCulture, textToTranslate, hintToken, settings, report, cancellationToken);
            default:
                return new TTranslatorResult(
                    false,
                    textToTranslate,
                    null,
                    0,
                    "");
        }
    }
}
