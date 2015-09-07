using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TagVorto_Bot
{
    class Program
    {
        private const String XML_PATH = "..\\..\\sample.xml";
        private const String TOKEN_API = "124472920:AAGN8w67IHoP-ddf_0smKGW1BSRmipbLTC4";

        static void Main(string[] args)
        {
            var itemList = XMLParser.Parse(XML_PATH);
            
            var Bot = new Telegram.Bot.Api(TOKEN_API);
            var me = Bot.GetMe().Result;

            Console.WriteLine(me.FirstName);

            int lastUpdateId = 0;

            while (true)
            {
                foreach (var update in Bot.GetUpdates(lastUpdateId+1, 100).Result)
                {
                    lastUpdateId = update.Id;
                    Console.WriteLine(update.Message.Text);
                    if (!String.IsNullOrEmpty(update.Message.Text))
                    {
                        if (update.Message.Text.StartsWith("/vorto"))
                        {
                            var item = itemList.FirstOrDefault();
                            var vorto = String.Format("{0}\n\n{1}", item.Title, item.Description);

                            Bot.SendTextMessage(update.Message.From.Id, vorto);
                        }
                    }
                }
            }

            Console.ReadKey();
        }
    }
}
