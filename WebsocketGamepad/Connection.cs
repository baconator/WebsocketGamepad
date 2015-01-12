using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebsocketGamepad
{
    public class Connection
    {
        public string UserId;
        public string Address;
        public bool Active;
        public bool Banned;

        public override string ToString()
        {
            return Active.ToString() + "\t" + Address + "\t" + UserId;
        }

        public override bool Equals(object obj)
        {
            return UserId == ((Connection)obj).UserId;
        }
    }
}
