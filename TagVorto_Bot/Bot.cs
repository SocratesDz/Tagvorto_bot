using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TagVorto_Bot
{
    public class Bot
    {
        private const String RSS_PATH = "http://eo.lernu.net/rss/tagovortoj.php";
        private const String TOKEN_API = "124472920:AAGN8w67IHoP-ddf_0smKGW1BSRmipbLTC4";
        private const String USERS_FILE = "users.dat";

        private XmlDocument tagvortoDoc;
        private Item tagvorto;
        private Telegram.Bot.Api botApi;
        private UserList userList;

        public Bot()
        {
            botApi = new Telegram.Bot.Api(TOKEN_API);
            tagvortoDoc = new XmlDocument();
            tagvorto = this.GetSingleWord();
            userList = new UserList();
            userList = this.getUsersFromFile();
            this.GetXml();
        }

        #region Word Fetching

        private void GetXml()
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

        #endregion

        #region Word Sending

        private void senduVorton(Telegram.Bot.Types.Update update)
        {
            var item = this.GetSingleWord();
            if (item == null)
            {
                botApi.SendTextMessage(update.Message.Chat.Id, "Pardonu, mi ne povas sendi la vorton nun. Mi pardonpetas pro la ĝeno.");
            }
            else
            {
                var vorto = String.Format("{0}\n\n{1}", item.Title, item.Description);
                botApi.SendTextMessage(update.Message.Chat.Id, vorto);
            }
        }

        private void bonvenaMesagxo(Telegram.Bot.Types.Update update)
        {
            var bonvenon = "Estu bonvena! Mi estas la Tagvorta Roboto. \n" +
                "Mi sendos al vi la vorton de la tago. \n" +
                "Tajpu /vorto por ke mi sendu la vorton al vi!";

            botApi.SendTextMessage(update.Message.From.Id, bonvenon);
        }

        private void senduAbonitaMesagxo(Telegram.Bot.Types.Update update)
        {
            var abonitaMesagxo = "Dankon por aboni al ĉi tia servo. Vi ricevos ĉiutage la vorton de la tago.";
            botApi.SendTextMessage(update.Message.From.Id, abonitaMesagxo);
        }

        private void senduMalabonitaMesagxo(Telegram.Bot.Types.Update update)
        {
            var abonitaMesagxo = "Vi estas malabonita de ĉi tia servo.";
            botApi.SendTextMessage(update.Message.From.Id, abonitaMesagxo);
        }

        private void senduVortonAlAbonantoj()
        {
            var item = this.GetSingleWord();
            var vorto = String.Format("{0}\n\n{1}", item.Title, item.Description);

            foreach (var id in userList.users)
            {
                botApi.SendTextMessage(id, vorto);
            }
        }

        #endregion

        #region UserManagement

        private void createUsersFile()
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(USERS_FILE, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, userList);
            stream.Close();
        }

        private UserList getUsersFromFile()
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream;
            try
            {
                stream = new FileStream(USERS_FILE, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (System.IO.FileNotFoundException notFoundEx)
            {
                this.createUsersFile();
                stream = new FileStream(USERS_FILE, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            UserList list = (UserList)formatter.Deserialize(stream);
            stream.Close();

            return list;
        }

        private void saveNewUser(int userId)
        {
            this.userList.users.Add(userId);
            this.createUsersFile();
        }

        private void deleteUser(int userId)
        {
            this.userList.users.Remove(userId);
            this.createUsersFile();
        }

        #endregion

        #region Run

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
                            if (!this.userList.users.Contains(update.Message.From.Id))
                            {
                                this.saveNewUser(update.Message.From.Id);
                            }
                            this.senduAbonitaMesagxo(update);
                        }
                        else if (update.Message.Text.StartsWith("/malaboni"))
                        {
                            if (this.userList.users.Contains(update.Message.From.Id))
                            {
                                this.deleteUser(update.Message.From.Id);
                            }
                            this.senduMalabonitaMesagxo(update);
                        }
                        else if (update.Message.Text.StartsWith("/stop"))
                        {
                            // FARU NENION (EBLE /malaboni)
                        }
                        else if (update.Message.Text.Any())
                        {
                            // FARU NENION
                        }
                    }
                }

                if (tagvorto != null && tagvorto.PubDate.AddDays(1).CompareTo(DateTime.Now) == -1)
                {
                    this.senduVortonAlAbonantoj();
                }
            }
        }

        #endregion
    }
}
