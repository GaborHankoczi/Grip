using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adapter.EverlinkProtocol.Messages
{
    public class LoginResultMessage : MessageBase
    {
        public bool IsSuccess { get; private set; }
        public LoginResultMessage(byte[] message)
        {
            if(Encoding.UTF8.GetString(message).Trim('\0')=="+")
                IsSuccess = true;
            else
                IsSuccess = false;
        }
        
    }
}