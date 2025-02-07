using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Translator;

public partial class TTranslatorEx
{
    public async Task<TTranslatorResult> Translate(TLog.eLogType mode, string profile, string fromCulture, string toCulture, string textToTranslate, string hintToken, Dictionary<string, string> settings, IProgress<ProgressReport> report, CancellationToken cancellationToken)
    {
        string type = settings["type"];
        switch (type)
        {
            case "loopback": return await Translate_Loopback(mode, fromCulture, toCulture, textToTranslate, hintToken, settings, report, cancellationToken);
            case "openai-api": return await Translate_OpenAIAPI(mode, fromCulture, toCulture, textToTranslate, hintToken, settings, report, cancellationToken);
            default:
                return new TTranslatorResult(
                    false,
                    textToTranslate,
                    null,
                    null,
                    null);
        }
    }
}
