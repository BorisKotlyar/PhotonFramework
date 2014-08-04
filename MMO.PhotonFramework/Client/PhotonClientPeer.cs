using ExitGames.Logging;
using MMO.Framework;
using MMO.PhotonFramework.Application;
using MMO.PhotonFramework.Server;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMO.PhotonFramework.Client
{
    public class PhotonClientPeer : PeerBase
    {
        protected ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly Guid _peerID;
        private readonly Dictionary<Type, ClientData> _clientData = new Dictionary<Type, ClientData>();
        private readonly PhotonApplication _server;
        private readonly PhotonClientHandlerList _handlerList;

        public PhotonServerPeer CurrentServer { get; set; }

        #region Factory Method

        public delegate PhotonClientPeer Factory(InitRequest initRequest);

        #endregion

        public PhotonClientPeer(InitRequest request, IEnumerable<ClientData> clientData, PhotonClientHandlerList handlerList, PhotonApplication application) : base (request.Protocol, request.PhotonPeer)
        {
            _peerID = Guid.NewGuid();
            _handlerList = handlerList;
            _server = application;

            foreach (var data in clientData)
            {
                _clientData.Add(data.GetType(), data);
            }

            _server.ConnectionCollection.Clients.Add(_peerID, this);
        }

        protected override void OnDisconnect(PhotonHostRuntimeInterfaces.DisconnectReason reasonCode, string reasonDetail)
        {
            _server.ConnectionCollection.OnClientDisconnect(this);
            Log.DebugFormat("Client {0} disconnected", _peerID);
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            _handlerList.HandleMessage(
                new PhotonRequest(
                    operationRequest.OperationCode,
                    operationRequest.Parameters.ContainsKey(_server.SubCodeParameterKey) ? (int?)Convert.ToInt32(operationRequest.Parameters[_server.SubCodeParameterKey]) : null,
                    operationRequest.Parameters),
                this);
        }

        public Guid PeerId
        {
            get { return _peerID; }
        }

        public T ClientData<T>() where T : ClientData
        {
            ClientData result;
            _clientData.TryGetValue(typeof(T), out result);
            if (result != null)
            {
                return result as T;
            }

            return null;
        }
    }
}
