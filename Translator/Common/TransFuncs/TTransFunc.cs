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
        private const string _loopback = "Loopback";
        private const string _openAI_1 = "OpenAI_1";
        private const string _LMS_llama_3_2_1b_instruct = "LMS_llama-3.2-1b-instruct";

        public static List<string> Types = new List<string>
        {
            _openAI_1,
            _LMS_llama_3_2_1b_instruct,
            _loopback
        };

        public static void LoadSettingsPage(string funcType)
        {
            switch (funcType)
            {
                case _loopback: WeakReferenceMessenger.Default.Send(new NavigateMessage(typeof(TTF_Loopback_SettingsPage))); break;
                case _openAI_1: WeakReferenceMessenger.Default.Send(new NavigateMessage(typeof(TTF_OpenAI_1_SettingsPage))); break;
                case _LMS_llama_3_2_1b_instruct: WeakReferenceMessenger.Default.Send(new NavigateMessage(typeof(TTF_LMS_llama_3_2_1b_instruct_SettingsPage))); break;
                default:break;
            }
        }

        public static string GetSettingsPath(string funcType)
        {
            switch (funcType)
            {
                case _loopback: return TTF_Loopback.GetSettingsPath();
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
                case _loopback: return TTF_Loopback.InitGlobal(mode, fromCulture);
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
                case _loopback: return TTF_Loopback.InitPerCulture(mode, fromCulture, toCulture);
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
                case _loopback: return TTF_Loopback.Translate(mode, fromCulture, toCulture, textToTranslate, hintToken);
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
                case _loopback: return TTF_Loopback.DeInitGlobal(mode);
                case _openAI_1: return TTF_OpenAI_1.DeInitGlobal(mode);
                case _LMS_llama_3_2_1b_instruct: return TTF_LMS_llama_3_2_1b_instruct.DeInitGlobal(mode);
                default: return false;
            }
        }

    }
}
