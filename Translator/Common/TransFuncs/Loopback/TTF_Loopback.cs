using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Net;

namespace Translator
{
    public static class TTF_Loopback
    {
        //just a simple sample translator that just prefixes a $ and returns that as the translated text.

        private static string _fromCulture = string.Empty;
        private static string _toCulture = string.Empty;
        private static string _logPrefix = "Loopback";
        private const string _settingsFilename = null;

        private static void Log(TLog.eMode mode, TLog.eLogItemType type, int indent, string msg)
        {
            TLog.Log(mode, type, indent, _logPrefix + ": " + msg);
        }

        public static bool InitGlobal(TLog.eMode mode, string fromCulture)
        {
            if (mode == TLog.eMode.translate)
            {
                Log(mode, TLog.eLogItemType.inf, 0, String.Format("InitGlobal: fromCulture={0}", _fromCulture));
                Log(mode, TLog.eLogItemType.inf, 0, "DESCRIPTION: Just returns the text to translate with a $ prefix.");
                Log(mode, TLog.eLogItemType.inf, 0, "DESCRIPTION: Use it to simulate receiving a translation from an API.");
                Log(mode, TLog.eLogItemType.inf, 0, "AUTHOR: Tetherscript");
                Log(mode, TLog.eLogItemType.inf, 0, "CONTACT: support@tetherscript.com");
            }
            return true;
        }

        public static bool InitPerCulture(TLog.eMode mode, string fromCulture, string toCulture)
        {
            return true;
        }

        public static bool DeInitGlobal(TLog.eMode mode)
        {
            return true;
        }

        public static string GetSettingsPath()
        {
            return null;
        }

        public static string Translate(TLog.eMode mode, string fromCulture, string toCulturestring, 
            string textToTranslate, string hintToken)
        {
            return "$" + textToTranslate;
        }
    }
}
