using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adapter.EverlinkProtocol.Messages
{
    public class LoginMessage : MessageBase
    {
        public LoginMessage(string username, string password)
        {
            Prefix = new byte[]{};
            Data = new List<byte>(Encoding.UTF8.GetBytes( $"{username}\0{password}\0")).ToArray();
            Suffix = new byte[]{};
        }
        public LoginMessage(byte[] message) : base()
        {
            throw new NotImplementedException();
        }
    }
}