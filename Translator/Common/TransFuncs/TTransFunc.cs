using System.Collections.Generic;
using System.ComponentModel;

namespace Translator
{
    public static class TTransFunc
    {
        private const string _sample = "Sample";
        private const string _openAI_1 = "OpenAI_1";
        private const string _LMS_llama_3_2_1b_instruct = "LMS_llama-3.2-1b-instruct";

        //TLMS_DeepSeekR1_OpenAIEmulation
        public static List<string> Types = new List<string>
        {
            _openAI_1,
            _LMS_llama_3_2_1b_instruct,
            _sample
        };

        public static string GetSettingsPath(string funcType)
        {
            switch (funcType)
            {
                case _sample: return string.Empty;
                case _openAI_1: return TTF_OpenAI_1.GetSettingsPath();
                case _LMS_llama_3_2_1b_instruct: return TTF_LMS_llama_3_2_1b_instruct.GetSettingsPath();
                default: return string.Empty;
            }
        }

        public static bool InitGlobal(TLog.eMode mode, string funcType, string fromCulture)
        {
            //called at the beginning of the translation task
            switch (funcType)
            {
                case _sample: return false;
                case _openAI_1: return TTF_OpenAI_1.InitGlobal(mode, fromCulture);
                case _LMS_llama_3_2_1b_instruct: return TTF_LMS_llama_3_2_1b_instruct.InitGlobal(mode, fromCulture);
                default: return false;
            }
        }

        public static bool InitPerCulture(TLog.eMode mode, string funcType, string fromCulture, string toCulture)
        {
            //called just before calling the first .Translate call for a culture ex: 'de-DE'
            switch (funcType)
            {
                case _sample: return false;
                case _openAI_1: return TTF_OpenAI_1.InitPerCulture(mode, fromCulture, toCulture); 
                case _LMS_llama_3_2_1b_instruct: return TTF_LMS_llama_3_2_1b_instruct.InitPerCulture(mode, fromCulture, toCulture);
                default: return false;
            }
        }

        public static string Translate(TLog.eMode mode, string funcType, string fromCulture, string toCulture, string textToTranslate, 
            string hintToken)
        {
            //called for each translateable item
            switch (funcType)
            {
                case _sample: return null;
                case _openAI_1: return TTF_OpenAI_1.Translate(mode, fromCulture, toCulture, textToTranslate, hintToken);
                case _LMS_llama_3_2_1b_instruct: return TTF_LMS_llama_3_2_1b_instruct.Translate(mode, fromCulture, toCulture, textToTranslate, hintToken);
                default: return null;
            }
        }

        public static bool DeInitGlobal(TLog.eMode mode, string funcType)
        {
            //called at the beginning of the translation task
            switch (funcType)
            {
                case _sample: return false;
                case _openAI_1: return TTF_OpenAI_1.DeInitGlobal(mode);
                case _LMS_llama_3_2_1b_instruct: return TTF_LMS_llama_3_2_1b_instruct.DeInitGlobal(mode);
                default: return false;
            }
        }

    }
}
