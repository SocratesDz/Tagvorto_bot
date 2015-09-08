using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TagVorto_Bot
{
    public class Bot
    {
        private const String RSS_PATH = "http://eo.lernu.net/rss/tagovortoj.php";
        private const String TOKEN_API = "124472920:AAGN8w67IHoP-ddf_0smKGW1BSRmipbLTC4";
        private XmlDocument tagvortoDoc;
        private DateTime mostRecentCheck;
        private TimeSpan timeCheckSpan;
        private Item tagvorto;
        private Telegram.Bot.Api botApi;

        public Bot()
        {
            botApi = new Telegram.Bot.Api(TOKEN_API);
            mostRecentCheck = new DateTime();
            tagvortoDoc = new XmlDocument();
            tagvorto = null;
            GetXml();
        }

        public void GetXml()
        {
            WebRequest getRSS = WebRequest.Create(RSS_PATH);
            Stream response = getRSS.GetResponse().GetResponseStream();
            tagvortoDoc.Load(response);
        }

        public Item GetSingleWord()
        {
            if (!tagvortoDoc.HasChildNodes)
            {
                this.GetXml();
            }

            if (tagvorto == null)
            {
                var itemList = XMLItemParser.GetItemsFromDocument(ref tagvortoDoc);
                tagvorto = itemList.FirstOrDefault();
            }
            if (tagvorto != null)
            {
                if (tagvorto.PubDate.AddDays(1).CompareTo(DateTime.Now) == -1)
                {
                    this.GetXml();
                    var itemList = XMLItemParser.GetItemsFromDocument(ref tagvortoDoc);
                    tagvorto = itemList.FirstOrDefault();
                }
            }
            return tagvorto;
        }

        public void senduVorton(Telegram.Bot.Types.Update update)
        {
            var item = this.GetSingleWord();
            var vorto = String.Format("{0}\n\n{1}", item.Title, item.Description);

            botApi.SendTextMessage(update.Message.From.Id, vorto);
        }

        public void bonvenaMesagxo(Telegram.Bot.Types.Update update)
        {
            var bonvenon = "Estu bonvena! Mi estas la Tagvorta Roboto. \n" +
                "Mi sendos al vi la vorton de la tago. \n" +
                "Tajpu /vorto por ke mi sendu la vorton al vi!";

            botApi.SendTextMessage(update.Message.From.Id, bonvenon);
        }

        public void Run()
        {
            var me = botApi.GetMe().Result;

            Console.WriteLine(me.FirstName);

            int lastUpdateId = 0;

            while (true)
            {
                foreach (var update in botApi.GetUpdates(lastUpdateId + 1, 100).Result)
                {
                    lastUpdateId = update.Id;
                    Console.WriteLine(update.Message.Text);
                    if (!String.IsNullOrEmpty(update.Message.Text))
                    {
                        if (update.Message.Text.StartsWith("/vorto"))
                        {
                            this.senduVorton(update);
                        }
                        else if (update.Message.Text.StartsWith("/start"))
                        {
                            this.bonvenaMesagxo(update);
                        }
                        else if (update.Message.Text.StartsWith("/aboni"))
                        {
                            // TODO: Implementi la uzantan gardadon
                        }
                        else if (update.Message.Text.StartsWith("/malaboni"))
                        {
                            // TODO: Forvisxi la uzanton
                        }
                        else if (update.Message.Text.StartsWith("/stop"))
                        {
                            // TODO: Same kiel /malaboni
                        }
                        else if (update.Message.Text.Any())
                        {
                            // FARU NENION
                        }
                    }
                }
            }
        }
    }
}
