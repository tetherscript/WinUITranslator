using System.Collections.Generic;

namespace Translator
{
    public static class TTransFunc
    {
        public static List<string> Types = new List<string>
        {
            "OpenAI_API",
            "MyTranslationFunction"
        };

        public static string Translate(Dictionary<string, string> hints, string type, string text, string cultureName, string hint)
        {
            //translationFunction = "OpenAI_API";
            if (type == "OpenAI_API")
            {
                return TTransFunc_OpenAI.Translate(hints, type, text, cultureName, hint);
            }
            else if (type == "MyTranslationFunction")
            {
                //call your translation function here to your custom translation calling code
                return TTransFunc_Sample.Translate(hints, type, text, cultureName, hint);
            }
            else
            {
                //Log("Translation Function is unknown: " + translationFunction);
                return null;
            }
        }
    }
}
