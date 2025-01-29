using System;
using System.Diagnostics;
using System.IO;

namespace Translator
{
    public static class TLog
    {
        public enum eMode { scan, translate, system }
        public enum eLogType { inf, err, dbg, sep };

        private static int _logCounter = 0;
        private static int _errorCounter = 0;
        public static int ErrorCounter { 
            get
            {
                return _errorCounter;
            }
        }

        public static string TsDown = new string('▼', 30);
        public static string TsNeutral = new string('━', 60);
        public static string TsUp = new string('▲', 30);

        public static string Text = string.Empty;
        public static eMode Mode = eMode.system;


        public static void Log(eLogType logType, int indent, string msg)
        {
            //line #
            string lineNumber = _logCounter.ToString("D4"); //0473

            //type
            string type = String.Format("[{0}]", logType.ToString()).ToUpper(); //[INF]

            //indent
            string ind = new string(' ', indent);

            //message
            string m = msg.Trim();

            string res = String.Format("{0}: {1} {2}", lineNumber, type, m);

            Text = res + Environment.NewLine + Text;

            if (Mode == eMode.scan)
            {
                App.Vm.ScanLog = Text;
            }
            else
            if (Mode == eMode.translate)
            {
                App.Vm.TranslateLog = Text;
            }
            _logCounter++;
        }

        public static void Reset()
        {
            _logCounter = 0;
            _errorCounter = 0;
            Text = "";
        }

        public static void ClearAllFiles()
        {
            File.WriteAllText(TUtils.TargetTranslateLogPath, "");
            File.WriteAllText(TUtils.TargetScanLogPath, "");
        }

        public static void Save(string path)
        {
            File.WriteAllText(path, Text);
        }
    }
}
