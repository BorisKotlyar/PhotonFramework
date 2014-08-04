using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MMO.Framework;
using MMO.PhotonFramework.Client;
using MMO.PhotonFramework.Server;
using ExitGames.Logging;
using Photon.SocketServer;

namespace MMO.PhotonFramework.Application
{
    public abstract class PhotonConnectionCollection : IConnectionCollection<PhotonServerPeer, PhotonClientPeer>
    {
        protected readonly ILogger Log = LogManager.GetCurrentClassLogger();
        
        public Dictionary<Guid, PhotonClientPeer> Clients { get; protected set; }
        public Dictionary<Guid, PhotonServerPeer> Servers { get; protected set; }

        public PhotonConnectionCollection()
        {
            Servers = new Dictionary<Guid, PhotonServerPeer>();
            Clients = new Dictionary<Guid, PhotonClientPeer>();
        }

        public void OnConnect(PhotonServerPeer serverPeer)
        {
            if (!serverPeer.ServerId.HasValue)
            {
                throw new InvalidOperationException("server id cannot be null");
            }

            Guid id = serverPeer.ServerId.Value;

            lock(this)
            {
                PhotonServerPeer peer;
                if (Servers.TryGetValue(id, out peer))
                {
                    peer.Disconnect();
                    Servers.Remove(id);
                    Disconnect(peer);
                }

                Servers.Add(id, serverPeer);
                Log.Warn("Sending to connect");
                Connect(serverPeer);

                ResetServers();
            }
        }

        public void OnDisconnect(PhotonServerPeer serverPeer)
        {
            if (!serverPeer.ServerId.HasValue)
            {
                Disconnect(serverPeer);
                throw new InvalidOperationException("server id cannot be null");
            }

            lock(this)
            {
                PhotonServerPeer peer;
                if (serverPeer.ServerId.HasValue)
                {
                    Guid id = serverPeer.ServerId.Value;
                    if (!Servers.TryGetValue(id, out peer)) return;
                    if (peer == serverPeer)
                    {
                        Servers.Remove(id);
                        Disconnect(peer);
                        ResetServers();
                    }
                }
            }
        }

        public void OnClientConnect(PhotonClientPeer clientPeer)
        {
            ClientConnection(clientPeer);
            Clients.Add(clientPeer.PeerId, clientPeer);
        }

        public void OnClientDisconnect(PhotonClientPeer clientPeer)
        {
            ClientDisconnect(clientPeer);
            Clients.Remove(clientPeer.PeerId);
        }

        public PhotonServerPeer GetServerByType(int serverType)
        {
            return OnGetServerByType(serverType);
        }

        public abstract void Disconnect(PhotonServerPeer serverPeer);
        public abstract void Connect(PhotonServerPeer serverPeer);
        public abstract void ClientConnection(PhotonClientPeer clientPeer);
        public abstract void ClientDisconnect(PhotonClientPeer clientPeer);

        public abstract void ResetServers();

        public abstract bool IsServerPeer(InitRequest initRequest);
        public abstract PhotonServerPeer OnGetServerByType(int serverType);
    }
}
