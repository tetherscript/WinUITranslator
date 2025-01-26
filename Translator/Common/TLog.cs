using System;
using System.IO;

namespace Translator
{
    public static class TLog
    {
        private static int _logCounter = 0;
        public static string Text = string.Empty;
        public static void Log(string msg, bool isError = false)
        {
            string s = string.Empty;
            if (isError)
            {
                s = ">>>>>> ";
            }
            s = s + String.Format(@"{0}{1}", msg, Environment.NewLine);
            Text = Text + s;
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
