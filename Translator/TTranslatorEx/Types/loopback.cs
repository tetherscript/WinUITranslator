using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Translator;

public partial class TTranslatorEx
{
    public async Task<TTranslatorResult> Translate_Loopback(TLog.eLogType mode, string fromCulture, string toCulture, string textToTranslate, string hintToken, Dictionary<string, string> settings, IProgress<ProgressReport> report, CancellationToken cancellationToken)
    {
        await Task.Delay(100);
        return new TTranslatorResult(
            true,
            textToTranslate,
            "$" + textToTranslate,
            100,
            "");
    }
}


