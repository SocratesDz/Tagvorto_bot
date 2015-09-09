using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagVorto_Bot
{
    [Serializable]
    public class UserList
    {
        public UserList()
        {
            users = new List<int>();
        }
        public List<int> users { get; set; }
    }
}
