using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Translator
{
    public static class TTransFunc_OpenAI
    {
        //OPENAI API TRANSLATION FUNCTION
        public static string Translate(Dictionary<string, string> hints, string type, string text, string cultureName, string hintToken)
        {
            //returning null means this function has failed. It could be due to a bad return data structure.
            
            //However, the API may not understand the question and returns something like a confused 'I do not understand',
            //which is a valid return data structure, but a failed translation.  In this OpenAI API,
            //it would do that if we send None ....so we add single quotes going to, and remove the quotes on return.

            //Worst case is some obscure string has this confused bad translation that is rarely seen by users.
            //Best thing to do is to review the Translate log where you can see the API responses for translations.

            //this Translation Function is the original one used on this translator app.  It is not optimized.
            string openAiApiKey = Environment.GetEnvironmentVariable("OpenAIAPIKey1");

            const string openAiChatEndpoint = "https://api.openai.com/v1/chat/completions";

            using HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);

            string systemContent = string.Empty;
            string userContent = string.Empty;

            systemContent = String.Format(hints[type + ":" + hintToken], cultureName);
            userContent = "'" + text + "'"; //because sending None to the API returns a 'please specify the string response...'

            var requestBody = new
            {
                model = "gpt-4",
                messages = new[]
                {
                    new { role = "developer", content = systemContent},
                    new { role = "user", content = userContent}
                    },
                temperature = 0.0,
                max_tokens = 1000
            };

            var requestContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            using HttpResponseMessage response = httpClient.PostAsync(openAiChatEndpoint, requestContent).Result;
            response.EnsureSuccessStatusCode();

            string jsonResponse = response.Content.ReadAsStringAsync().Result;

            // The response JSON includes an array of "choices"; we want the "message.content"
            // of the first choice. For structure details, see:
            // https://platform.openai.com/docs/guides/chat/introduction
            try
            {
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                JsonElement root = doc.RootElement;
                JsonElement choices = root.GetProperty("choices");
                JsonElement firstChoice = choices[0];
                JsonElement message = firstChoice.GetProperty("message");
                string translatedText = message.GetProperty("content").GetString() ?? string.Empty;

                //remove the quotes
                translatedText = translatedText.Substring(1, translatedText.Length - 2);

                return translatedText.Trim();
            }
            catch
            {
                // Fallback in case of any unexpected structure
                TLog.Log("Bad API return structure.", true);
                return null;
            }
        }
    }
}
