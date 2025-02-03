using System;
using System.IO;

namespace Translator
{
    public static class TLog
    {
        public enum eMode { scan, translate, tfTranslate, system }
        public enum eLogItemType { inf, err, dbg, sep };
        public enum eLogSeparatorType { lineWide, lineShort };

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

        public static void LogSeparator(eMode mode, eLogSeparatorType type)
        {
            string s = string.Empty;
            switch (type)
            {
                case eLogSeparatorType.lineWide: s = new string('━', 60); break;
                case eLogSeparatorType.lineShort: s = new string('━', 30); break;
            }
            Log(mode, eLogItemType.inf, 0, s);
        }

        public static void Log(eMode mode, eLogItemType logType, int indent, string msg)
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

        private static void IncLogCounter(eMode mode)
        {
            switch (mode)
            {
                case eMode.scan: _logScanCounter++; break;
                case eMode.translate: _logTranslateCounter++; break;
                case eMode.tfTranslate: _logTFCounter++; break;
                default: break;
            }
        }

        private static int GetLogCounter(eMode mode)
        {
            switch (mode)
            {
                case eMode.scan: return _logScanCounter;
                case eMode.translate: return _logTranslateCounter;
                case eMode.tfTranslate: return _logTFCounter;
                default: return -1;
            }
        }


        private static void Add(eMode mode, string msg)
        {
            if (mode == eMode.scan)
            {
                ScanText = ScanText + msg + Environment.NewLine;
            }
            else
            if (mode == eMode.translate)
            {
                TranslateText = TranslateText + msg + Environment.NewLine;
            }
            else
            if (mode == eMode.tfTranslate)
            {
                TfTranslateText = TfTranslateText + msg + Environment.NewLine;
            }
        }

        public static string ScanText = string.Empty;
        public static string TranslateText = string.Empty;
        public static string TfTranslateText = string.Empty;

        public static void Flush(eMode mode)
        {
            if (mode == eMode.scan)
            {
                App.Vm.ScanLog = ScanText;
            }
            else
            if (mode == eMode.translate)
            {
                App.Vm.TranslateLog = TranslateText;
            }
            else
            if (mode == eMode.tfTranslate)
            {
                //App.Vm.TFLog = App.Vm.TFLog + Environment.NewLine + TfTranslateText;
            }
        }

        public static void Reset(eMode mode)
        {
            if (mode == eMode.scan)
            {
                _logScanCounter = 0;
                App.Vm.ScanLog = "";
                ScanText = "";
            }
            else
            if (mode == eMode.translate)
            {
                _logTranslateCounter = 0;
                App.Vm.TranslateLog = "";
                TranslateText = "";
            }
            else
            if (mode == eMode.tfTranslate)
            {
                _logTFCounter = 0;
                App.Vm.TFLog = "";
                TfTranslateText = "";
            }
        }

        public static string GetText(eMode mode)
        {
            switch (mode)
            {
                case eMode.scan: return ScanText;
                case eMode.translate: return TranslateText;
                case eMode.tfTranslate: return TfTranslateText;
                default: return "";
            }
        }

        public static void Save(eMode mode, string path)
        {  
            if (mode != eMode.tfTranslate)
            {
                File.WriteAllText(path, GetText(mode));
            }
        }
    }
}
