using System.Collections.Generic;
using System.ComponentModel;

namespace Translator
{
    public static class TTransFunc
    {
        private const string _sample = "Sample";
        private const string _openAI_1 = "OpenAI_1";
        private const string _lMS_DeepSeekR1_OpenAIEmu = "LMS_DeepSeekR1_OpenAIEmul";
        //TLMS_DeepSeekR1_OpenAIEmulation
        public static List<string> Types = new List<string>
        {
            _openAI_1,
            _lMS_DeepSeekR1_OpenAIEmu,
            _sample
        };

        public static bool InitGlobal(string funcType, string fromCulture)
        {
            //called at the beginning of the translation task
            switch (funcType)
            {
                case _sample: return false;
                case _openAI_1: return TTF_OpenAI_1.InitGlobal(fromCulture);
                case _lMS_DeepSeekR1_OpenAIEmu: return TTF_LMS_DeepSeekR1_OpenAIEmul.InitGlobal(fromCulture);
                default: return false;
            }
        }

        public static bool InitPerCulture(string funcType, string fromCulture, string toCulture)
        {
            //called just before calling the first .Translate call for a culture ex: 'de-DE'
            switch (funcType)
            {
                case _sample: return false;
                case _openAI_1: return TTF_OpenAI_1.InitPerCulture(fromCulture, toCulture); 
                case _lMS_DeepSeekR1_OpenAIEmu: return TTF_LMS_DeepSeekR1_OpenAIEmul.InitPerCulture(fromCulture, toCulture);
                default: return false;
            }
        }

        public static string Translate(string funcType, string fromCulture, string toCulture, string textToTranslate, 
            string hintToken)
        {
            //called for each translateable item
            switch (funcType)
            {
                case _sample: return null;
                case _openAI_1: return TTF_OpenAI_1.Translate(fromCulture, toCulture, textToTranslate, hintToken);
                case _lMS_DeepSeekR1_OpenAIEmu: return TTF_LMS_DeepSeekR1_OpenAIEmul.Translate(fromCulture, toCulture, textToTranslate, hintToken);
                default: return null;
            }
        }

    }
}
