using ExitGames.Logging;
using MMO.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMO.PhotonFramework.Server
{
    public  class PhotonServerHandlerList
    {
        private readonly DefaultRequestHandler   _defaultRequestHandler;
        private readonly DefaultResponseHandler  _defaultResponseHandler;
        private readonly DefaultEventHandler     _defaultEventHandler;

        private readonly Dictionary<int, PhotonServerHandler> _requestHandlerList;
        private readonly Dictionary<int, PhotonServerHandler> _responseHandlerList;
        private readonly Dictionary<int, PhotonServerHandler> _eventsHandlerList;

        protected readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public PhotonServerHandlerList(IEnumerable<IHandler<PhotonServerHandler>> handlers, DefaultRequestHandler defaultRequestHandler, DefaultResponseHandler defaultResponseHandler, DefaultEventHandler defaultEventHandler)
        {
            _defaultRequestHandler  = defaultRequestHandler;
            _defaultResponseHandler = defaultResponseHandler;
            _defaultEventHandler    = defaultEventHandler;

            _requestHandlerList  = new Dictionary<int, PhotonServerHandler>();
            _responseHandlerList = new Dictionary<int, PhotonServerHandler>();
            _eventsHandlerList   = new Dictionary<int, PhotonServerHandler>();

            foreach(PhotonServerHandler handler in handlers)
            {
                if (!RegisterHandler(handler))
                {
                    Log.WarnFormat("Attempted to register handler {0} for type {1}:{2}", handler.GetType().Name, handler.Type, handler.Code);
                }
            }
        }

        public bool RegisterHandler(PhotonServerHandler handler)
        {
            var registered = false;

            if (CheckHandler(handler, MessageType.Request, _requestHandlerList, "RequestHandler")) registered = true;
            if (CheckHandler(handler, MessageType.Response, _responseHandlerList, "ResponseHandler")) registered = true;
            if (CheckHandler(handler, MessageType.Async, _eventsHandlerList, "EventHandler")) registered = true;

            return registered;
        }

        public bool HandleMessage(IMessage message, PhotonServerPeer peer)
        {
            bool handled = false;

            switch (message.Type)
            {
                case MessageType.Request:
                    handled = CheckMessage(message, peer, _requestHandlerList, _defaultRequestHandler);
                    break;

                case MessageType.Response:
                    handled = CheckMessage(message, peer, _responseHandlerList, _defaultResponseHandler);
                    break;

                case MessageType.Async:
                    handled = CheckMessage(message, peer, _eventsHandlerList, _defaultEventHandler);
                    break;
            }

            return handled;
        }

        private bool CheckMessage(IMessage message, PhotonServerPeer peer, Dictionary<int, PhotonServerHandler> list, PhotonServerHandler defaultHandler)
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
            else
            {
                defaultHandler.HandleMessage(message, peer);
            }

            return handled;
        }

        private bool CheckHandler(PhotonServerHandler handler, MessageType type, Dictionary<int, PhotonServerHandler> list, string checkName)
        {
            var registered = false;

            if ((handler.Type & type) == type)
            {
                if (handler.SubCode.HasValue && !list.ContainsKey(handler.SubCode.Value))
                {
                    list.Add(handler.SubCode.Value, handler);
                    registered = true;
                }
                else if (!list.ContainsKey(handler.Code))
                {
                    list.Add(handler.Code, handler);
                    registered = true;
                }
                else
                {
                    Log.ErrorFormat("{0} list already contains handler for {1} - cannot add {2}", checkName, handler.Code, handler.GetType().Name);
                }
            }

            return registered;
        }

    }
}
