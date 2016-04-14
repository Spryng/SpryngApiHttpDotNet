using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spryng
{
    public class SpryngClientException : Exception
    {
        public int Code { get; private set; }

        public SpryngClientException(int code)
        {
            Code = code;
        }

        public override string Message
        {
            get
            {
                switch(Code)
                {
                    case 100:
                        return "Missing Parameter";
                    case 101:
                        return "username too short";
                    case 102:
                        return "username too long";
                    case 103:
                        return "password too short";
                    case 104:
                        return "password too long";
                    case 105:
                        return "destination too short";
                    case 106:
                        return "destination too long";
                    case 107:
                        return "sender too long";
                    case 108:
                        return "sender too short";
                    case 109:
                        return "body too short";
                    case 110:
                        return "body too long";
                    case 200:
                        return "Security Error";
                    case 201:
                        return "Unknown route";
                    case 202:
                        return "Route access violation";
                    case 203:
                        return "Insufficient credits";
                    case 800:
                        return "Technical Error";
                    default:
                        return "Unknown Spryng Error.";
                }
            }
        }
    }
}
