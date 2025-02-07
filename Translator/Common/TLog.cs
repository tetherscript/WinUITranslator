using System;
using System.IO;

namespace Translator
{
    public static class TLog
    {
        public enum eLogType { Scan, Translate, tfTranslate, system }
        public enum eLogItemType { inf, err, dbg, sep, sum, tra, wrn };
        public enum eLogSeparatorType { lineWide, lineShort };

        public static  string SepWide = new string ('━', 30);
        public static string SepNarrow = new string ('━', 15);

        private static int _logScanCounter = 0;
        private static int _logTranslateCounter = 0;
        private static int _logTFCounter = 0;
        private static int _errorCounter = 0;
        public static int ErrorCounter { 
            get
            {
                return _errorCounter;
            }
        }

        public static void LogSeparator(eLogType mode, eLogSeparatorType type)
        {
            string s = string.Empty;
            switch (type)
            {
                case eLogSeparatorType.lineWide: s = new string('━', 60); break;
                case eLogSeparatorType.lineShort: s = new string('━', 30); break;
            }
            Log(mode, eLogItemType.inf, 0, s);
        }

        public static void Log(eLogType mode, eLogItemType logType, int indent, string msg)
        {
            if ((logType == eLogItemType.dbg) && (!App.Vm.Debug))
            {
                return;
            }

            //line #
            IncLogCounter(mode);
            string lineNumber = GetLogCounter(mode).ToString("D4"); //0473

            //type
            string type = String.Format("[{0}]", logType.ToString()).ToUpper(); //[INF]

            //indent
            string ind = new string(' ', indent);

            //error
            string attn = " ";
            if (logType == eLogItemType.err)
            {
                attn = "E";
            }
            else
            if (logType == eLogItemType.dbg)
            {
                attn = "D";
            }

            //message
            string m = msg.Trim();

            string res = String.Format("{0}: {1} {2} {3}", lineNumber, attn, ind, m);

            Add(mode, res);
        }

        private static void IncLogCounter(eLogType mode)
        {
            switch (mode)
            {
                case eLogType.Scan: _logScanCounter++; break;
                case eLogType.Translate: _logTranslateCounter++; break;
                case eLogType.tfTranslate: _logTFCounter++; break;
                default: break;
            }
        }

        private static int GetLogCounter(eLogType mode)
        {
            switch (mode)
            {
                case eLogType.Scan: return _logScanCounter;
                case eLogType.Translate: return _logTranslateCounter;
                case eLogType.tfTranslate: return _logTFCounter;
                default: return -1;
            }
        }


        private static void Add(eLogType mode, string msg)
        {
            if (mode == eLogType.Scan)
            {
                ScanText = ScanText + msg + Environment.NewLine;
            }
            else
            if (mode == eLogType.Translate)
            {
                TranslateText = TranslateText + msg + Environment.NewLine;
            }
            else
            if (mode == eLogType.tfTranslate)
            {
                TfTranslateText = TfTranslateText + msg + Environment.NewLine;
            }
        }

        public static string ScanText = string.Empty;
        public static string TranslateText = string.Empty;
        public static string TfTranslateText = string.Empty;

        public static void Flush(eLogType mode)
        {
            if (mode == eLogType.Scan)
            {
                App.Vm.ScanLog = ScanText;
            }
            else
            if (mode == eLogType.Translate)
            {
                //App.Vm.TranslateLog = TranslateText;
            }
            else
            if (mode == eLogType.tfTranslate)
            {
                App.Vm.TFLog = App.Vm.TFLog + Environment.NewLine + TfTranslateText;
            }
        }

        public static void Reset(eLogType mode)
        {
            if (mode == eLogType.Scan)
            {
                _logScanCounter = 0;
                App.Vm.ScanLog = "";
                ScanText = "";
            }
            else
            if (mode == eLogType.Translate)
            {
                _logTranslateCounter = 0;
                //App.Vm.TranslateLog = "";
                TranslateText = "";
            }
            else
            if (mode == eLogType.tfTranslate)
            {
                _logTFCounter = 0;
                App.Vm.TFLog = "";
                TfTranslateText = "";
            }
        }

        public static string GetText(eLogType mode)
        {
            switch (mode)
            {
                case eLogType.Scan: return ScanText;
                case eLogType.Translate: return TranslateText;
                case eLogType.tfTranslate: return TfTranslateText;
                default: return "";
            }
        }

        public static void Save(eLogType mode, string path)
        {  
            if (mode != eLogType.tfTranslate)
            {
                File.WriteAllText(path, GetText(mode));
            }
        }
    }
}
