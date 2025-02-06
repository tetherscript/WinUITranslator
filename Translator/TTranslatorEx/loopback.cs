using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Translator;

public partial class TTranslatorEx
{
    public async Task<TTranslatorResult> Translate_Loopback(TLog.eMode mode, string fromCulture, string toCulture, string textToTranslate, string hintToken, Dictionary<string, string> settings, IProgress<TranslateProgressReport> report, CancellationToken cancellationToken)
    {
        return new TTranslatorResult(
            true,
            textToTranslate,
            "$" + textToTranslate,
            100,
            null);
    }
}


