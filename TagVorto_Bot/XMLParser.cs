using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TagVorto_Bot
{
    public static class XMLParser
    {
        public static List<Item> Parse(String docUrl)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(docUrl);

            if (doc.FirstChild.NodeType == XmlNodeType.XmlDeclaration)
            {
                XmlDeclaration xmlDecl = (XmlDeclaration)doc.FirstChild;
                xmlDecl.Encoding = "ISO-8859-3";
            }

            var itemList = GetItems(ref doc);
            return itemList;
        }


        private static List<Item> GetItems(ref XmlDocument doc)
        {
            List<Item> items = new List<Item>();

            XmlNodeList ItemList = doc.SelectNodes("//channel//item");
            foreach (XmlNode node in ItemList)
            {
                Item item = new Item();
                var title = node.SelectSingleNode("title");
                item.Title = title.InnerText;

                var link = node.SelectSingleNode("link");
                item.Link = link.InnerText;

                var desc = node.SelectSingleNode("description");
                desc.InnerText = desc.InnerText.Replace("<br />", "");
                item.Description = desc.InnerText;

                var pubDate = node.SelectSingleNode("pubDate");
                item.PubDate = DateTime.Parse(pubDate.InnerText);

                items.Add(item);
            }

            return items;
        }
    }
}
