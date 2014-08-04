using ExitGames.Logging;
using MMO.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMO.PhotonFramework.Client
{
    public class PhotonClientHandlerList
    {
        protected readonly ILogger Log = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<int, PhotonClientHandler> _requestHandlerList;

        public PhotonClientHandlerList(IEnumerable<PhotonClientHandler> handlers)
        {
            _requestHandlerList = new Dictionary<int, PhotonClientHandler>();

            foreach (var handler in handlers)
            {
                if (!RegisterHandler(handler))
                {
                    Log.WarnFormat("Attempted to register handler {0} for tyope {1}:{2}", handler.GetType().Name, handler.Code);
                }
            }
        }

        public bool RegisterHandler(PhotonClientHandler handler)
        {
            var registered = false;

            if ((handler.Type & MessageType.Request) == MessageType.Request)
            {
                if (handler.SubCode.HasValue && !_requestHandlerList.ContainsKey(handler.SubCode.Value))
                {
                    _requestHandlerList.Add(handler.SubCode.Value, handler);
                    registered = true;
                }
                else if (!_requestHandlerList.ContainsKey(handler.Code))
                {
                    _requestHandlerList.Add(handler.Code, handler);
                    registered = true;
                }
                else
                {
                    Log.ErrorFormat("{0} list already contains handler for {1} - cannot add {2}", "RequestHandler", handler.Code, handler.GetType().Name);
                }
            }

            return registered;
        }

        public bool HandleMessage(IMessage message, PhotonClientPeer peer)
        {
            bool handled = false;

            switch (message.Type)
            {
                case MessageType.Request:
                    handled = CheckMessage(message, peer, _requestHandlerList);
                    break;
            }

            return handled;
        }

        private bool CheckMessage(IMessage message, PhotonClientPeer peer, Dictionary<int, PhotonClientHandler> list)
        {
            var handled = false;

            if (message.SubCode.HasValue && list.ContainsKey(message.SubCode.Value))
            {
                list[message.SubCode.Value].HandleMessage(message, peer);
                handled = true;
            }
            else if (!message.SubCode.HasValue && list.ContainsKey(message.Code))
            {
                list[message.Code].HandleMessage(message, peer);
                handled = true;
            }

            return handled;
        }
    }
}
