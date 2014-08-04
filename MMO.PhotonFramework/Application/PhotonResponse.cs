using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMO.Framework;

namespace MMO.PhotonFramework
{
    public class PhotonResponse : IMessage
    {
        private readonly byte _code;
        private readonly Dictionary<byte, object> _parameters;
        private readonly int? _subCode;
        private readonly string _debugMessage;
        private readonly short _returnCode;

        public PhotonResponse(byte code, int? subCode, Dictionary<byte, object> parameters, string debugMessage, short returnCode)
        {
            _code = code;
            _parameters = parameters;
            _subCode = subCode;
            _debugMessage = debugMessage;
            _returnCode = returnCode;
        }

        public byte Code
        {
            get { return _code; }
        }

        public string DebugMessage
        {
            get { return _debugMessage; }
        }

        public MessageType Type
        {
            get { return MessageType.Response; }
        }

        public short ReturnCode
        {
            get { return _returnCode; }
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
