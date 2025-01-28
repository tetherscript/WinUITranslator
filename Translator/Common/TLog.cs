using System;
using System.Diagnostics;
using System.IO;

namespace Translator
{
    public static class TLog
    {
        public enum eMode { scan, translate, system }

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

        public static void Log(string msg, bool isError = false)
        {
            if (isError)
            {
                Text = Text + TsDown + Environment.NewLine;
                Text = Text + "ERROR" + Environment.NewLine;
                _errorCounter++;
            }
            Text = Text + String.Format(@"{0}{1}", msg, Environment.NewLine);
            if (isError)
            {
                Text = Text + TsUp + Environment.NewLine;
            }
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

        public static void LogInsert(string msg, bool isError = false)
        {
            string s = string.Empty;
            if (isError)
            {
                s = ">>>>>> ";
                _errorCounter++;

            }
            s = s + String.Format(@"{0}{1}", msg, Environment.NewLine);
            Text = s + Text;
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
