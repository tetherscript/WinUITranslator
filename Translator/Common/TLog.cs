using System;
using System.IO;

namespace Translator
{
    public static class TLog
    {
        public enum eLogType { Scan, Translate, ProfileTest, system }
        public enum eLogItemType { inf, err, dbg, sep, sum, tra, wrn };
        public enum eLogSeparatorType { lineWide, lineShort };

        public static  string SepWide = new string ('━', 30);
        public static string SepNarrow = new string ('━', 15);







    }
}
