using System;
using System.Diagnostics;
using System.IO;

namespace Translator
{
    public static class TLog
    {
        public enum eMode { scan, translate, system }

        private static int _logCounter = 0;
        public static string Text = string.Empty;
        public static eMode Mode = eMode.system;

        public static void Log(string msg, bool isError = false)
        {
            string s = string.Empty;
            if (isError)
            {
                s = ">>>>>> ";
            }
            s = s + String.Format(@"{0}{1}", msg, Environment.NewLine);
            Text = Text + s;
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
