using System.Collections.Generic;

namespace Translator
{
    public static class TTransFunc_Sample
    {

        //A SAMPLE TRANSLATION FUNCTION
        public static string Translate(Dictionary<string, string> hints, string type, string text, string cultureName, string hintToken)
        {
            //put your code here to translate the text from en-US to culturename.  Use the hint to tweak your prompts.
            //let's just return a null, which means that the translationfailed.
            TLog.Log("MyTranslationFunction returned null");
            return null;
        }
        
    }
}
