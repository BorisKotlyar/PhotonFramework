using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMO.Framework
{
    public interface IHandler<T>
    {
        MessageType Type { get; }
        byte Code { get; }
        int? SubCode { get; }
        bool HandleMessage(IMessage message, T peer);
    }
}
