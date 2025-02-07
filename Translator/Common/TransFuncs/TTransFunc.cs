using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace Translator
{
    public static class TTransFunc
    {
        private const string _loopback = "loopback";
        private const string _openaiapi = "openai-api";

        public static bool InitGlobal(TLog.eLogType mode, string funcType, string fromCulture)
        {
            //called at the beginning of the translation task
            switch (funcType)
            {
                case _loopback: return TTF_Loopback.InitGlobal(mode, fromCulture);
                case _openaiapi: return TTF_OpenAI_1.InitGlobal(mode, fromCulture);
                default: return false;
            }
        }

        public static bool InitPerCulture(TLog.eLogType mode, string funcType, string fromCulture, string toCulture)
        {
            //called just before calling the first .Translate call for a culture ex: 'de-DE'
            switch (funcType)
            {
                case _loopback: return TTF_Loopback.InitPerCulture(mode, fromCulture, toCulture);
                case _openaiapi: return TTF_OpenAI_1.InitPerCulture(mode, fromCulture, toCulture); 
                default: return false;
            }
        }

        public static string Translate(TLog.eLogType mode, string funcType, string fromCulture, string toCulture, string textToTranslate, 
            string hintToken)
        {
            //called for each translateable item
            switch (funcType)
            {
                case _loopback: return TTF_Loopback.Translate(mode, fromCulture, toCulture, textToTranslate, hintToken);
                case _openaiapi: return TTF_OpenAI_1.Translate(mode, fromCulture, toCulture, textToTranslate, hintToken);
                default: return null;
            }
        }

        public static bool DeInitGlobal(TLog.eLogType mode, string funcType)
        {
            //called at the beginning of the translation task
            switch (funcType)
            {
                case _loopback: return TTF_Loopback.DeInitGlobal(mode);
                case _openaiapi: return TTF_OpenAI_1.DeInitGlobal(mode);
                default: return false;
            }
        }

    }
}

/*
ChatGPT 4
You are a professional translator who translates from en-US to strict de-DE.  Always respond in JSON format and name your text value 'response' and give a confidence rating 1-100 and name it's integer value 'confidence'. Place your reasoning text in the 'reasoning' property. Do the translation in the context of the text displayed on a photography software user interface.


LM STUDIO
llama-3.2-1b-instruct
You are a professional translator who translates from en-US to strict de-DE.  Always respond in JSON format and place your translated text the the 'translated' property and your confidence 1-100 integer in the 'confidence' property.  Do the translation in the context of the text displayed on a photography software user interface.

DeepSeek R1 Distill Llama 8B
You are a professional translator who translates from en-US to strict de-DE.  Always respond in JSON format and place your translated text the the 'translated' property and your confidence 1-100 integer in the 'confidence' property.  Do the translation in the context of the text displayed on a photography software user interface.
*/
