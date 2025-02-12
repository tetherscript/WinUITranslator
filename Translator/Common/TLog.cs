namespace Translator
{
    public static class TLog
    {
        public enum eLogType { Scan, Translate, ProfileTest, system }
        public enum eLogItemType { inf, err, dbg, sep, sum, tra, wrn };

        public static  string SepWide = new string ('━', 30);

    }
}
