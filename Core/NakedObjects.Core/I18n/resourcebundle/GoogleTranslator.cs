﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Common.Logging;

namespace NakedObjects.Reflect.I18n.Resourcebundle {
    public class GoogleTranslator : ITranslator {
        private const string GOOGLE = "http://ajax.googleapis.com/ajax/services/language/translate?v=1.0&";
        private static readonly ILog Log = LogManager.GetLogger(typeof (GoogleTranslator));

        private readonly string sourceLanguage;
        private readonly string translatedLanguage;

        public GoogleTranslator(string sourceLanguage, string translatedLanguage) {
            this.sourceLanguage = sourceLanguage;
            this.translatedLanguage = translatedLanguage;
        }

        #region ITranslator Members

        public string Translate(string phrase) {
            if (phrase.Trim().Length == 0) {
                return "";
            }

            phrase = phrase.Substring(0, 1) + phrase.Substring(1).ToLower();

            // create the web request to the Google Translate REST interface

            string ip = GetIP();
            string userip = ip == null ? "" : "&userip=" + ip;

            var oRequest = (HttpWebRequest) WebRequest.Create(GOOGLE + "q=" + HttpUtility.UrlEncode(phrase) + "&langpair=" + sourceLanguage + "%7C" + translatedLanguage + userip);
            oRequest.Referer = @"http://www.nakedobjects.net";

            WebResponse oResponse = oRequest.GetResponse();
            var oReader = new StreamReader(oResponse.GetResponseStream());
            string page = oReader.ReadToEnd();

            var reg = new Regex("\":\"(.*?)\"}, ");
            MatchCollection matches = reg.Matches(page);
            string translated;
            if (matches.Count != 1 || matches[0].Groups.Count != 2) {
                translated = "*" + phrase;

                var regError = new Regex(@".*""responseDetails"":(.*),.*");
                MatchCollection matchesError = regError.Matches(page);
                string warning = matchesError.Count > 0 ? matchesError[0].Groups[1].Value : "Not Found";
                Log.WarnFormat("Translator Response {0}", warning);
            }
            else {
                translated = matches[0].Groups[1].Value;
            }
            Log.Debug("Translated '" + phrase + "' as '" + translated + "'");
            return translated;
        }

        public string TranslatedLanguage {
            get { return translatedLanguage; }
        }

        #endregion

        private string GetIP() {
            IPAddress[] a = Dns.GetHostAddresses(Dns.GetHostName());
            return a.Any() ? a.First().ToString() : null;
        }
    }
}