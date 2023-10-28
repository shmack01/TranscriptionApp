/*
 * Sample Code is provided for the purpose of illustration only and is not intended to be used in a 
 * production environment. THIS SAMPLE CODE AND ANY RELATED INFORMATION ARE PROVIDED "AS IS" WITHOUT 
 * WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
 * We grant You a nonexclusive, royalty-free right to use and modify the Sample Code and to 
 * reproduce and distribute the object code form of the Sample Code, provided that. 
 * You agree: (i) to not use Our name, logo, or trademarks to market 
 * Your software product in which the Sample Code is embedded; (ii) to include a 
 * valid copyright notice on Your software product in which the Sample Code is 
 * embedded; and (iii) to indemnify, hold harmless, and defend Us and 
 * Our suppliers from and against any claims or lawsuits, including attorneys’ fees, 
 * that arise or result from the use or distribution of the Sample Code
 * 
 *  Copyright (c) Microsoft. All rights reserved.
 *  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
 *  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
 *  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
 *  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Newtonsoft.Json;

namespace TranscriptionApp.Services
{

    class Translate
    {
        private static bool AutoDetect { get; set; }
        //TODO: Modify to use more secure method
        // https://docs.microsoft.com/azure/cognitive-services/translator/reference/v3-0-translate
        private static string route = "/translate?api-version=3.0&{0}&to={1}";
        private const string fromLanguage = "from={0}";
        private const string region_var = "TRANSLATOR_SERVICE_REGION";
        private const string key_var = "TRANSLATOR_TEXT_RESOURCE_KEY";
        private const string endpoint_var = "TRANSLATOR_TEXT_ENDPOINT";
        private static readonly string region = Environment.GetEnvironmentVariable(region_var);
        private static readonly string resourceKey = Environment.GetEnvironmentVariable(key_var);
        private static readonly string endpoint = Environment.GetEnvironmentVariable(endpoint_var);




        static Translate()
        {
            AutoDetect = false; //Default
            if (null == region)
            {
                throw new Exception("Please set/export the environment variable: " + region_var);
            }
            if (null == resourceKey)
            {
                throw new Exception("Please set/export the environment variable: " + key_var);
            }
            if (null == endpoint)
            {
                throw new Exception("Please set/export the environment variable: " + endpoint_var);
            }

        }

        // Async call to the Translator Text API
        static public async Task<List<string>> TranslateTextRequest(string inputText, string fromLang, string toLang, bool autoDetect)
        {

            List<string> results = new List<string>();
            if (String.IsNullOrEmpty(inputText)) { return results; };
            string routesb = String.Empty;
            object[] body = new object[] { new { Text = inputText } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                // Build the request.
                request.Method = HttpMethod.Post;
                if (autoDetect)
                {
                    routesb = String.Format(route, "", toLang);//leave the "from" empty
                }
                else
                {
                    string from = String.Format(fromLanguage, fromLang);
                    routesb = String.Format(route, from, toLang);
                }
                request.RequestUri = new Uri(endpoint + routesb);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", resourceKey);
                request.Headers.Add("Ocp-Apim-Subscription-Region", region);

                // Send the request and get response.
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                // Read response as a string.
                string result = await response.Content.ReadAsStringAsync();
                TranslationResult[] deserializedOutput = JsonConvert.DeserializeObject<TranslationResult[]>(result);
                // Iterate over the deserialized results.
                foreach (TranslationResult o in deserializedOutput)
                {
                    // Print the detected input language and confidence score.
                    //Console.WriteLine("Detected input language: {0}\nConfidence score: {1}\n", o.DetectedLanguage.Language, o.DetectedLanguage.Score);
                    // Iterate over the results and print each translation.
                    foreach (Translation t in o.Translations)
                    {
                        Console.WriteLine("Translated to {0}: {1}", t.To, t.Text);
                        results.Add(t.Text);
                    }
                }
            }
            return results;
        }



    }
    public class TranslationResult
    {
        public DetectedLanguage DetectedLanguage { get; set; }
        public TextResult SourceText { get; set; }
        public Translation[] Translations { get; set; }
    }
    public partial class LanguageResult
    {
        [JsonProperty("translation")]
        public Dictionary<string, Language> Translation { get; set; }
    }

    public class Language
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        public string ShortName { get; set; }
    }

    public class DetectedLanguage
    {
        public string Language { get; set; }
        public float Score { get; set; }
    }

    public class TextResult
    {
        public string Text { get; set; }
        public string Script { get; set; }
    }

    public class Translation
    {
        public string Text { get; set; }
        public TextResult Transliteration { get; set; }
        public string To { get; set; }
        public Alignment Alignment { get; set; }
        public SentenceLength SentLen { get; set; }
    }

    public class Alignment
    {
        public string Proj { get; set; }
    }

    public class SentenceLength
    {
        public int[] SrcSentLen { get; set; }
        public int[] TransSentLen { get; set; }
    }
}
