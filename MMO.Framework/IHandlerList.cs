﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMO.Framework
{
    public interface IHandlerList<T>
    {
        bool RegisterHandler(IHandler<T> handler);
        bool HandleMessage(IMessage message, T peer);
    }
}
