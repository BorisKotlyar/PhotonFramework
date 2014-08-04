using MMO.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMO.PhotonFramework.Application
{
    class PhotonEvent : IMessage
    {
                private readonly byte _code;
        private readonly Dictionary<byte, object> _parameters;
        private readonly int? _subCode;

        public PhotonEvent(byte code, int? subCode, Dictionary<byte, object> parameters)
        {
            _code = code;
            _parameters = parameters;
            _subCode = subCode;
        }

        public byte Code
        {
            get { return _code; }
        }


        public MessageType Type
        {
            get { return MessageType.Async; }
        }

        public int? SubCode
        {
            get { return _subCode; }
        }

        public Dictionary<byte, object> Parameters
        {
            get { return _parameters; }
        }
    }

}
