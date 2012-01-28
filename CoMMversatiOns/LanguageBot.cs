/**
 *  CoMMversatiOns Project: API Tool for increase client functionalities of
 *  realXtend technologies.
 *  Copyright (C) 2010 Information Engineering Research Unit
 *  
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *  
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using OpenMetaverse;
using System.Xml.Linq;
using CoMMversatiOns.Bot;
using CoMMversatiOns.Events;

namespace CoMMversatiOns
{
    /// <summary>
    /// Bot that makes language-related operations with the user.
    /// </summary>
    class LanguageBot : BotBase
    {
        private Dictionary<string, int> knownUsers = new Dictionary<string, int>();

        private List<int> alreadyTranslatedPhrases = new List<int>();
        private List<int> alreadyAnalyzedPhrases = new List<int>();

        public override void Initialize()
        {
            base.Initialize();

            //Add some actions
            RegisterChatAction("dime", new EventHandler<ChatActionEventArgs>(TellMeHandler));
            RegisterChatAction("hola", new EventHandler<ChatActionEventArgs>(GreetingHandler));
            RegisterChatAction("analiza", new EventHandler<ChatActionEventArgs>(AnalyseHandler));
            RegisterChatAction("invierte", new EventHandler<ChatActionEventArgs>(ReverseHandler));
            RegisterChatAction("traduce", new EventHandler<ChatActionEventArgs>(TranslateHandler));
        }

        /// <summary>
        /// Handler for greeting message.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GreetingHandler(object sender, ChatActionEventArgs e)
        {
            if (knownUsers.ContainsKey(e.FromName))
                knownUsers[e.FromName]++;
            else
                knownUsers[e.FromName] = 1;

            if(knownUsers[e.FromName] == 1)
                Client.Self.Chat("Hola " + e.FromName + "! Encantado de conocerte :)", 0, ChatType.Whisper);
            else
                Client.Self.Chat("Hola de nuevo, " + e.FromName + "!", 0, ChatType.Whisper);
        }

        /// <summary>
        /// Handler for "reverse sentence" message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReverseHandler(object sender, ChatActionEventArgs e)
        {
            Client.Self.Chat("Voy a dar la vuelta a tu frase con una aplicación web. Conectando...", 0, ChatType.Whisper);

            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webClient_ReverseDownloadStringCompleted);
            webClient.DownloadStringAsync(new Uri("http://www.ieru.org/projects/mmol/reverse.php?phrase=" + Uri.EscapeUriString(e.Message.Replace("Invierte ", ""))));
        }

        /// <summary>
        /// Handler for "tell me" message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TellMeHandler(object sender, ChatActionEventArgs e)
        {
            Client.Self.Chat("Ahí iría un edificio del Camino de Santiago.", 0, ChatType.Whisper);
        }

        /// <summary>
        /// Handler for "analyse" message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnalyseHandler(object sender, ChatActionEventArgs e)
        {
            string phrase = e.Message.Replace("Analiza ", "").Replace("?", " ?").Replace(".", " .");

            if (alreadyAnalyzedPhrases.Contains(phrase.GetHashCode()))
            {
                Client.Self.Chat("¡Esa frase ya la analicé antes! Dime una frase nueva :)", 0, ChatType.Whisper);
            }else{
                Client.Self.Chat("Voy a analizar tu frase. Conectando...", 0, ChatType.Whisper);

                string language = "English";

                if (phrase.Contains(" es ") || phrase.Contains("Ese "))
                    language = "Spanish";

                WebClient webClient = new WebClient();
                webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webClient_OpenNLPDownloadStringCompleted);
                webClient.DownloadStringAsync(new Uri("http://localhost:8080/OpenSimNLP/OpenNLP?phrase=" + Uri.EscapeUriString(phrase) + "&language=" + language));

                alreadyAnalyzedPhrases.Add(phrase.GetHashCode());
            }
        }

        /// <summary>
        /// Handler for "translate" message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TranslateHandler(object sender, ChatActionEventArgs e)
        {
            string phrase = e.Message.Replace("Traduce ", "");

            if (alreadyTranslatedPhrases.Contains(phrase.GetHashCode()))
            {
                Client.Self.Chat("¡Esa frase ya la traduje antes! Dime una frase nueva :)", 0, ChatType.Whisper);
            }
            else
            {
                Client.Self.Chat("Voy a traducir tu frase. Conectando...", 0, ChatType.Whisper);

                WebClient webClient = new WebClient();
                webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webClient_TranslationDownloadStringCompleted);
                webClient.DownloadStringAsync(new Uri("http://api.microsofttranslator.com/V2/Http.svc/Translate?"
                    + "appId=5F1D586DEE19CCA530C52B5BA70F57800BAAC2F9"
                    + "&text=" + Uri.EscapeUriString(phrase)
                    + "&from=en"
                    + "&to=es"));

                alreadyTranslatedPhrases.Add(phrase.GetHashCode());
            }
        }

        /// <summary>
        /// Handler for reverse action completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void webClient_ReverseDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            Client.Self.Chat("Este es el resultado: \"" + e.Result + "\"", 0, ChatType.Whisper);
        }

        /// <summary>
        /// Handler for analyse action completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void webClient_OpenNLPDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null) return;

            //Client.Self.Chat("Este es el resultado:", 0, ChatType.Whisper);
            string trimmedResult = e.Result.Trim();
            string[] sentences = trimmedResult.Split('\n');
            foreach (string s in sentences)
                Client.Self.Chat(s, 0, ChatType.Whisper);

        }

        /// <summary>
        /// Handler for translate action completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void webClient_TranslationDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null) return;

            //Client.Self.Chat("Este es el resultado:", 0, ChatType.Whisper);
            XDocument doc = XDocument.Parse(e.Result);

            string t = doc.Root.Value;

            Client.Self.Chat(t, 0, ChatType.Whisper);
        }
    }
}
