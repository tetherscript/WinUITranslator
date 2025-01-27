using System.Collections.Generic;
using System.ComponentModel;

namespace Translator
{
    public static class TTransFunc
    {
        public static List<string> Types = new List<string>
        {
            "OpenAI_1",
            "Sample"
        };

        public static string GetDescription(string funcType)
        {
            if (funcType == "Sample")
            {

            }
            if (funcType == "OpenAI_1")
            {
                return TTF_OpenAI_1.Description(funcType).Trim();
            }
            else
                return null;
        }

        public static bool InitGlobal(string funcType, string fromCulture)
        {
            //called at the beginning of the translation task
            //called only once, so you can load settings and perhaps prepare an assistant?
            if (funcType == "Sample")
            {

            }
            if (funcType == "OpenAI_1")
            {
                return TTF_OpenAI_1.InitGlobal(fromCulture);
            }
            else
                return false;
        }

        public static bool InitPerCulture(string funcType, string fromCulture, string toCulture)
        {
            //called just before calling the first .Translate call for a culture ex: 'de-DE'
            if (funcType == "Sample")
            {

            }
            if (funcType == "OpenAI_1")
            {
                return TTF_OpenAI_1.InitPerCulture(fromCulture, toCulture);
            }
            else
                return false;
        }

        public static string Translate(string funcType, string fromCulture, string toCulture, string textToTranslate, 
            string hintToken)
        {
            //called for each translateable item
            if (funcType == "Sample")
            {

            }
            if (funcType == "OpenAI_1")
            {
                return TTF_OpenAI_1.Translate(fromCulture, toCulture, textToTranslate, hintToken);
            }
            else
                return null;
        }

    }
}
